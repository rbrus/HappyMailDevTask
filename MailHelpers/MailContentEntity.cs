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
    /// MailContentEntity class.
    /// </summary>
    public class MailContentEntity : IEntity
    {
        [BsonId]
        public string Uid { get; set; }
        public DateTime Date { get; set; }
        public string MessageId { get; set; }
        public string Text { get; set; }
        public string Html { get; set; }
        public string CustomHeader { get; set; }
        public bool IsComplete { get; set; }
        public MailContentEntity() { }
    }
}
