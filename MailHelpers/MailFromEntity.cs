using System;
using System.Collections.Generic;
using System.Text;
using Limilabs.Mail.Headers;
using LiteDB;

namespace Models
{
    public class MailFromEntity : IEntity
    {
        /// <summary>
        /// Gets display name of this emmail address e.g. "John Smith". Can by <c>null</c>.
        /// Please note that two classes inherit from this class - you can use <see cref="P:Limilabs.Mail.Headers.MailBox.Address" /> or <see cref="P:Limilabs.Mail.Headers.MailGroup.Addresses" /> properties to get email address(es).
        /// Consider using <see cref="M:Limilabs.Mail.Headers.MailAddress.GetMailboxes" /> method to extract <see cref="T:Limilabs.Mail.Headers.MailBox" />(es) represented by this email address.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets email address e.g. "john.smith@example.com".
        /// </summary>
        [BsonId]
        public string Address { get; set; }

        /// <summary>
        /// Gets the domain part of the <see cref="P:Limilabs.Mail.Headers.MailBox.Address" /> e.g. "example.com".
        /// </summary>
        public string DomainPart { get; set; }

        /// <summary>
        /// Gets the local part of the <see cref="P:Limilabs.Mail.Headers.MailBox.Address" /> e.g. "john.smith".
        /// </summary>
        public string LocalPart { get; set; }

        public MailFromEntity() { }
    }
}
