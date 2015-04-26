/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101229
 * 说明：聚合SQL实现
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101229
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rafy.Domain.ORM.SqlServer;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 聚合SQL的生成器
    /// 
    /// 使用了以下约定：
    /// 主键名是 "Id"
    /// 属性名就是列名。
    /// </summary>
    internal class AggregateSQLGenerator
    {
        private AggregateDescriptor _aggregateInfo;

        private string _whereCondition;

        private string _joinFilterCondition;

        /// <summary>
        /// 生成指定加载选项的聚合SQL。
        /// </summary>
        /// <param name="aggregate">
        /// 聚合加载选项
        /// </param>
        /// <param name="whereCondition">
        /// 简单的过滤条件，如：
        /// PBS.PBSTypeId = '...'
        /// 
        /// 如果传入Null，则默认生成以根对象为条件的格式化字符串。
        /// </param>
        /// <param name="joinFilterCondition">
        /// 用于配合where条件进行过滤的join条件
        /// select 
        /// {0},
        /// {1}
        /// from PBS 
        ///     join ProjectPBS pp on pbs.Id = pp.PBSId
        ///     left outer join PBSBQItem i on pbs.Id = i.PBSId
        /// where pp.ProjectId = '{{0}}'
        /// order by pbs.Id, i.Id"
        /// </param>
        /// <returns></returns>
        public AggregateSQLGenerator(AggregateDescriptor aggregate, string whereCondition = null, string joinFilterCondition = null)
        {
            if (aggregate == null) throw new ArgumentNullException("aggregate");
            if (aggregate.Items.Count < 1) throw new InvalidOperationException("aggregate.Items.Count < 2 must be false.");

            this._aggregateInfo = aggregate;
            this._whereCondition = whereCondition;
            this._joinFilterCondition = joinFilterCondition;
        }

        #region Generate

        private StringBuilder _sql;

        private EntityRepository _directlyQueryRepository;

        /// <summary>
        /// 生成对应的SQL
        /// </summary>
        /// <returns></returns>
        internal string Generate()
        {
            this._sql = new StringBuilder();

            this._directlyQueryRepository = this._aggregateInfo.Items.First.Value.OwnerRepository;

            this.GenerateSelect();

            this.GenerateFrom();

            this.GenerateOuterJoin();

            this.GenerateWhere();

            this.GenerateOrderBy();

            string result = this._sql.ToString();

            this._sql = null;
            this._tableAlias.Clear();
            this._whereCondition = null;
            this._directlyQueryRepository = null;

            return result;
        }

        #endregion

        #region 具体SQL生成方法

        private void GenerateSelect()
        {
            this._sql.AppendLine("SELECT");

            //生成第一个直接查询的表的所有列
            var rdp = RdbDataProvider.Get(this._directlyQueryRepository);
            var directlyQueryTableAlias = this.GetTableAlias(rdp.DbTable.Name);
            var directlyQueryTableColumns = new SQLColumnsGenerator(this._directlyQueryRepository)
                .GetReadableColumnsSql(directlyQueryTableAlias);
            this._sql.Append(directlyQueryTableColumns).Append(',').AppendLine();

            //依次添加其它关系表的所有列
            var items = this._aggregateInfo.Items;
            foreach (var item in items)
            {
                var tableName = RdbDataProvider.Get(item.PropertyEntityRepository).DbTable.Name;
                var tableAlias = GetTableAlias(tableName);
                var columns = new SQLColumnsGenerator(item.PropertyEntityRepository);
                var sqlColumns = columns.GetReadableColumnsSql(tableAlias);

                this._sql.Append(sqlColumns);
                if (item != items.Last.Value)
                {
                    this._sql.Append(',');
                }
                this._sql.AppendLine();
            }
        }

        private void GenerateFrom()
        {
            //From “第一个直接查询的表”
            var tableName = RdbDataProvider.Get(this._directlyQueryRepository).DbTable.Name;
            var tableAlias = this.GetTableAlias(tableName);
            this._sql.Append("FROM ").Append(tableName).Append(" ").AppendLine(tableAlias);

            //如果有joinFilterCondition，则添加上。
            if (this._joinFilterCondition != null)
            {
                var joinCondition = this._joinFilterCondition.Replace(tableName + '.', tableAlias + '.');

                this._sql.Append("    ").AppendLine(joinCondition);
            }
        }

        private void GenerateOuterJoin()
        {
            //依次添加其它关系表的关系约束
            var tmp = this._aggregateInfo.Items.First;
            do
            {
                var item = tmp.Value;

                //主键表和外键表的寻找有所不同：
                //如果是子对象加载类型，则分别是父表和子表
                //如果是引用对象加载类型，则主键表应该是被引用实体表
                RdbTable pkTable = null;
                RdbTable fkTable = null;
                string fkName = null;
                if (item.LoadType == AggregateLoadType.Children)
                {
                    pkTable = RdbDataProvider.Get(item.OwnerRepository).DbTable;
                    fkTable = RdbDataProvider.Get(item.PropertyEntityRepository).DbTable;
                    var parentProperty = item.PropertyEntityRepository.FindParentPropertyInfo(true);
                    var refIdProperty = (parentProperty.ManagedProperty as IRefEntityProperty).RefIdProperty;
                    fkName = refIdProperty.Name;
                }
                else
                {
                    pkTable = RdbDataProvider.Get(item.PropertyEntityRepository).DbTable;
                    fkTable = RdbDataProvider.Get(item.OwnerRepository).DbTable;
                    fkName = item.PropertyMeta.Name;
                }

                //当前的关系表的查找方法是固定的，就是属性实体。
                string propertyTableName = RdbDataProvider.Get(item.PropertyEntityRepository).DbTable.Name;

                //表所对应的别名
                string propertyTableAlias = this.GetTableAlias(propertyTableName);
                string pkTableAlias = this.GetTableAlias(pkTable.Name);
                string fkTableAlias = this.GetTableAlias(fkTable.Name);

                //组装SQL中的关系表
                this._sql.Append("    LEFT OUTER JOIN ").Append(propertyTableName)
                    .Append(" ").Append(propertyTableAlias);

                //组装SQL中的关系表的Join条件。
                this._sql.Append(" ON ").Append(fkTableAlias).Append('.').Append(fkName)
                    .Append(" = ").Append(pkTableAlias).Append('.').AppendLine(pkTable.PKColumn.Name);

                tmp = tmp.Next;
            } while (tmp != null);
        }

        private void GenerateWhere()
        {
            if (this._whereCondition != null)
            {
                //把 whereCondition 中的所有表名都进行替换。
                this._sql.Append("WHERE ");
                string condition = this._whereCondition;
                foreach (var kv in this._tableAlias)
                {
                    condition = condition.Replace(kv.Key + '.', kv.Value + '.');
                }
                this._sql.AppendLine(condition);
            }
            else if (this._directlyQueryRepository.EntityMeta.EntityCategory == EntityCategory.Child)
            {
                this._sql.Append("WHERE ");

                var parentProperty = this._directlyQueryRepository.FindParentPropertyInfo(true);
                var refIdProperty = (parentProperty.ManagedProperty as IRefEntityProperty).RefIdProperty;
                var rootTypeFKColumn = refIdProperty.Name;//属性名就是列名

                var dqTableName = RdbDataProvider.Get(this._directlyQueryRepository).DbTable.Name;
                var dqTableAlias = this.GetTableAlias(dqTableName);
                this._sql.Append(dqTableAlias).Append('.').Append(rootTypeFKColumn)
                    .AppendLine(" = {0}");
            }
        }

        private void GenerateOrderBy()
        {
            var table = RdbDataProvider.Get(this._directlyQueryRepository).DbTable;
            var directlyQueryTableAlias = this.GetTableAlias(table.Name);

            this._sql.Append("ORDER BY ").Append(directlyQueryTableAlias)
                .Append('.').Append(table.PKColumn.Name);

            var tmp = this._aggregateInfo.Items.First;
            do
            {
                var item = tmp.Value;

                var propertyTable = RdbDataProvider.Get(item.PropertyEntityRepository).DbTable;
                var propertyTableAlias = this.GetTableAlias(propertyTable.Name);

                this._sql.Append(", ").Append(propertyTableAlias).Append('.').Append(propertyTable.PKColumn.Name);

                tmp = tmp.Next;
            } while (tmp != null);
        }

        #endregion

        #region GetTableAlias

        private IDictionary<string, string> _tableAlias = new SortedDictionary<string, string>();

        /// <summary>
        /// 获取/生成表名的临时表名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private string GetTableAlias(string tableName)
        {
            string result = null;

            if (!this._tableAlias.TryGetValue(tableName, out result))
            {
                result = string.Empty;

                //抽取出所有大写的字母为最终的别名。
                var allChars = tableName.ToCharArray();
                for (int i = 0, c = allChars.Length; i < c; i++)
                {
                    var item = allChars[i];
                    if (char.IsUpper(item))
                    {
                        result += char.ToLower(item);
                    }
                }

                //在别名上加上数字。
                result += this._tableAlias.Count;

                this._tableAlias.Add(tableName, result);
            }

            return result;
        }

        #endregion
    }
}