using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Config;
using NUnit.Framework;
using MailModule;
using MailStorage;
using Models;

namespace MailModuleTests
{
    /// <summary>
    /// MailController general testing class.
    /// Note: worth to mention that mail controller is a thing which is really hard to test with unit tests.
    /// Note: mainly due to timings e.g. how long test should wait for any mail headers to assume that test failed or succeeded?
    /// Note: mail providers that mail controller is using are from 3rd parties therefore they must be tested.
    /// Note: testing 3rd party libraries is even harder. But can be achieved on some level.
    /// </summary>
    public class GeneralTests
    {
        // MailController settings (this could be loaded from xml file, separate for each developer/machine).
        private readonly string _host = "imap.mail.ch";
        private readonly string _user = "onecodiedog";
        private readonly string _password = "Haslo.123";
        private readonly Random _randomizer = new Random();
        private readonly string _dbFilepath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HappyMail"),"mailDbTest.db");

        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// Test creation methods. Simple tests basically awaiting for any exception.
        /// </summary>
        [Test]
        public void TestCreateMailController()
        {
            try
            {
                CreateMailController();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Assert.Fail();
            }
        }
        [Test]
        public void TestCreateMailHeaderMailContentAndDbInsertion()
        {
            try
            {
                var generatedUid = CreateMailHeaderAndInsertIntoDb();
                CreateMailContentAndInsertIntoDb(generatedUid);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Assert.Fail();
            }
        }
     
        /// <summary>
        /// Initial tests to test helpers methods.
        /// Check if empty params are passed Start method will throw an exception.
        /// </summary>
        [Test]
        public void EmptyParametersTest()
        {
            MailController mailController = null;
            try
            {
                mailController = CreateMailController();
                mailController.Start(MailServerType.Imap, MailServerEncryption.SslTls, "", "", "");
            }
            catch (Exception e)
            {
                mailController?.Dispose();
                return;
            }

            Assert.Fail(); 
        }


        /// <summary>
        /// Test mail controller connection (if connects).
        /// If yes, it will also try to download any first message header.
        /// Note: this is set of tests to test different mail server types and mail server encryption.
        /// Note: this test at the same time tests main RunCheckForNewMailAsyncLoop method which does all the magic to download an email.
        /// </summary>
        [Test]
        public void ImapSslTls_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap,MailServerEncryption.SslTls);
            if (mailHeader == null) Assert.Fail();
        }
        [Test]
        public void ImapStartTlss_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap, MailServerEncryption.StartTls);
            if (mailHeader == null) Assert.Fail();
        }
        [Test]
        public void ImaUnencrypted_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap, MailServerEncryption.Unencrypted);
            if (mailHeader == null) Assert.Fail();
        }
        [Test]
        public void Pop3SslTls_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap,MailServerEncryption.SslTls);
            if (mailHeader == null) Assert.Fail();
        }
        [Test]
        public void Pop3StartTls_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap, MailServerEncryption.StartTls);
            if (mailHeader == null) Assert.Fail();
        }
        [Test]
        public void Pop3Unencrypted_TestConnectionAndAwaitForAnyFirstMessage()
        {
            MailHeader mailHeader = TestConnectionAndAwaitAnyFirstMailHeader(MailServerType.Imap, MailServerEncryption.Unencrypted);
            if (mailHeader == null) Assert.Fail();
        }

        /// <summary>
        /// Test of RunDownloadContentAsyncLoop. This method is not called explicitly.
        /// When MailController starts and downloads all available mail headers afterwards it will start downloading and publishing mail contents.
        /// </summary>
        [Test]
        public void ImapSslTls_TestRunDownloadContentAsyncLoop()
        {
            MailContent mailContent = null;
            var mailController = CreateMailController();

            try
            {
                mailController.Start(MailServerType.Imap, MailServerEncryption.SslTls, _host, _user, _password);
                mailContent = mailController.MailContentStream.Take(1).Timeout(DateTime.Now.AddSeconds(60)).Wait();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                mailContent = null;
            }
            finally
            {
                mailController.Dispose();
            }

            Debug.WriteLine($"[{mailContent.Date}] {mailContent.Uid} | Subject: {mailContent.Text}");

            if (mailContent == null)
                Assert.Fail();
        }

        /// <summary>
        /// Test if LoadAllExistingMailHeaders method loads mail headers.
        /// </summary>
        [Test]
        public async Task TestLoadAllExistingMailHeaders()
        {
            try
            {
                MailHeader mailHeader = null;
                var generatedUid = CreateMailHeaderAndInsertIntoDb();
                var mailController = CreateMailController();
                // Run is not awaiting on purpose. This is only away to add the same time wait for mail header and lead mail header from database.
                Task.Run(() => { mailController.LoadAllExistingMailHeaders(); });
                mailHeader = mailController.MailHeaderStream.Take(1).Timeout(DateTime.Now.AddSeconds(15)).Wait();

                if (mailHeader.Uid != generatedUid)
                    Assert.Fail();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Assert.Fail();
            }
        }

        /// <summary>
        /// Test if PublishRequestedByUidMailContent method loads mail contents.
        /// </summary>
        [Test]
        public async Task TestPublishRequestedByUidMailContent()
        {
            try
            {
                MailContent mailContent = null;
                var generatedUid = CreateMailContentAndInsertIntoDb();
                var mailController = CreateMailController();
                // Run is not awaiting on purpose. This is only away to add the same time wait for mail content and lead mail content from database.
                Task.Run(() => { mailController.EmailContentRequestByUidStream.OnNext(generatedUid); });
                mailContent = mailController.MailContentStream.Take(1).Timeout(DateTime.Now.AddSeconds(15)).Wait();

                if (mailContent.Uid != generatedUid)
                    Assert.Fail();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                Assert.Fail();
            }
        }


        [TearDown]
        public void Cleanup()
        {
            if(File.Exists(_dbFilepath)) File.Delete(_dbFilepath);
        }

        private string CreateMailHeaderAndInsertIntoDb()
        {
            string uid = _randomizer.Next(1, 100).ToString();

            using (var storage = new LiteDbMailHeaderEntityStorage(_dbFilepath))
            {
                storage.Insert(new List<MailHeaderEntity>() { new MailHeaderEntity()
                {
                    Uid = uid,
                    MailFromEntity = new MailFromEntity() { Name = "TestName", Address = "test@email.name"}
                } });
            }

            return uid;
        }

        private string CreateMailContentAndInsertIntoDb(string uid = null)
        {
            if(uid == null)
                uid = _randomizer.Next(101, 200).ToString();

            using (var storage = new LiteDbMailContentEntityStorage(_dbFilepath))
            {
                storage.Insert(new List<MailContentEntity>() { new MailContentEntity()
                {
                    Uid = uid,
                    Date = DateTime.Now,
                    Text = "Test email text.",
                    Html = "<html><body><b>Test email html text.</b></body></html>",
                    IsComplete = true,
                } });
            }

            return uid;
        }

        // Private functions used through tests.
        private MailController CreateMailController()
        {
            return new MailController(() => new LiteDbMailHeaderEntityStorage(_dbFilepath), () => new LiteDbMailContentEntityStorage(_dbFilepath));
        }

        private MailHeader TestConnectionAndAwaitAnyFirstMailHeader(MailServerType serverType, MailServerEncryption serverEncryption)
        {
            MailHeader mailHeader = null;
            var mailController = CreateMailController();

            try
            {
                mailController.Start(MailServerType.Imap, MailServerEncryption.SslTls, _host, _user, _password);
                mailHeader = mailController.MailHeaderStream.Take(1).Timeout(DateTime.Now.AddSeconds(15)).Wait();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                mailHeader = null;
            }
            finally
            {
                mailController.Dispose();
            }

            Debug.WriteLine($"[{mailHeader.Date}] {mailHeader.MailFrom} | Subject: {mailHeader.Subject}");
            return mailHeader;
        }
    }
}