/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150815
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150815 18:02
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Rafy.Data;
using Rafy.DbMigration.Oracle;
using Rafy.Domain.ORM.Oracle;
using Rafy.ManagedProperty;

namespace Rafy.Domain.ORM.BatchSubmit.Oracle
{
    /// <summary>
    /// Oracle 数据库的实体批量导入器
    /// 使用 ODP.NET 中的批量导入功能完成。
    /// </summary>
    [Serializable]
    public class OracleBatchImporter : BatchImporter
    {
        private static OracleRunGenerator _oracleRunGenerator = new OracleRunGenerator();

        /// <summary>
        /// Initializes a new instance of the <see cref="OracleBatchImporter"/> class.
        /// </summary>
        public OracleBatchImporter()
        {
            this.BatchSize = 100000;
            this.SqlGenerator = new OracleSqlGenerator();
        }

        /// <summary>
        /// 每次导入时，一批最多同时导入多少条数据。
        /// </summary>
        /// <exception cref="System.InvalidOperationException">ORACLE 中每批次最多只能导入 100000 行数据。</exception>
        public override int BatchSize
        {
            get { return base.BatchSize; }
            set
            {
                if (value > 100000) throw new InvalidOperationException("ORACLE 中每批次最多只能导入 100000 行数据。");
                base.BatchSize = value;
            }
        }

        #region ImportInsert

        /// <summary>
        /// 子类重写此方法实现批量插入逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected override void ImportInsert(EntityBatch batch)
        {
            //为所有实体生成 Id
            if (batch.Table.IdentityColumn != null)
            {
                this.GenerateId(batch);
            }

            //分批插入实体，每批最多十万条数据
            foreach (var section in this.EnumerateAllBatches(batch.InsertBatch))
            {
                this.ImportBatch(batch, section);
            }
        }

        /// <summary>
        /// 为所有的实体生成 Id。
        /// </summary>
        /// <param name="batch"></param>
        private void GenerateId(EntityBatch batch)
        {
            var dba = batch.DBA;

            //如果批量生成 Id 使用的序列号太低，则需要抛出异常。
            var seqName = _oracleRunGenerator.SequenceName(batch.Table.Name, batch.Table.IdentityColumn.Name);
            var incrementBy = Convert.ToInt32(dba.QueryValue(
                "SELECT INCREMENT_BY FROM ALL_SEQUENCES WHERE SEQUENCE_NAME = {0} AND SEQUENCE_OWNER = {1}",
                seqName,
                _oracleRunGenerator.IdentifierQuoter.Prepare(DbConnectionSchema.GetOracleUserId(dba.ConnectionSchema))
                ));
            if (incrementBy < 100) { throw new InvalidOperationException(string.Format("使用批量保存，表 {0} 的序列 {1} 的每次递增数不能少于 100。建议在数据库生成完成后使用 Rafy.Domain.ORM.BatchSubmit.Oracle.OracleBatchImporter.EnableBatchSequence() 来变更序列的每次递增数以批量生成实体聚合中的所有 Id 标识。", batch.Table.Name, seqName)); }

            //由于每次生成的 Id 号数有限，所以需要分批生成 Id
            var nextSeqValueSql = string.Format("SELECT {0}.NEXTVAL FROM DUAL", seqName);
            foreach (var section in EnumerateAllBatches(batch.InsertBatch, incrementBy))
            {
                var nextValue = Convert.ToInt64(dba.QueryValue(nextSeqValueSql));
                var startId = nextValue - incrementBy + 1;
                for (int i = 0, c = section.Count; i < c; i++)
                {
                    var item = section[i];
                    if (!((IEntityWithId)item).IdProvider.IsAvailable(item.Id))
                    {
                        item.Id = startId++;
                    }
                }
            }
        }

        private void ImportBatch(EntityBatch meta, IList<Entity> entities)
        {
            var rowsCount = entities.Count;

            var sql = GenerateInsertSQL(meta.Table);

            var parameters = GenerateInsertParameters(meta, entities);

            //设置 ArrayBindCount 后再执行，就是批量导入功能。
            var command = meta.DBA.RawAccesser.CommandFactory.CreateCommand(sql, CommandType.Text, parameters);
            (command as OracleCommand).ArrayBindCount = rowsCount;

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 生成 Insert 语句
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private static string GenerateInsertSQL(RdbTable table)
        {
            //代码参考 RdbTable.GenerateInsertSQL() 方法。
            var sql = new StringWriter();
            sql.Write("INSERT INTO ");
            sql.AppendQuote(table, table.Name).Write("(");

            var columns = table.Columns;

            bool comma = false;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                if (comma) { sql.Write(','); }
                else { comma = true; }

                sql.AppendQuote(table, columns[i].Name);
            }

            sql.Write(") VALUES (");

            comma = false;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                if (comma) { sql.Write(','); }
                else { comma = true; }

                sql.Write(":");
                sql.Write(columns[i].Name);
            }

            sql.Write(")");

            return sql.ToString();
        }

        /// <summary>
        /// 生成与 Insert 语句相匹配的参数列表。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private static IDbDataParameter[] GenerateInsertParameters(EntityBatch meta, IList<Entity> entities)
        {
            var dba = meta.DBA.RawAccesser;
            var table = meta.Table;

            //把所有实体中所有属性的值读取到数组中，参数的值就是这个数组。
            var parameters = new List<IDbDataParameter>();
            var columns = table.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var parameter = ReadIntoBatchParameter(entities, columns[i], dba);
                parameters.Add(parameter);
            }
            return parameters.ToArray();
        }

        #region EnableBatchSequence

        /// <summary>
        /// 为指定的聚合启用批量插入。
        /// 此方法会更改整个聚合中所有实体对应的序列的每次递增数为 sequenceStep。
        /// 调用时机：在数据库生成完成后调用。
        /// 原因：在 ORACLE 中，如果要批量插入实体，则需要先把实体对应的 Sequence 变为以 sequenceStep 为每次递增。
        /// 副作用：这会导致不使用批量插入功能时，实体的 Id 号变为 100000,200000,300000 这样的形式递增。
        /// </summary>
        /// <param name="aggtRepo">The aggt repo.</param>
        /// <param name="sequenceStep">The sequence step.</param>
        public static void EnableBatchSequence(IRepository aggtRepo, int sequenceStep = 100000)
        {
            if (sequenceStep < 100) throw new ArgumentOutOfRangeException("sequenceStep");

            using (var dba = RdbDataProvider.Get(aggtRepo).CreateDbAccesser())
            {
                foreach (var repo in DomainHelper.EnumerateAllTypesInAggregation(aggtRepo))
                {
                    var table = RdbDataProvider.Get(repo).DbTable;
                    var seqName = _oracleRunGenerator.SequenceName(table.Name, table.IdentityColumn.Name);
                    dba.ExecuteText(string.Format("ALTER SEQUENCE {0} INCREMENT BY {1} NOCACHE", seqName, sequenceStep));
                }
            }
        }

        #endregion

        #region //旧的生成 Id 的操作

        /*********************** 代码块解释 *********************************
         * 由于 ORACLE 中使用 DDL 语句会隐式提交事务，所以下面的方法不再使用。
        **********************************************************************/

        ///// <summary>
        ///// 为所有的实体生成 Id。
        ///// </summary>
        ///// <param name="batch"></param>
        //private void GenerateId(EntityBatch batch)
        //{
        //    var entities = batch.InsertBatch;

        //    var startId = GetBatchIDs(batch.DBA, batch.Table, entities.Count);

        //    for (int i = 0, c = entities.Count; i < c; i++)
        //    {
        //        var item = entities[i];
        //        item.Id = startId++;
        //    }
        //}

        ///// <summary>
        ///// 获取指定大小批量的连续的 Id 号。返回第一个 Id 号的值。
        ///// </summary>
        ///// <param name="dba">The dba.</param>
        ///// <param name="table">The table.</param>
        ///// <param name="size">需要连续的多少个 Id 号。</param>
        ///// <returns></returns>
        //private static int GetBatchIDs(IDbAccesser dba, RdbTable table, int size)
        //{
        //    var nextValue = 0;

        //    var seqName = OracleMigrationProvider.SequenceName(table.Name, table.IdentityColumn.Name);
        //    try
        //    {
        //        //先把 STEP 改为 10 万，再获取下一个加了 10 万的值，这样不会有并发问题。
        //        dba.ExecuteText(string.Format("ALTER SEQUENCE {0} INCREMENT BY {1} NOCACHE", seqName, size));
        //        nextValue = Convert.ToInt64(dba.QueryValue(string.Format("SELECT {0}.NEXTVAL FROM DUAL", seqName)));
        //    }
        //    finally
        //    {
        //        dba.ExecuteText(string.Format("ALTER SEQUENCE {0} INCREMENT BY 1 NOCACHE", seqName));
        //    }

        //    return nextValue - size + 1;
        //} 

        #endregion

        #endregion

        #region ImportUpdate

        /// <summary>
        /// 子类重写此方法实现批量更新逻辑。
        /// </summary>
        /// <param name="batch"></param>
        protected override void ImportUpdate(EntityBatch batch)
        {
            //分批插入实体，每批最多十万条数据
            foreach (var section in this.EnumerateAllBatches(batch.UpdateBatch))
            {
                this.ImportUpdate(batch, section);
            }
        }

        private void ImportUpdate(EntityBatch meta, IList<Entity> entities)
        {
            //生成 Sql
            var sql = this.GenerateUpdateSQL(meta.Table);

            //生成对应的参数列表。
            var parameters = this.GenerateUpdateParameters(meta, entities);

            //设置 ArrayBindCount 后再执行，就是批量导入功能。
            var command = meta.DBA.RawAccesser.CommandFactory.CreateCommand(sql, CommandType.Text, parameters);
            (command as OracleCommand).ArrayBindCount = entities.Count;

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// 生成 Update 语句。
        /// 注意，此方法不会更新 LOB 字段。
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private string GenerateUpdateSQL(RdbTable table)
        {
            //代码参考 RdbTable.GenerateUpdateSQL() 方法。

            var sql = new StringWriter();
            sql.Write("UPDATE ");
            sql.AppendQuoteName(table);
            sql.Write(" SET ");

            bool comma = false;

            var updateLOB = this.UpdateLOB;

            var columns = table.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i];
                if (!column.Info.IsPrimaryKey && (updateLOB || !column.IsLOB))
                {
                    if (comma) { sql.Write(','); }
                    else { comma = true; }

                    sql.AppendQuote(table, column.Name).Write(" = :");
                    sql.Write(column.Name);
                }
            }

            sql.Write(" WHERE ");
            sql.AppendQuote(table, table.PKColumn.Name);
            sql.Write(" = :");
            sql.Write(table.PKColumn.Name);

            return sql.ToString();
        }

        /// <summary>
        /// 生成与 Sql 配套的参数列表。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="entities"></param>
        /// <returns></returns>
        private IDbDataParameter[] GenerateUpdateParameters(EntityBatch meta, IList<Entity> entities)
        {
            var dba = meta.DBA.RawAccesser;
            var table = meta.Table;
            var updateLOB = this.UpdateLOB;

            //把所有实体中所有属性的值读取到数组中，参数的值就是这个数组。
            var parameters = new List<IDbDataParameter>();
            var columns = table.Columns;
            for (int i = 0, c = columns.Count; i < c; i++)
            {
                var column = columns[i];
                if (!column.Info.IsPrimaryKey && (updateLOB || !column.IsLOB))
                {
                    var parameter = ReadIntoBatchParameter(entities, column, dba);
                    parameters.Add(parameter);
                }
            }

            //主键列放在最后。
            var pkParameter = ReadIntoBatchParameter(entities, table.PKColumn, dba);
            parameters.Add(pkParameter);
            return parameters.ToArray();
        }

        #endregion

        #region 帮助方法

        private static IDbDataParameter ReadIntoBatchParameter(IList<Entity> entities, RdbColumn column, IRawDbAccesser dba)
        {
            var parameter = dba.ParameterFactory.CreateParameter();
            parameter.ParameterName = column.Name;

            #region 对于 CLOB 类型，需要特殊处理。

            var dbType = column.Info.ColumnMeta.DataType ?? OracleDbTypeHelper.ConvertFromCLRType(column.Info.DataType);
            /*********************** 代码块解释 *********************************
             * 不再需要对 CLOB 类型特殊处理
             * 
             * 这是因为：某个字段使用 CLOB 类型后，如果再使用 BatchInsert 的功能，如果数据条数较多时（1000条未再现，10000条时出现），
             * oracle.exe 会一直占用 100% CPU，而且一直运行，不再返回（目前还不知道什么原因）。
             * 
             * 另外，如果不使用 OracleDbType.Clob 来标记 OracleParameter，而只是使用 DbType.String，也可以正确的运行。
            **********************************************************************/
            //bool setOracleDbTypeEx = false;
            //if (dbType == DbType.String || dbType == DbType.AnsiString)
            //{
            //    //对于 CLOB 类型，需要特殊处理。
            //    var length = column.Info.ColumnMeta.DataTypeLength;
            //    if (OracleDbTypeHelper.CLOBTypeName.EqualsIgnoreCase(length) || "MAX".EqualsIgnoreCase(length))
            //    {
            //        (parameter as OracleParameter).OracleDbTypeEx = OracleDbType.Clob;
            //        setOracleDbTypeEx = true;
            //    }
            //}
            //else if (dbType == DbType.Binary)
            //{
            //    (parameter as OracleParameter).OracleDbTypeEx = OracleDbType.Blob;
            //    setOracleDbTypeEx = true;
            //}
            //if (!setOracleDbTypeEx)
            //{
            parameter.DbType = dbType;
            //}

            #endregion

            int rowsCount = entities.Count;
            var valueArray = new object[rowsCount] as IList;
            for (int j = 0; j < rowsCount; j++)
            {
                var entity = entities[j];
                var value = column.ReadParameterValue(entity);
                valueArray[j] = value;
            }

            parameter.Value = valueArray;
            return parameter;
        }

        #endregion
    }
}