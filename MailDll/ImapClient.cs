using System;
using System.Collections.Generic;
using System.Linq;
using Config;
using Limilabs.Client.IMAP;
using Limilabs.Mail;

namespace MailDll
{
    public class ImapClient : IMailClient
    {
        private readonly Imap _imap = new Imap();

        public bool IsConnected => _imap.Connected;
        public bool IsEncrypted => _imap.IsEncrypted;

        /// <summary>
        /// Gets UIDs of all messages in the current folder (mailbox) sorted from oldest to newest. Equivalent to Search(Expression.All()).
        /// </summary>
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.GetMessageByUID(System.Int64)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.GetHeadersByUID(System.Int64)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.PeekMessageByUID(System.Int64)" />
        /// <returns>UID list sorted from oldest to newest.</returns>
        public List<string> GetAllUids()
        {
            _imap.SelectInbox();
            return _imap.GetAll().Select(x => x.ToString()).ToList();
        }

        /// <summary>
        /// Gets UIDS of all messages in the current folder (mailbox) with specified flag sorted from oldest to newest. Equivalent to Search(Expression.HasFlag(flag))
        /// </summary>
        /// <param name="flag">Flag to search for.</param>
        /// <returns>UID list sorted from oldest to newest.</returns>
        public List<long> Search(FolderInfo folder, Flag flag)
        {
            _imap.Select(folder);
            return _imap.Search(flag);
        }

        /// <summary>
        /// Gets UIDS of all messages in the current folder (mailbox) with specified flag sorted from oldest to newest. Equivalent to Search(Expression.HasFlag(flag))
        /// </summary>
        /// <param name="flag">Flag to search for.</param>
        /// <returns>UID list sorted from oldest to newest.</returns>
        public List<long> Search(string folder, Flag flag)
        {
            _imap.Select(folder);
            return _imap.Search(flag);
        }

        /// <summary>
        /// Gets UIDS of all messages in the current folder (mailbox) with specified flag sorted from oldest to newest. Equivalent to Search(Expression.HasFlag(flag))
        /// </summary>
        /// <param name="flag">Flag to search for.</param>
        /// <returns>UID list sorted from oldest to newest.</returns>
        public List<long> SearchInbox(Flag flag)
        {
            _imap.SelectInbox();
            return _imap.Search(flag);
        }

        /// <summary>
        /// Gets all headers of the specified email message from server. Use <see cref="T:Limilabs.Mail.MailBuilder" /> class to create <see cref="T:Limilabs.Mail.IMail" /> object.
        /// </summary>
        /// <param name="uid">Unique-id of the message to get headers for.</param>
        /// <returns>Email message headers or null when <paramref name="uid" /> is incorrect.</returns>
        public byte[] GetHeadersByUid(string uid)
        {
            _imap.SelectInbox();
            return _imap.GetHeadersByUID(long.Parse(uid));
        }

        /// <summary>
        /// Gets specified email message from server. Use <see cref="T:Limilabs.Mail.MailBuilder" /> class to create <see cref="T:Limilabs.Mail.IMail" /> object.
        /// This method sets the <see cref="F:Limilabs.Client.IMAP.Flag.Seen" /> unless folder is selected with <see cref="M:Limilabs.Client.IMAP.Imap.Examine(System.String)" />, <see cref="M:Limilabs.Client.IMAP.Imap.Examine(Limilabs.Client.IMAP.FolderInfo)" />  or <see cref="M:Limilabs.Client.IMAP.Imap.ExamineInbox" />.
        /// </summary>
        /// <param name="uid">Unique-id of the message to get.</param>
        /// <returns>Email message or null when <paramref name="uid" /> is incorrect.</returns>
        public byte[] GetMessageByUid(string uid)
        {
            _imap.SelectInbox();
            return _imap.GetMessageByUID(long.Parse(uid));
        }

        /// <summary>
        /// Ends a pending asynchronous connection request.
        /// </summary>
        /// <param name="result">Object that stores state information and any user defined data for this asynchronous operation.</param>
        public void EndConnect(IAsyncResult result)
        {
            _imap.EndConnect(result);
        }

        /// <summary>
        /// Connects to IMAP server.
        /// </summary>
        /// <param name="serverEncryption">Server encryption (SslTls, Unencrypted, StartTls)</param>
        /// <param name="host">Target host name or IP address.</param>
        /// <exception cref="T:Limilabs.Client.ServerException">DNS resolving error, connection error.</exception>
        /// <exception cref="T:Limilabs.Client.IMAP.ImapResponseException">Initial error response.</exception>
        public void Connect(MailServerEncryption serverEncryption, string host)
        {
            switch (serverEncryption)
            {
                case MailServerEncryption.SslTls:
                {
                    _imap.ConnectSSL(host);
                    break;
                }

                case MailServerEncryption.Unencrypted:
                {
                    _imap.Connect(host);
                    break;
                }

                case MailServerEncryption.StartTls:
                {
                    _imap.Connect(host);
                    _imap.StartTLS();
                    break;
                }

                default:
                {
                    _imap.Connect(host);
                    break;
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous request for a remote server connection using.
        /// </summary>
        /// <param name="serverEncryption">Server encryption (SslTls, Unencrypted, StartTls)</param>
        /// <param name="host">The name or IP address of the remote server.</param>
        /// <returns>An IAsyncResult that references the asynchronous connection.</returns>
        public IAsyncResult BeginConnect(MailServerEncryption serverEncryption, string host)
        {
            switch (serverEncryption)
            {
                case MailServerEncryption.SslTls:
                {
                    return _imap.BeginConnectSSL(host);
                }

                case MailServerEncryption.Unencrypted:
                {
                    return _imap.BeginConnect(host);
                }

                case MailServerEncryption.StartTls:
                {
                    var asyncResult = _imap.BeginConnect(host);
                    _imap.StartTLS();
                    return asyncResult;
                }

                default:
                {
                    return _imap.BeginConnect(host);
                }
            }
        }

        /// <summary>
        /// Logs user in using best method available.
        /// When no AUTH capability is found, this method switches to SSL (<see cref="M:Limilabs.Client.IMAP.Imap.StartTLS" />) and tries again.
        /// </summary>
        /// <param name="user">User's login.</param>
        /// <param name="password">User's password.</param>
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.Login(System.String,System.String)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.LoginPLAIN(System.String,System.String)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.LoginPLAIN(System.String,System.String,System.String)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.LoginCRAM(System.String,System.String)" />
        /// <seealso cref="M:Limilabs.Client.IMAP.Imap.LoginOAUTH(System.String)" />
        /// <exception cref="T:Limilabs.Client.IMAP.ImapResponseException">Throws <see cref="T:Limilabs.Client.IMAP.ImapResponseException" /> on error response.</exception>
        public void Login(string user, string password)
        {
            _imap.UseBestLogin(user, password);
        }

        /// <summary>
        /// Selects 'INBOX' as a current folder (mailbox) so that messages inside can be accessed.
        /// Select command will remove the <see cref="F:Limilabs.Client.IMAP.Flag.Recent" /> flag since the folder has now been viewed
        /// (This is not the same as the <see cref="F:Limilabs.Client.IMAP.Flag.Seen" /> IMAP flag).
        /// </summary>
        /// <returns>Status of the folder (mailbox). It can be updated by subsequent <see cref="M:Limilabs.Client.IMAP.Imap.Noop" /> commands or other commands.</returns>
        public FolderStatus SelectInbox()
        {
            return _imap.SelectInbox();
        }

        /// <summary>
        /// Sends LOGOUT command. Releases all resources acquired by this object.
        /// </summary>
        public void Close()
        {
            _imap.Close();
        }

        /// <summary>
        /// Releases all resources acquired by this object. Closes connection, without issuing any quit commands.
        /// </summary>
        public void Dispose()
        {
            _imap.Dispose();
        }
    }
}
