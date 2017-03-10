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
        private string _insertSql;

        private string _withIdInsertSql;

        public SqlServerTable(IRepositoryInternal repository) : base(repository) { }

        public override void Insert(IDbAccesser dba, Entity item)
        {
            //如果有 Id 列，那么需要在执行 Insert 的同时，执行 SELECT @@IDENTITY。
            //在为 SQL Server 插入数据时，执行 Insert 的同时，必须同时执行 SELECT @@IDENTITY。否则会有多线程问题。
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                var isIdentityHasValue = (item as IEntityWithId).IdProvider.IsAvailable(item.Id);
                string insertSql = isIdentityHasValue ? _withIdInsertSql ?? (_withIdInsertSql = $" SET IDENTITY_INSERT {this.Name} ON ;{this.GenerateInsertSQL(true)};SET IDENTITY_INSERT {this.Name} OFF ;") :
                    _insertSql ?? (_insertSql = $"{this.GenerateInsertSQL()};SELECT @@IDENTITY;");

                var parameters = Columns.Where(c => c.CanInsert || (c.Info.IsIdentity && isIdentityHasValue))
                                 .Select(c => c.ReadParameterValue(item))
                                 .ToArray();

                //由于默认是 decimal 类型，所以需要类型转换。
                var idValue = dba.QueryValue(insertSql, parameters);
                if (!isIdentityHasValue)
                {
                    idValue = TypeHelper.CoerceValue((item as IEntityWithId).IdProvider.KeyType, idValue);
                    idColumn.LoadValue(item, idValue);
                }

                //如果实体的 Id 是在插入的过程中生成的，
                //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                item.SyncIdToChildren();
            }
            else
            {
                base.Insert(dba, item);
            }
        }
    }
}
