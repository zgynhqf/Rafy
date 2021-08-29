/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150729
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150729 11:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.SqlServer
{
    class SqlServerTable : SqlTable
    {
        public SqlServerTable(IRepositoryInternal repository, string dbProvider) : base(repository, dbProvider)
        {
            _insertSql = new Lazy<string>(() =>
            {
                var generatedSql = this.GenerateInsertSQL(false);
                return $@"{generatedSql};
SELECT @@IDENTITY";
            });
            _insertSqlWithId = new Lazy<string>(() =>
            {
                var generatedSql = this.GenerateInsertSQL(true);
                return $@"SET IDENTITY_INSERT {this.Name} ON;
{generatedSql};
SET IDENTITY_INSERT {this.Name} OFF";
            });
        }

        public override void Insert(IDbAccesser dba, Entity item)
        {
            //如果有 Id 列，那么需要在执行 Insert 的同时，执行 SELECT @@IDENTITY。
            //在为 SQL Server 插入数据时，执行 Insert 的同时，必须同时执行 SELECT @@IDENTITY。否则会有多线程问题。
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                var idProvider = (item as IEntityWithId).IdProvider;
                var hasId = idProvider.IsAvailable(item.Id);

                string insertSql = hasId ? _insertSqlWithId.Value : _insertSql.Value;
                object[] parameters = this.GenerateInsertSqlParameters(item, hasId);

                var idValue = dba.QueryValue(insertSql, parameters);

                if (!hasId)
                {
                    idValue = TypeHelper.CoerceValue(idProvider.KeyType, idValue);
                    idColumn.WritePropertyValue(item, idValue);

                    //如果实体的 Id 是在插入的过程中生成的，
                    //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                    item.SyncIdToChildren();
                }
            }
            else
            {
                base.Insert(dba, item);
            }
        }
    }
}