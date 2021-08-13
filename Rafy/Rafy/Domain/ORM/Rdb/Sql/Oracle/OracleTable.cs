/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120429
 * 
*******************************************************/

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.DbMigration.Oracle;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.Domain.ORM.Oracle
{
    internal class OracleTable : SqlOraTable
    {
        private OracleRunGenerator _oracleRunGenerator = new OracleRunGenerator();

        public OracleTable(IRepositoryInternal repository, string dbProvider) : base(repository, dbProvider) { }

        internal override RdbColumn CreateColumn(IRdbColumnInfo columnInfo)
        {
            return new OracleColumn(this, columnInfo);
        }

        private string _selectSequenceSql;

        public override void Insert(IDbAccesser dba, Entity item)
        {
            var idColumn = this.IdentityColumn;
            if (idColumn != null)
            {
                var idProvider = (item as IEntityWithId).IdProvider;
                var hasIdValue = idProvider.IsAvailable(item.Id);
                if (!hasIdValue)
                {
                    if (_selectSequenceSql == null)
                    {
                        //此序列是由 DbMigration 中 OracleRunGenerator 自动生成的。
                        var seqName = _oracleRunGenerator.SequenceName(this.Name, idColumn.Name);
                        _selectSequenceSql = string.Format(@"SELECT {0}.NEXTVAL FROM DUAL", seqName);
                    }

                    var sequenceValue = dba.RawAccesser.QueryValue(_selectSequenceSql);

                    sequenceValue = TypeHelper.CoerceValue(idProvider.KeyType, sequenceValue);
                    idColumn.WritePropertyValue(item, sequenceValue);

                    //如果实体的 Id 是在插入的过程中生成的，
                    //那么需要在插入组合子对象前，先把新生成的父对象 Id 都同步到子列表中。
                    item.SyncIdToChildren();
                }
            }

            base.Insert(dba, item);
        }

        public override SqlGenerator CreateSqlGenerator()
        {
            var generator = new OracleSqlGenerator();
            return generator;
        }

        public override void QueryList(IDbAccesser dba, IEntitySelectArgs args)
        {
            try
            {
                base.QueryList(dba, args);
            }
            catch (TooManyItemsInInClauseException)
            {
                /*********************** 代码块解释 *********************************
                 * 如果参数的个数过多，而当前的查询比较简单，那么就尝试分批使用参数进行查询。
                 * 这种情况主要出现在使用贪婪加载时，In 语句中的参数过多，但是查询本身非常简单，所以可以分批查询。
                **********************************************************************/
                if (!TryBatchQuery(dba, args))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 只有一些特定的查询，可以进行分批查询。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        private bool TryBatchQuery(IDbAccesser dba, IEntitySelectArgs args)
        {
            if (!PagingInfo.IsNullOrEmpty(args.PagingInfo)) { return false; }

            //分批查询的条件：WHERE 条件中只有 IN 或 NOT IN 语句。
            var query = args.Query;
            var inClause = query.Where as IColumnConstraint;
            if (inClause == null || inClause.Operator != PropertyOperator.In && inClause.Operator != PropertyOperator.NotIn)
            {
                return false;
            }

            var values = inClause.Value as IEnumerable;
            var parameters = values as IList ?? values.Cast<object>().ToArray();

            var autoSelection = AutoSelectionForLOB(query);

            var readType = autoSelection ? ReadDataType.ByIndex : ReadDataType.ByName;

            /*********************** 代码块解释 *********************************
             * 以下分批进行查询。算法：
             * 先临时把树中的条件中的值改成子集合，
             * 然后使用新的树生成对应的 Sql 语句并查询实体。
             * 所有查询完成后，再把树中的集合还原为原始的大集合。
            **********************************************************************/
            var maxItemsCount = SqlGenerator.CampatibleMaxItemsInInClause;
            var start = 0;
            var paramSection = new List<object>(maxItemsCount);
            inClause.Value = paramSection;//临时把树中的条件中的值改成子集合。
            while (start < parameters.Count)
            {
                paramSection.Clear();

                var end = Math.Min(start + maxItemsCount - 1, parameters.Count - 1);
                for (int i = start; i <= end; i++)
                {
                    paramSection.Add(parameters[i]);
                }

                //生成 Sql
                var generator = this.CreateSqlGenerator();
                QueryFactory.Instance.Generate(generator, query);
                var sql = generator.Sql;
                base.QueryDataReader(dba, args, readType, sql);

                start += paramSection.Count;
            }

            return true;
        }
    }
}