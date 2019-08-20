using System;
using System.Collections.Generic;
using Config;
using Limilabs.Client.POP3;
using Limilabs.Mail;

namespace MailDll
{
    public class Pop3Client : IMailClient
    {
        private readonly Pop3 _pop3 = new Pop3();
        public bool IsConnected => _pop3.Connected;
        public bool IsEncrypted => _pop3.IsEncrypted;

        /// <summary>
        /// Gets UIDS of all messages.
        /// </summary>
        /// <seealso cref="M:Limilabs.Client.POP3.Pop3.GetMessageByUID(System.String)" />
        /// <seealso cref="M:Limilabs.Client.POP3.Pop3.GetHeadersByUID(System.String)" />
        /// <exception cref="T:Limilabs.Client.POP3.Pop3ResponseException">Throws <see cref="T:Limilabs.Client.POP3.Pop3ResponseException" /> on error response.</exception>
        /// <returns>UID list.</returns>
        public List<string> GetAllUids()
        {
            return _pop3.GetAll();
        }

        /// <summary>
        /// Gets headers of the specified email message from server. Use <see cref="T:Limilabs.Mail.MailBuilder" /> class to create <see cref="T:Limilabs.Mail.IMail" /> object.
        /// </summary>
        /// <exception cref="T:Limilabs.Client.POP3.Pop3ResponseException">Throws <see cref="T:Limilabs.Client.POP3.Pop3ResponseException" /> on error response.</exception>
        /// <exception cref="T:System.Exception">Invalid uid provided.</exception>
        /// <param name="uid">Unique-id of the message to get.</param>
        /// <returns>Email message headers.</returns>
        public byte[] GetHeadersByUid(string uid)
        {
            return _pop3.GetHeadersByUID(uid);
        }

        /// <summary>
        /// Gets specified email message from server. Use <see cref="T:Limilabs.Mail.MailBuilder" /> class to create <see cref="T:Limilabs.Mail.IMail" /> object.
        /// </summary>
        /// <exception cref="T:Limilabs.Client.POP3.Pop3ResponseException">Throws <see cref="T:Limilabs.Client.POP3.Pop3ResponseException" /> on error response.</exception>
        /// <exception cref="T:System.Exception">Invalid uid provided.</exception>
        /// <param name="uid">Unique-id of the message to get.</param>
        /// <returns>Email message.</returns>
        public byte[] GetMessageByUid(string uid)
        {
            return _pop3.GetMessageByUID(uid);
        }

        /// <summary>
        /// Ends a pending asynchronous connection request.
        /// </summary>
        /// <param name="result">Object that stores state information and any user defined data for this asynchronous operation.</param>
        public void EndConnect(IAsyncResult result)
        {
            _pop3.EndConnect(result);
        }

        /// <summary>
        /// Connects to POP3 server.
        /// </summary>
        /// <param name="host">Host name or IP address.</param>
        /// <param name="serverEncryption">Server encryption (SslTls, Unencrypted, StartTls)</param>
        /// <seealso cref="M:Limilabs.Client.ClientBase.Connect(System.String,System.Int32,System.Boolean)" />
        /// <exception cref="T:Limilabs.Client.ServerException">DNS resolving error, connection error.</exception>
        /// <exception cref="T:Limilabs.Client.POP3.Pop3ResponseException">Initial error response.</exception>
        public void Connect(MailServerEncryption serverEncryption, string host)
        {
            switch (serverEncryption)
            {
                case MailServerEncryption.SslTls:
                {
                    _pop3.ConnectSSL(host);
                    break;
                }

                case MailServerEncryption.Unencrypted:
                {
                    _pop3.Connect(host);
                    break;
                }

                case MailServerEncryption.StartTls:
                {
                    _pop3.Connect(host);
                    _pop3.StartTLS();
                    break;
                }

                default:
                {
                    _pop3.Connect(host);
                    break;
                }
            }
        }

        /// <summary>
        /// Begins an asynchronous request for a remote server secure connection.
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
                    return _pop3.BeginConnectSSL(host);
                }

                case MailServerEncryption.Unencrypted:
                {
                    return _pop3.BeginConnect(host);
                }

                case MailServerEncryption.StartTls:
                {
                    var asyncResult = _pop3.BeginConnect(host);
                    _pop3.StartTLS();
                    return asyncResult;
                }

                default:
                {
                    return _pop3.BeginConnect(host);
                }
            }
        }

        /// <summary>
        /// Logs user in using USER and PASS commands. This method sends the password in clear text, unless SSL connection is used.
        /// </summary>
        /// <param name="user">User's login.</param>
        /// <param name="password">User's password.</param>
        /// <seealso cref="M:Limilabs.Client.POP3.Pop3.LoginAPOP(System.String,System.String)" />
        /// <exception cref="T:Limilabs.Client.POP3.Pop3ResponseException">Throws <see cref="T:Limilabs.Client.POP3.Pop3ResponseException" /> on error response.</exception>
        public void Login(string user, string password)
        {
            _pop3.Login(user, password);
        }

        /// <summary>
        /// Sends QUIT command. Disconnects and releases all resources acquired by this object.
        /// </summary>
        public void Close()
        {
            _pop3.Close();
        }

        /// <summary>
        /// Releases all resources acquired by this object. Closes connection, without issuing any quit commands.
        /// </summary>
        public void Dispose()
        {
            _pop3.Dispose();
        }
    }
}
