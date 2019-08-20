using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Config;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using Limilabs.Mail.Headers;
using Limilabs.Mail.MIME;
using Models;
using MailDll;
using MailStorage;
using NLog;

namespace MailModule
{
    /// <summary>
    /// Mail controller every 30s queries mail server for new email.
    /// When there are new emails available it first download all mail headers and publishes them to the stream.
    /// Afterwards it starts downloading all mail content for just downloaded mail headers.
    /// If connection will be interrupted and download of mail header will be stopped.
    /// Mail controller will attempt to reconnect and download mail headers once again.
    /// Streams can be observed, clients can subscribe to them and react on incoming mail data..
    /// </summary>
    public class MailController : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private string _host;
        private string _user;
        private string _password;
        private MailServerType _serverType;
        private MailServerEncryption _serverEncryption;
        private CancellationTokenSource _cancelSource;
        private readonly Func<IMailStorage<MailHeaderEntity>> _mailHeaderStorageFactory;
        private readonly Func<IMailStorage<MailContentEntity>> _mailContentStorageFactory;

        // MailLoop holds handle to the main mail checking loop.
        private Task MainLoop { get; set; }

        /// <summary>
        /// Observable controller state stream.
        /// </summary>
        private readonly Subject<ControllerState> _controllerStateStream = new Subject<ControllerState>();
        public IObservable<ControllerState> ControllerStateStream
        {
            get { return _controllerStateStream; }
        }

        /// <summary>
        /// Observable MailHeader (both new and existing from database) stream.
        /// </summary>
        private readonly Subject<MailHeader> _mailHeaderStream = new Subject<MailHeader>();
        public IObservable<MailHeader> MailHeaderStream
        {
            get { return _mailHeaderStream; }
        }

        /// <summary>
        /// Observable MailContent (both new and existing from database) stream.
        /// </summary>
        private readonly Subject<MailContent> _mailContentStream = new Subject<MailContent>();
        public IObservable<MailContent> MailContentStream
        {
            get { return _mailContentStream; }
        }

        /// <summary>
        /// Observer stream.
        /// Awaits for any email content requests by uid.
        /// </summary>
        private readonly Subject<string> _emailContentRequestByUidStream = new Subject<string>();
        public IObserver<string> EmailContentRequestByUidStream
        {
            get { return _emailContentRequestByUidStream; }
        }

        /// <summary>
        /// MailController constructor.
        /// </summary>
        /// <param name="mailHeaderStorageFactory"></param>
        /// <param name="mailContentStorageFactory"></param>
        public MailController(Func<IMailStorage<MailHeaderEntity>> mailHeaderStorageFactory,
            Func<IMailStorage<MailContentEntity>> mailContentStorageFactory)
        {
            _mailHeaderStorageFactory = mailHeaderStorageFactory;
            _mailContentStorageFactory = mailContentStorageFactory;

            _cancelSource = new CancellationTokenSource();

            // Publish "Hello message" to stream subscribers.
            _controllerStateStream.OnNext(ControllerState.Disconnected);

            // Subscribe to _emailContentRequestByUidStream steam and react on any given requests for content email content.
            _emailContentRequestByUidStream.Subscribe(async uid => { await PublishRequestedByUidMailContent(_cancelSource.Token, uid); });

            // Log each controller state changed.
            _controllerStateStream.Subscribe(state => Logger.Info(state.ToString()));
        }

        /// <summary>
        /// Starts mail controller.
        /// </summary>
        /// <param name="serverType"></param>
        /// <param name="serverEncryption"></param>
        /// <param name="host"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        public void Start(MailServerType serverType,
            MailServerEncryption serverEncryption,
            string host,
            string user,
            string password)
        {
            // Prevents possible issues when Start was called more then once.
            if (MainLoop != null && !MainLoop.IsCompleted)
            {
                Logger.Error("Unexpected engine start detected.");
                return;
                // At the moment it logs error message instead of throwing it.
                // throw new Exception("Unexpected engine start detected.");
            }

            // Save all settings to local variable.
            _host = !string.IsNullOrWhiteSpace(host) ? host : throw new ArgumentException("Invalid host.");
            _user = !string.IsNullOrWhiteSpace(user) ? user : throw new ArgumentException("Invalid user.");
            _password = !string.IsNullOrWhiteSpace(password) ? password : throw new ArgumentException("Invalid password.");
            _serverType = serverType;
            _serverEncryption = serverEncryption;

            MainLoop = Task.Factory.StartNew(() => StartMailCheckAsyncLoop(_cancelSource.Token), _cancelSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            MainLoop.ContinueWith(e =>
            {
                Logger.Error(e.Exception, "StartNewOnFaulted");
            }, TaskContinuationOptions.OnlyOnFaulted);
            MainLoop.ContinueWith(e =>
            {
                Logger.Trace(e.Exception, "StartNewOnCanceled");
            }, TaskContinuationOptions.OnlyOnCanceled);

            Logger.Trace($"Started");
        }

        /// <summary>
        /// Start mail check async loop.
        /// </summary>
        /// <returns></returns>
        private async Task StartMailCheckAsyncLoop(CancellationToken cancel)
        {
            // Run as long it won't be cancelled.
            // Note: The idea is that controller will never stop checking for new emails and always will try to auto-reconnect in case of any connection problems.
            while (!cancel.IsCancellationRequested)
            {
                try
                {
                    await RunCheckForNewMailAsyncLoop(cancel);
                }
                catch (Exception e)
                {
                    // We should never log and error here but inside Run method.
                    Logger.Error(e, "StartMailCheckAsyncLoop");
                }

                if (!cancel.IsCancellationRequested)
                {
                    // Publish reconnecting state.
                    _controllerStateStream.OnNext(ControllerState.Reconnecting);

                    // Give it 5s before reconnecting.
                    await Observable.Return(0).Delay(TimeSpan.FromSeconds(5), Scheduler.CurrentThread).ToTask(cancel);
                }
            }
        }

        /// <summary>
        /// Every 30s queries mail server for new email.
        /// When there are new emails available it first download all mail headers and publishes them to the stream.
        /// Afterwards start downloading all mail content for just downloaded mail headers.
        /// </summary>
        /// <param name="cancel"></param>
        /// <returns></returns>
        private async Task RunCheckForNewMailAsyncLoop(CancellationToken cancel)
        {
            // Create mail client.
            IMailClient client = (new TrivialMailDllFactory()).Build(_serverType);

            try
            {
                // Publish Connecting state.
                _controllerStateStream.OnNext(ControllerState.Connecting);
                client.Connect(_serverEncryption, _host);

                // Publish LoggingIn state.
                _controllerStateStream.OnNext(ControllerState.LoggingIn);
                client.Login(_user, _password);

                // Publish Connected state.
                _controllerStateStream.OnNext(ControllerState.Connected);

                // Main loop
                while (!cancel.IsCancellationRequested)
                {
                    // If disconnect or not encrypted (when should be) then reconnect.
                    if (client.IsConnected && (_serverEncryption == MailServerEncryption.Unencrypted || client.IsEncrypted))
                    {
                        // MailHeaderList contains new headers which will be published to subscribers.
                        List<MailHeaderEntity> mailHeaderEntities = new List<MailHeaderEntity>();

                        using (IMailStorage<MailHeaderEntity> storage = _mailHeaderStorageFactory())
                        {
                            // 1. Get from mail server all uids (emails).
                            // ToDo: for Imap this could be improved.
                            List<string> newUids = client.GetAllUids().ToList();

                            // 2. Distinct list of uids which are not yet stored in the database.
                            // Let's reverse and start checking with the most recent email (the latest uid).
                            newUids.Reverse();
                            List<string> uidList = new List<string>();
                            foreach (var uid in newUids)
                            {
                                if (!storage.Exists(x => x.Uid == uid))
                                    uidList.Add(uid);
                                else
                                    break; 
                                // Note: if any first exists, break the loop other emails are probably downloaded. 
                            }

                            if (uidList.Count > 0)
                            {
                                // 3. Download mail headers.
                                foreach (var uid in uidList)
                                {
                                    // Download message header.
                                    var header = client.GetHeadersByUid(uid);

                                    // Note: MailDll documentation states that header can be null.
                                    if (header == null)
                                        throw new ArgumentNullException(nameof(header), $"Downloaded an empty email header ({uid}).");

                                    var email = new MailBuilder().CreateFromEml(header);
                                    var emailFrom = email?.From.FirstOrDefault();
                                    var mailHeaderEntity = new MailHeaderEntity()
                                    {
                                        Uid = uid,
                                        Date = email?.Date ?? DateTime.MinValue,
                                        Subject = email?.Subject,
                                        MailFromEntity = new MailFromEntity()
                                        {
                                            Address = emailFrom?.Address,
                                            Name = emailFrom?.Name,
                                            LocalPart = emailFrom?.LocalPart,
                                            DomainPart = emailFrom?.DomainPart
                                        }
                                    };

                                    mailHeaderEntities.Add(mailHeaderEntity);
                                }

                                // 4. Insert all new mail headers into the storage.
                                storage.Insert(mailHeaderEntities);
                            }
                        }

                        // For all new email headers publish them to the subscribers and download the content.
                        // Note: This whole block is taken out from above using() to release storage handle asap.
                        if (mailHeaderEntities.Count > 0)
                        {
                            // 5. Publish all new mail headers to the stream.
                            mailHeaderEntities.ForEach(mailHeaderEntity => { _mailHeaderStream.OnNext(new MailHeader(mailHeaderEntity)); });

                            // 6. Start downloading content loop
                            // It's not done in above foreach loop to not to keep storage open for too long
                            // when running over slow internet connection.
                            RunDownloadContentAsyncLoop(cancel, mailHeaderEntities.Select(x => x.Uid).ToList());
                        }
                    }
                    else
                    {
                        break;
                    }

                    // Check for new email again in 30s
                    await Observable.Return(0).Delay(TimeSpan.FromSeconds(30), Scheduler.CurrentThread).ToTask(cancel);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"RunCheckForNewMailAsyncLoop");
            }
            finally
            {
                client?.Close();

                if (!cancel.IsCancellationRequested)
                {
                    // Publish Disconnected state.
                    _controllerStateStream.OnNext(ControllerState.Disconnected);
                }
            }
        }

        /// <summary>
        /// Loads all existing mail headers in local database and publish them to the stream.
        /// </summary>
        public async void LoadAllExistingMailHeaders()
        {
            try
            {
                await Task.Run(() =>
                {
                    // Loads all existing mail headers in local database and publish them to subscribers.
                    using (IMailStorage<MailHeaderEntity> storage = _mailHeaderStorageFactory())
                    {
                        var mailHeaderEntities = storage.FindAll().ToList();
                        if (mailHeaderEntities.Count > 0)
                        {
                            // Publish mail headers.
                            mailHeaderEntities.ForEach(mailHeaderEntity => { _mailHeaderStream.OnNext(new MailHeader(mailHeaderEntity)); });
                        }
                    }
                });
            }
            catch(Exception e)
            {
                Logger.Error(e, "LoadAllExistingMailHeaders");
            }
        }

        /// <summary>
        /// Publish requested mail content to the stream.
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private async Task PublishRequestedByUidMailContent(CancellationToken cancel, string uid)
        {
            try
            {
                using (IMailStorage<MailContentEntity> storage = _mailContentStorageFactory())
                {
                    // 1. Check if content already exists in the database.
                    if (storage.Exists(x => x.Uid == uid))
                    {
                        var mailContentEntity = storage.FindOne(x => x.Uid == uid);

                        if (mailContentEntity == null || !mailContentEntity.IsComplete)
                        {
                            // 2. If content is not yet downloaded of any reason, download it and publish afterwards.
                            await RunDownloadContentAsyncLoop(cancel, new List<string>() { uid });
                        }
                        else
                        {
                            _mailContentStream.OnNext(new MailContent(mailContentEntity));
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Logger.Error(e, "PublishRequestedByUidMailContent");
            }
        }
        
        /// <summary>
        /// Download MailContent for given uids.
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="uids"></param>
        /// <returns></returns>
        private async Task RunDownloadContentAsyncLoop(CancellationToken cancel, List<string> uids)
        {
            if (uids == null || uids.Count == 0)
                return;

            // Create mail client.
            IMailClient client = (new TrivialMailDllFactory()).Build(_serverType);

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    client.Connect(_serverEncryption, _host);
                    client.Login(_user, _password);

                    // If is not connected or encrypted (if req) then will break and try to reconnect.
                    if (client.IsConnected && (_serverEncryption == MailServerEncryption.Unencrypted || client.IsEncrypted))
                    {
                        using (IMailStorage<MailContentEntity> storage = _mailContentStorageFactory())
                        {
                            foreach (var uid in uids)
                            {
                                if (string.IsNullOrWhiteSpace(uid))
                                    continue;

                                if (cancel.IsCancellationRequested)
                                    break;

                                var downloadRequired = true;
                                MailContentEntity mailContentEntity = null;
                                if (storage.Exists(x => x.Uid == uid))
                                {
                                    mailContentEntity = storage.FindOne(x => x.Uid == uid);

                                    if (mailContentEntity != null && mailContentEntity.IsComplete)
                                    {
                                        downloadRequired = false;
                                    }
                                }

                                if (downloadRequired)
                                {
                                    // 1. Insert empty MailContent with only uid set to prevent other concurrent method to download same content.
                                    mailContentEntity = new MailContentEntity()
                                    {
                                        Uid = uid,
                                        IsComplete = false
                                    };
                                    storage.Insert(new List<MailContentEntity>() { mailContentEntity });

                                    // 2. Download complete email.
                                    var message = client.GetMessageByUid(uid);

                                    // Note: MailDll documentation states that message can be null.
                                    if (message != null)
                                    {
                                        IMail email = new MailBuilder().CreateFromEml(message);
                                        if (email != null)
                                        {
                                            // 3. Update database with downloaded email content.
                                            mailContentEntity = new MailContentEntity()
                                            {
                                                Uid = uid,
                                                Date = email?.Date == null ? DateTime.MinValue : email.Date.Value,
                                                Html = email.Html,
                                                MessageId = email.MessageID,
                                                Text = email.Text,
                                                CustomHeader = email?.Document.Root.Headers["x-spam"],
                                                IsComplete = true
                                            };

                                            storage.Update(new List<MailContentEntity>() { mailContentEntity });

                                            // Publish MailContent.
                                            _mailContentStream.OnNext(new MailContent(mailContentEntity));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }, cancel, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
            }
            catch (Exception e)
            {
                Logger.Error(e, $"RunDownloadContentAsync");
            }
            finally
            {
                client?.Close();
            }
        }

        /// <summary>
        /// Stops mail controller and resets it to its original state.
        /// </summary>
        public void Stop()
        {
            try
            {
                // Publish Stopped state.
                _controllerStateStream.OnNext(ControllerState.Disconnected);

                // Cancel all currently running processes / threads / tasks.
                _cancelSource?.Cancel();
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Stop");
            }
            finally
            {
                // Reset MailController to it's default state.
                _host = string.Empty;
                _user = string.Empty;
                _password = string.Empty;
                MainLoop = null;
                _cancelSource = new CancellationTokenSource();
            }
        }

        /// <summary>
        /// Stops (disposes) mail controller and resets it to its original state.
        /// </summary>
        public void Dispose()
        {
            this.Stop();
        }
    }
}
