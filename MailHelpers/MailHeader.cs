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
    public class MailHeader
    {
        [BsonId]
        public string Uid { get; }
        public DateTime Date { get; }
        public string Subject { get; }
        public MailFrom MailFrom { get; }
        public string FolderName { get; }

        public MailHeader(string uid, IMail email, string folderName = null)
        {
            this.Uid = uid;
            this.Date = email?.Date ?? DateTime.MinValue;
            this.Subject = email?.Subject;
            this.MailFrom = new MailFrom(email?.From.FirstOrDefault());
            this.FolderName = folderName;
        }

        public MailHeader(MailHeaderEntity mailHeaderEntity)
        {
            this.Uid = mailHeaderEntity.Uid;
            this.Date = mailHeaderEntity.Date;
            this.Subject = mailHeaderEntity.Subject;
            this.MailFrom = new MailFrom(mailHeaderEntity.MailFromEntity);
            this.FolderName = mailHeaderEntity.FolderName;
        }

        public override string ToString()
        {
            return $"[{MailFrom} | {Date.ToString("f")}] {Subject}";
        }
    }
}
