using System;
using System.ComponentModel;

namespace Config
{
    public enum MailServerType
    {
        [Description("IMAP")]
        Imap,
        [Description("POP3")]
        Pop3
    }
}
