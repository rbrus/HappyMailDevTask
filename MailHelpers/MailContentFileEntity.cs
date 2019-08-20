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
    public class MailContentFileEntity : IEntity
    {
        [BsonId]
        public string Uid { get; set; }
        public DateTime Date { get; set; }

        public MailContentFileEntity() { }
    }
}
