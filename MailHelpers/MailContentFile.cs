using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Limilabs.Mail;
using Limilabs.Mail.Fluent;
using LiteDB;

namespace Models
{
    /// <summary>
    /// MailHeader class.
    /// </summary>
    public class MailContentFile
    {
        [BsonId]
        public string Uid { get; }
        public DateTime Date { get; }

        /// <summary>
        /// Required by mail storage - BsonDocument.
        /// </summary>
        public MailContentFile() { }

        public MailContentFile(string uid, IMail mail, string folderName = null)
        {
            this.Uid = uid;
            this.Date = mail.Date ?? DateTime.MinValue;

            // ToDo: to be done.
            //foreach (var attachement in mail.Attachments)
            //{
            //    attachement.FileName
            //}
        }

        public MailContentFile(MailContentFileEntity mailContentFileEntity)
        {

        }
    }
}
