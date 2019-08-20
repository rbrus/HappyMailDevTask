using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MailStorage;
using Models;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace MailStorageTests
{
    public class Tests
    {
        private readonly Random _randomizer = new Random();
        private readonly string _dbFilepath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HappyMail"), "storageDbTest.db");

        [SetUp]
        public void Setup()
        {
        }

        /// <summary>
        /// As all storage methods are somehow linked through their usage.
        /// E.g. First inserting, then select (find) to check and then delete to clear.
        /// Basically to test one all are tested. So lets make it as a one flow storage functionality test.
        /// </summary>
        [Test]
        public void TestLiteDbMailHeaderStorageMethods()
        {
            // Create first mailHeaderEntity.
            string uid1 = _randomizer.Next(1, 100).ToString();
            var mailHeaderEntity1 = CreateMailHeader(uid1);

            // Create second mailHeaderEntity.
            string uid2 = _randomizer.Next(1, 100).ToString();
            var mailHeaderEntity2 = CreateMailHeader(uid2);

            using (var storage = new LiteDbMailHeaderEntityStorage(_dbFilepath))
            {
                // Insert.
                storage.Insert(mailHeaderEntity1);

                // Upsert.
                storage.Upsert(mailHeaderEntity2);

                // Check count == 2.
                if (storage.Count() != 2) Assert.Fail();

                // Find.
                var mhe = storage.FindOne(x => x.Uid == uid1);

                // Check if found and with correct uid.
                if(mhe == null || mhe.Uid != uid1)
                    Assert.Fail();

                // Test find many should be two.
                var mhemany = storage.FindMany(x => x.Uid == uid1 || x.Uid == uid2).ToList();
                if(mhemany.Count != 2)
                    Assert.Fail();

                // Try to delete.
                storage.Delete(x => x.Uid == uid1);

                // Check if delete and not found.
                var mheone = storage.FindOne(x => x.Uid == uid1);
                if (mheone != null)
                    Assert.Fail();

                // Check count == 1.
                if (storage.Count() != 1) Assert.Fail();

                var mheall = storage.FindAll().ToList();
                if (mheall.Count != 1)
                    Assert.Fail();

                if(!storage.Exists(x => x.Uid == uid2))
                    Assert.Fail();

                // Change subject to a new one.
                string newsubj = "NewSubject";
                mailHeaderEntity2[0].Subject = newsubj;

                // Update.
                storage.Update(mailHeaderEntity2);

                var mheone2 = storage.FindOne(x => x.Uid == uid2);
                if (mheone2 == null)
                    Assert.Fail();

                if (mheone2.Subject != newsubj)
                    Assert.Fail();
            }
        }

        [Test]
        public void TestLiteDbMailContentStorageMethods()
        {
            // Create first mailHeaderEntity.
            string uid1 = _randomizer.Next(1, 100).ToString();
            var mailContentEntity1 = CreateMailContent(uid1);

            // Create second mailHeaderEntity.
            string uid2 = _randomizer.Next(1, 100).ToString();
            var mailContentEntity2 = CreateMailContent(uid2);

            using (var storage = new LiteDbMailContentEntityStorage(_dbFilepath))
            {
                // Insert.
                storage.Insert(mailContentEntity1);

                // Upsert.
                storage.Upsert(mailContentEntity2);

                // Check count == 2.
                if (storage.Count() != 2) Assert.Fail();

                // Find.
                var mhe = storage.FindOne(x => x.Uid == uid1);

                // Check if found and with correct uid.
                if (mhe == null || mhe.Uid != uid1)
                    Assert.Fail();

                // Test find many should be two.
                var mhemany = storage.FindMany(x => x.Uid == uid1 || x.Uid == uid2).ToList();
                if (mhemany.Count != 2)
                    Assert.Fail();

                // Try to delete.
                storage.Delete(x => x.Uid == uid1);

                // Check if delete and not found.
                var mheone = storage.FindOne(x => x.Uid == uid1);
                if (mheone != null)
                    Assert.Fail();

                // Check count == 1.
                if (storage.Count() != 1) Assert.Fail();

                var mheall = storage.FindAll().ToList();
                if (mheall.Count != 1)
                    Assert.Fail();

                if (!storage.Exists(x => x.Uid == uid2))
                    Assert.Fail();

                // Change subject to a new one.
                string newtxt = "NewText";
                mailContentEntity2[0].Text = newtxt;

                // Update.
                storage.Update(mailContentEntity2);

                var mheone2 = storage.FindOne(x => x.Uid == uid2);
                if (mheone2 == null)
                    Assert.Fail();

                if (mheone2.Text != newtxt)
                    Assert.Fail();
            }
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(_dbFilepath)) File.Delete(_dbFilepath);
        }

        private List<MailHeaderEntity> CreateMailHeader(string uid)
        {
            return new List<MailHeaderEntity>()
            {
                new MailHeaderEntity()
                {
                    Uid = uid,
                    MailFromEntity = new MailFromEntity() {Name = "TestName", Address = "test@email.name"}
                }
            };
        }

        private List<MailContentEntity> CreateMailContent(string uid)
        {
            return new List<MailContentEntity>()
            {
                new MailContentEntity()
                {
                    Uid = uid,
                    Date = DateTime.Now,
                    Text = "Test email text.",
                    Html = "<html><body><b>Test email html text.</b></body></html>",
                    IsComplete = true,
                }
            };
        }
    }
}