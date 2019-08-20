using System;
using System.Collections.Generic;

namespace Config
{
    /// <summary>
    /// IMailClient is an interface which should be implemented
    /// by any mail clients used by the application.
    /// </summary>
    public interface IMailClientFactory
    {
        IMailClient Build(MailServerType serverType);
    }
}
