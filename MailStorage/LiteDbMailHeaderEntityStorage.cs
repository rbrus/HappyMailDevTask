using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LiteDB;
using Models;
using NLog.LayoutRenderers.Wrappers;

namespace MailStorage
{
    public class LiteDbMailHeaderEntityStorage : IMailStorage<MailHeaderEntity>
    {
        private readonly BsonMapper _mapper = BsonMapper.Global;
        private readonly LiteCollection<MailHeaderEntity> _mailHeaderEntity;
        private readonly LiteCollection<MailFromEntity> _mailFromEntity;
        private readonly LiteDatabase _liteDatabase;

        public LiteDbMailHeaderEntityStorage(string dbFilepath)
        {
            // If path doesn't exist, create it.
            if (!Directory.Exists(dbFilepath))
            {
                var dir = Path.GetDirectoryName(dbFilepath);
                if (dir != null) Directory.CreateDirectory(dir);
            }

            _mapper.Entity<MailHeaderEntity>().DbRef(x => x.MailFromEntity, "MailFromEntity");
            _liteDatabase = new LiteDatabase(dbFilepath);
            _mailFromEntity = _liteDatabase.GetCollection<MailFromEntity>("MailFromEntity");
            _mailHeaderEntity = _liteDatabase.GetCollection<MailHeaderEntity>("MailHeaderEntity");
        }

        public int Count()
        {
            return _mailHeaderEntity.Count();
        }

        public void Insert(List<MailHeaderEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (!_mailFromEntity.Exists(x => x.Address == mail.MailFromEntity.Address))
                {
                    _mailFromEntity.Insert(mail.MailFromEntity);
                }

                if (!_mailHeaderEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailHeaderEntity.Insert(mail);
                    // create index
                    _mailHeaderEntity.EnsureIndex(x => x.Uid); ;
                }
            }
        }

        public void Update(List<MailHeaderEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (_mailFromEntity.Exists(x => x.Address == mail.MailFromEntity.Address))
                {
                    _mailFromEntity.Update(mail.MailFromEntity);
                }

                if (_mailHeaderEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailHeaderEntity.Update(mail);
                }
            }
        }

        public void Upsert(List<MailHeaderEntity> entities)
        {
            foreach (var mail in entities)
            {
                if (!_mailFromEntity.Exists(x => x.Address == mail.MailFromEntity.Address))
                {
                    _mailFromEntity.Upsert(mail.MailFromEntity);
                }

                if (!_mailHeaderEntity.Exists(x => x.Uid == mail.Uid))
                {
                    _mailHeaderEntity.Upsert(mail);
                    // create index
                    _mailHeaderEntity.EnsureIndex(x => x.Uid); ;
                }
            }
        }

        public bool Exists(Expression<Func<MailHeaderEntity, bool>> predicate)
        {
            return _mailHeaderEntity.Exists(predicate);
        }

        public void Delete(Expression<Func<MailHeaderEntity, bool>> predicate)
        {
            _mailHeaderEntity.Delete(predicate);
        }

        public MailHeaderEntity FindOne(Expression<Func<MailHeaderEntity, bool>> predicate)
        {
            return _mailHeaderEntity.Include(x => x.MailFromEntity).FindOne(predicate);
        }

        public IEnumerable<MailHeaderEntity> FindMany(Expression<Func<MailHeaderEntity, bool>> predicate)
        {
            return _mailHeaderEntity.Include(x => x.MailFromEntity).Find(predicate);
        }

        public IEnumerable<MailHeaderEntity> FindAll()
        {
            return _mailHeaderEntity.Include(x => x.MailFromEntity).FindAll();
        }

        public void Dispose()
        {
            _liteDatabase.Dispose();
        }
    }
}
