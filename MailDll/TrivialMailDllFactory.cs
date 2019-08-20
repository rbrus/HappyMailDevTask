using System;
using System.Collections.Generic;
using System.Text;
using Config;
using MailDll;

namespace MailDll
{
    public class TrivialMailDllFactory : IMailClientFactory
    {
        public IMailClient Build(MailServerType serverType)
        {
            switch (serverType)
            {
                case MailServerType.Imap: return new ImapClient();
                case MailServerType.Pop3: return new Pop3Client();
                default: return new ImapClient();
            }
        }
    }
}
