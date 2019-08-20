using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config;
using MailModule;
using MailStorage;

namespace HappyMail.Domain
{
    public static class DummyTrivialSingleton
    {
        private static MailController _mailController = null;

        // ToDo: this should not be here.
        private static string _dbFilepath = Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HappyMail"),
            "cache.db");

        public static MailController GetMailControllerService(string dbConnectionString = null)
        {
            if (_mailController == null)
                _mailController = new MailController(() => new LiteDbMailHeaderEntityStorage(dbConnectionString ?? _dbFilepath), 
                    () => new LiteDbMailContentEntityStorage(dbConnectionString ?? _dbFilepath));

            return _mailController;
        }
    }
}
