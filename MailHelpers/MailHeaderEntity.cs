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
    public class MailHeaderEntity : IEntity
    {
        [BsonId]
        public string Uid { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }
        public string FolderName { get; set; }
        public MailFromEntity MailFromEntity { get; set; }

        public MailHeaderEntity() { }
    }
}
