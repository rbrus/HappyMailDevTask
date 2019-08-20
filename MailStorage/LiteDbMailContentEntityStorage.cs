using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LiteDB;
using Models;

namespace MailStorage
{
    public class LiteDbMailContentEntityStorage : IMailStorage<MailContentEntity>
    {
        private readonly LiteCollection<MailContentEntity> _mailContentEntity;
        private readonly LiteDatabase _liteDatabase;

        public LiteDbMailContentEntityStorage(string dbFilepath)
        {
            // If path doesn't exist, create it.
            if (!Directory.Exists(dbFilepath))
            {
                var dir = Path.GetDirectoryName(dbFilepath);
                if(dir != null) Directory.CreateDirectory(dir);
            }
                
            _liteDatabase = new LiteDatabase(dbFilepath);
            _mailContentEntity = _liteDatabase.GetCollection<MailContentEntity>("MailContentEntity");
        }

        public int Count()
        {
            return _mailContentEntity.Count();
        }

        public void Insert(List<MailContentEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (!_mailContentEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailContentEntity.Insert(mail);
                    // create index
                    _mailContentEntity.EnsureIndex(x => x.Uid); ;
                }
            }
        }

        public void Update(List<MailContentEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (_mailContentEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailContentEntity.Update(mail);
                }
            }
        }

        public void Upsert(List<MailContentEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (!_mailContentEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailContentEntity.Upsert(mail);
                    // create index
                    _mailContentEntity.EnsureIndex(x => x.Uid); ;
                }
            }
        }

        public bool Exists(Expression<Func<MailContentEntity, bool>> predicate)
        {
            return _mailContentEntity.Exists(predicate);
        }

        public void Delete(Expression<Func<MailContentEntity, bool>> predicate)
        {
            _mailContentEntity.Delete(predicate);
        }

        public IEnumerable<MailContentEntity> FindMany(Expression<Func<MailContentEntity, bool>> predicate)
        {
            return _mailContentEntity.Find(predicate);
        }

        public IEnumerable<MailContentEntity> FindAll()
        {
            return _mailContentEntity.FindAll();
        }

        public MailContentEntity FindOne(Expression<Func<MailContentEntity, bool>> predicate)
        {
            return _mailContentEntity.FindOne(predicate);
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}
