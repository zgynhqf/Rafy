/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130123 10:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130123 10:49
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Domain.ORM.SqlServer;
using Rafy.Utils;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.SqlCe
{
    internal class SqlCeTable : SqlTable
    {
        public SqlCeTable(IRepositoryInternal repository) : base(repository) { }

        public override void Insert(IDbAccesser dba, Entity item)
        {
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                var idProvider = (item as IEntityWithId).IdProvider;
                var hasId = idProvider.IsAvailable(item.Id);

                string insertSql = hasId ? _insertSqlWithId.Value : _insertSql.Value;
                object[] parameters = this.GenerateInsertSqlParameters(item, hasId);

                if (hasId)
                {
                    //打开 Identity 列可以手动插入
                    dba.RawAccesser.ExecuteText($"SET IDENTITY_INSERT {Name} ON ");
                }

                dba.ExecuteText(insertSql, parameters);

                if (!hasId)
                {
                    var idValue = dba.RawAccesser.QueryValue("SELECT @@IDENTITY;");
                    idValue = TypeHelper.CoerceValue(idProvider.KeyType, idValue);
                    idColumn.WritePropertyValue(item, idValue);

                    //如果实体的 Id 是在插入的过程中生成的，
                    //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                    item.SyncIdToChildren();
                }
                else
                {
                    //关闭 Identity 列可以手动插入
                    dba.RawAccesser.ExecuteText($"SET IDENTITY_INSERT {Name} OFF ");
                }
            }
            else
            {
                base.Insert(dba, item);
            }
        }

        /// <summary>
        /// 在 sqlce 下，不支持 rowNumber 方案，但是支持 not in 方案。
        /// 鉴于实现 not in 方案比较耗时，所以暂时决定使用 IDataReader 分页完成。
        /// 
        /// not in 分页，参见以下 Sql：
        /// select top 10 [AuditItem].* from 
        /// [AuditItem] where 
        /// [AuditItem].id not in
        /// (
        ///     select top 100 [AuditItem].id from [AuditItem] order by LogTime desc
        /// )
        /// order by LogTime desc
        /// </summary>
        protected override PagingLocation GetPagingLocation(PagingInfo pagingInfo)
        {
            if (!PagingInfo.IsNullOrEmpty(pagingInfo) && pagingInfo.PageNumber == 1 && !pagingInfo.IsNeedCount)
            {
                return PagingLocation.Database;
            }
            return PagingLocation.Memory;
        }
    }
}