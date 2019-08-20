using System;
using System.Collections.Generic;

namespace Config
{
    /// <summary>
    /// IMailClient is an interface which should be implemented
    /// by any mail clients used by the application.
    /// </summary>
    public interface IMailClient : IDisposable
    {
        // Client connection info
        bool IsConnected { get; }
        bool IsEncrypted { get; }

        // Mail management
        List<string> GetAllUids();
        byte[] GetHeadersByUid(string uid);
        byte[] GetMessageByUid(string uid);
        void EndConnect(IAsyncResult result);
        void Connect(MailServerEncryption serverEncryption, string host);
        IAsyncResult BeginConnect(MailServerEncryption serverEncryption, string host);
        void Login(string user, string password);
        void Close();
    }
}
