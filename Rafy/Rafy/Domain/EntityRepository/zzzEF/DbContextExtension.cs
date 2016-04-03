//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110320
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100320
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//////////using System.Data.SqlClient;
//using System.Data;
//using Rafy.Utils;
//using System.Collections;
//
//namespace Rafy.Library
//{
//    /// <summary>
//    /// DbContext 类的扩展方法
//    /// </summary>
//    public static class DbContextExtension
//    {
//        public static DataTable QueryDataTable(this DbContext db, string sql)
//        {
//            using (var adapter = new SqlDataAdapter(sql, db.Database.Connection as SqlConnection))
//            {
//                var table = new DataTable();
//                adapter.Fill(table);

//                SQLTrace.Trace(sql);

//                return table;
//            }
//        }

//        public static IDbTable QueryTable(this DbContext db, string sql)
//        {
//            return new DbTable(QueryDataTable(db, sql));
//        }

//        public static IDbTableReader ExecSql(this DbContext db, string sql)
//        {
//            var t = QueryTable(db, sql);
//            return new DbReader(t);
//        }

//        public static IEnumerable ExecSql(this DbContext db, Type type, string sql, params string[] parameters)
//        {
//            return db.Database.SqlQuery(type, sql, parameters);
//        }

//        public static IEnumerable<TEntity> ExecSql<TEntity>(this DbContext db, string sql, params string[] parameters)
//        {
//            return db.Database.SqlQuery<TEntity>(sql, parameters);
//        }

//        #region 元数据

//        public static ObjectContext GetObjectContext(this DbContext context)
//        {
//            return (context as IObjectContextAdapter).ObjectContext;
//        }

//        public static MetadataWorkspace GetMetadata(this DbContext context)
//        {
//            return GetObjectContext(context).MetadataWorkspace;
//        }

//        #endregion

//        #region private class DbReader : IDbTableReader

//        private class DbReader : IDbTableReader
//        {
//            private IDbTable _table;

//            private int _curIndex;

//            private DataRow _curRow;

//            internal DbReader(IDbTable table)
//            {
//                this._table = table;
//            }

//            public bool Next()
//            {
//                if (this._curIndex < this._table.Count)
//                {
//                    this._curRow = this._table[this._curIndex];
//                    this._curIndex++;
//                    return true;
//                }
//                else
//                {
//                    this._curRow = null;
//                    return false;
//                }
//            }

//            public object Get(string columnName)
//            {
//                return this._curRow[columnName];
//            }
//        }

//        #endregion
//    }

//    public interface IDbTableReader
//    {
//        bool Next();
//        object Get(string columnName);
//    }

//    public static class EntityTypeConfigurationExtension
//    {
//        public static void MapToTable<TEntityType>(this EntityTypeConfiguration<TEntityType> config, string tableName)
//            where TEntityType : class
//        {
//            config.ToTable(tableName);

//            ORMContext.DbContextFactoryStore.MapClassToTable(typeof(TEntityType), tableName);
//        }
//    }
//}
