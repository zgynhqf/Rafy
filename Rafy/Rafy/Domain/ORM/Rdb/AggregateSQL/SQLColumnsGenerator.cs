/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合SQL列名的生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Diagnostics;
using Rafy.Domain.ORM.SqlServer;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 聚合SQL列名的生成器
    /// 
    /// 从原有的EntityRepository中直接抽取出来的类。
    /// </summary>
    internal class SQLColumnsGenerator
    {
        private IRepositoryInternal _repository;

        public SQLColumnsGenerator(IRepositoryInternal repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            this._repository = repository;
        }

        /// <summary>
        /// 数据行中的列名必须由 SQLColumnsGenerator 生成的列名对应。
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        internal Entity ReadDataDirectly(DataRow rowData)
        {
            Entity result = null;

            var tableInfo = this.GetTableInfo();
            if (tableInfo != null)
            {
                var idName = tableInfo.Name + "_Id";
                //如果Id有值，才表明有这个对象
                if (rowData[idName] != DBNull.Value)
                {
                    //利用反射创建对象。
                    result = this._repository.New();
                    result.PersistenceStatus = PersistenceStatus.Unchanged;

                    foreach (var column in tableInfo.Columns)
                    {
                        string conventionColumnName = tableInfo.Name + "_" + column.Name;

                        column.LoadValue(result, rowData[conventionColumnName]);
                    }
                }
            }

            return result;
        }

        public string GetReadableColumnsSql(string tableAlias)
        {
            var sql = new StringBuilder();

            var tableInfo = this.GetTableInfo();
            Debug.Assert(tableInfo != null, "类型不能操作数据库！");

            tableAlias = tableAlias ?? tableInfo.Name;

            foreach (var column in tableInfo.Columns)
            {
                if (sql.Length > 0)
                {
                    sql.Append(", ");
                }
                sql.Append(tableAlias);
                sql.Append('.');
                sql.Append(column.Name);
                sql.Append(" as ");
                sql.Append(tableInfo.Name);
                sql.Append('_');
                sql.Append(column.Name);
            }

            return sql.ToString();
        }

        internal string GetReadableIdColumnSql()
        {
            var table = this.GetTableInfo();
            return table.Name + "_" + table.PKColumn.Name;
        }

        public string GetReadableColumnSql(string columnName)
        {
            return this.GetTableInfo().Name + "_" + columnName;
        }

        private RdbTable GetTableInfo()
        {
            return RdbDataProvider.Get(this._repository).DbTable;
        }
    }
}