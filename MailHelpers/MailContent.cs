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
    /// MailContent class.
    /// </summary>
    public class MailContent
    {
        [BsonId]
        public string Uid { get; }
        public DateTime Date { get; }
        public string MessageId { get; }
        //public long UIDValidity { get; } // ToDo: use it?
        public string Text { get; }
        public string Html { get; }
        public string CustomHeader { get; }

        public MailContent() { }

        public MailContent(string uid, IMail email, string folderName = null)
        {
            this.Uid = uid;
            this.Date = email?.Date ?? DateTime.MinValue;
            this.MessageId = email?.MessageID;
            this.Text = email?.Text;
            this.Html = email?.Html;
            this.CustomHeader = email?.Document.Root.Headers["x-spam"];
        }

        public MailContent(MailContentEntity mailContentEntity)
        {
            this.Uid = mailContentEntity.Uid;
            this.Date = mailContentEntity.Date;
            this.MessageId = mailContentEntity.MessageId;
            this.Text = mailContentEntity.Text;
            this.Html = mailContentEntity.Html;
            this.CustomHeader = mailContentEntity.CustomHeader;
        }
    }
}
