using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Config
{
    public enum MailServerEncryption
    {
        [Description("SSL/TLS")]
        SslTls,
        [Description("Unencrypted")]
        Unencrypted,
        [Description("STARTTLS")]
        StartTls
    }
}
