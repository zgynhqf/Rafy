using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using hxy.Common.Data;
using OEA.Library;

namespace OEA.ORM
{
    /// <summary>
    /// ORM 中实体的数据库操作接口
    /// </summary>
    public interface IDb : IDisposable
    {
        /// <summary>
        /// 使用的数据库管理器
        /// </summary>
        IDBAccesser DBA { get; }

        int Insert(IEntity item);

        //int Update(Type type, ICollection items, IList<string> updateColumns);
        //int Update<T>(ICollection<T> items, IList<string> updateColumns);
        //int Update(object item, IList<string> updateColumns);
        //int Update(Type type, ICollection items);
        //int Update<T>(ICollection<T> items);
        int Update(IEntity item);

        //int Delete(Type type, ICollection items);
        //int Delete<T>(ICollection<T> items);
        int Delete(IEntity item);
        int Delete(Type type, IQuery query);
        //int Delete<T>(IQuery query);

        IList<Entity> Select(IQuery typedQuery);
        void Select(IQuery typedQuery, ICollection<Entity> list);
        //IList Select(Type type, IQuery query);
        //IList<T> Select<T>(IQuery query);

        //object Find(Type type, object key);
        //T Find<T>(object key);

        //object Call(string funcName, object[] parameters);

        IList<Entity> Select(Type type, string sql, params object[] parameters);
        void Select(Type type, ICollection<Entity> list, string sql, params object[] parameters);
        //IResultSet Exec(string procName, object[] parameters);
        //IResultSet Exec(string procName, object[] parameters, int[] outputs);

        //IQuery Query();

        /// <summary>
        /// 指定查询的强类型，这个接口生成的 IQuery，可以使用 IManagedProperty 作为查询参数。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        IQuery Query(Type entityType);
    }
}
