using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace OEA.ORM
{
    public interface IDb : IDisposable
    {
        bool IsClosed { get; }
        bool IsAutoCommit { get; }
        IDbConnection Connection { get; }
        IDbTransaction Transaction { get; }

        void Begin();
        void Commit();
        void Rollback();

        int Insert(Type type, ICollection items);
        int Insert<T>(ICollection<T> items);
        int Insert(object item);

        int Update(Type type, ICollection items, IList<string> updateColumns);
        int Update<T>(ICollection<T> items, IList<string> updateColumns);
        int Update(object item, IList<string> updateColumns);
        int Update(Type type, ICollection items);
        int Update<T>(ICollection<T> items);
        int Update(object item);

        int Delete(Type type, ICollection items);
        int Delete<T>(ICollection<T> items);
        int Delete(object item);
        int Delete(Type type, IQuery query);
        int Delete<T>(IQuery query);

        IList Select(IQuery typedQuery);
        IList Select(Type type, IQuery query);
        IList<T> Select<T>(IQuery query);

        object Find(Type type, object key);
        T Find<T>(object key);

        object Call(string funcName, object[] parameters);

        IList Exec(Type type, string procName, object[] parameters);
        IResultSet Exec(string procName, object[] parameters);
        IResultSet Exec(string procName, object[] parameters, int[] outputs);

        IQuery Query();

        /// <summary>
        /// 指定查询的强类型，这个接口生成的 IQuery，可以使用 IManagedProperty 作为查询参数。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IQuery Query(Type entityType);

        ITable GetTable(Type type);
    }
}
