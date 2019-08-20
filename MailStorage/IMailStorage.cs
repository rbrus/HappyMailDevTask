using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Models;

namespace MailStorage
{
    /// <summary>
    /// Interface shared by MailStorage implementations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMailStorage<T> : IDisposable where T : IEntity
    {
        int Count();
        void Insert(List<T> entities);
        void Update(List<T> entities);
        void Upsert(List<T> entities);
        bool Exists(Expression<Func<T, bool>> predicate);
        void Delete(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindMany(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindAll();
        T FindOne(Expression<Func<T, bool>> predicate);
    }
}
