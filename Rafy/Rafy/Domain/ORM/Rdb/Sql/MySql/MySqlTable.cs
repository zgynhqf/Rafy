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
        private string _insertSql;

        private string _withIdInsertSql;

        /// <summary>
        /// 构造函数 初始化仓库对象
        /// </summary>
        /// <param name="repository">仓库对象</param>
        public MySqlTable(IRepositoryInternal repository) : base(repository) { }

        /// <summary>
        /// 创建Sql生成器对象
        /// </summary>
        /// <returns></returns>
        public override SqlGenerator CreateSqlGenerator()
        {
            return new MySqlGenerator();
        }

        /// <summary>
        /// 追加引用符号
        /// </summary>
        /// <param name="sql">sql文本写入器</param>
        /// <param name="identifier">标识符</param>
        internal override void AppendQuote(TextWriter sql, string identifier)
        {
            sql.Write("`");
            this.AppendPrepare(sql, identifier);
            sql.Write("`");
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

                var isIdentityHasValue = (item as IEntityWithId).IdProvider.IsAvailable(item.Id);
                string insertSql = isIdentityHasValue ? _withIdInsertSql ?? (_withIdInsertSql = this.GenerateInsertSQL(true)) :
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