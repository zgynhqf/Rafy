/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20170104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20170104 10:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Data;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.MySql
{
    /// <summary>
    /// MySql的表对象
    /// </summary>

    internal sealed class MySqlTable : SqlOraTable
    {
        /// <summary>
        /// 构造函数 初始化仓库对象
        /// </summary>
        /// <param name="repository">仓库对象</param>
        public MySqlTable(IRepositoryInternal repository) : base(repository)
        {
            _insertSql = new Lazy<string>(() =>
            {
                var generatedSql = this.GenerateInsertSQL(false);
                return $@"{generatedSql};
SELECT @@IDENTITY;";
            });
        }

        /// <summary>
        /// 创建Sql生成器对象
        /// </summary>
        /// <returns></returns>
        public override SqlGenerator CreateSqlGenerator()
        {
            return new MySqlSqlGenerator();
        }

        /// <summary>
        /// 插入操作
        /// </summary>
        /// <param name="dba">数据库操作对象</param>
        /// <param name="item">要保存的实体对象</param>
        public override void Insert(IDbAccesser dba, Entity item)
        {
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                var idProvider = (item as IEntityWithId).IdProvider;
                var hasId = idProvider.IsAvailable(item.Id);

                string insertSql = hasId ? _insertSqlWithId.Value : _insertSql.Value;
                var parameters = this.GenerateInsertSqlParameters(item, hasId);

                var idValue = dba.QueryValue(insertSql, parameters);

                if (!hasId)
                {
                    idValue = TypeHelper.CoerceValue(idProvider.KeyType, idValue);
                    idColumn.LoadValue(item, idValue);

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

        /// <summary>
        /// 创建MySql的分页语句
        /// </summary>
        /// <param name="parts">原始语句</param>
        /// <param name="pagingInfo">分页对象</param>
        protected override void CreatePagingSql(ref PagingSqlParts parts, PagingInfo pagingInfo)
        {
            var pageNumber = pagingInfo.PageNumber;
            var pageSize = pagingInfo.PageSize;

            var sql = new StringBuilder("SELECT * FROM (");

            sql.AppendLine().Append(parts.RawSql)
                .Append(") T ")
                .Append("limit ").Append((pageNumber - 1) * pageSize)
                .Append(",").Append(pageSize);

            parts.PagingSql = sql.ToString();
        }
    }
}