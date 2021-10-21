/*******************************************************
* 
* 作者：胡庆访
* 创建时间：20101028
* 说明：批量插入实体的实现
* 运行环境：.NET 4.0
* 版本号：1.0.0
* 
* 历史记录：
* 创建文件 BatchInsert.cs 胡庆访 20101028
* 合并到 SqlBatchImporter 中。 胡庆访 20130416 15:35
* 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.SqlServer;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.Domain.ORM;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.BatchSubmit.SqlServer
{
    /// <summary>
    /// SqlServer 数据库的实体批量导入器。
    /// </summary>
    [Serializable]
    public class SqlServerBatchImporter : BatchImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerBatchImporter"/> class.
        /// </summary>
        public SqlServerBatchImporter()
        {
            this.SqlGenerator = new SqlServerSqlGenerator();
        }

        #region ImportInsert

        /// <summary>
        /// 批量导入指定的实体或列表。
        /// </summary>
        /// <param name="batch"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override void ImportInsert(EntityBatch batch)
        {
            var entities = batch.InsertBatch;

            if (batch.Table.IdentityColumn != null)
            {
                this.GenerateId(batch, entities);
            }

            foreach (var section in this.EnumerateAllBatches(entities))
            {
                var table = ToDataTable(batch.Table, section);

                this.SaveBulk(table, batch);
            }
        }

        #region 自动生成 Id

        private static object _identityLock = new object();

        internal virtual void GenerateId(EntityBatch meta, IList<Entity> entities)
        {
            var startId = GetBatchIDs(meta.DBA, meta.Table, entities.Count);

            for (int i = 0, c = entities.Count; i < c; i++)
            {
                var item = entities[i];
                if (!((IEntityWithId)item).IdProvider.IsAvailable(item.Id))
                {
                    item.Id = startId++;
                }
            }
        }

        /// <summary>
        /// 获取指定大小批量的连续的 Id 号。返回第一个 Id 号的值。
        /// </summary>
        /// <param name="dba">The dba.</param>
        /// <param name="table">The table.</param>
        /// <param name="size">需要连续的多少个 Id 号。</param>
        /// <returns></returns>
        private static long GetBatchIDs(IDbAccesser dba, RdbTable table, int size)
        {
            /*********************** 代码块解释 *********************************
             * 算法解释：
             * 1.先查出当前最大的 ID 值；
             * 2.计算出所需要使用的区间的起始、终止值 [start,end]；
             * 3.然后设置 IDENTITY 的种子值为终止值；
             * 4.由于上两步 SQL 执行过程中有可能其它线程使用了这个 IDENTITY 而造成 [start,end] 中的值被使用，
             * 所以需要对表中的数据进行检测，如果没有被使用，才把这些 ID 值返回。
             * 如果被使用了，则上述过程重来一次即可。
             * 
             * IDENTITY 的操作方法：http://www.cnblogs.com/gaizai/archive/2013/04/23/3038318.html
            **********************************************************************/

            var tableName = table.Name;

            lock (_identityLock)
            {
                while (true)
                {
                    var currentValue = Convert.ToInt64(dba.QueryValue(string.Format("SELECT IDENT_CURRENT('{0}')", tableName)));
                    var start = currentValue + 1;
                    var end = start + size - 1;
                    dba.ExecuteText(string.Format("DBCC CHECKIDENT('{0}', RESEED, {1})", tableName, end));

                    var concurrency = dba.QueryValue(string.Format(
                        "SELECT 1 WHERE EXISTS (SELECT 1 FROM {0} WHERE {1} >= {2} AND {1} <= {3})",
                        tableName, table.IdentityColumn.Name, start, end
                        ));
                    if (concurrency == null) return start;
                }
            }

            throw new InvalidOperationException("生成 Id 时，发生未知错误。");
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="list"></param>
        /// <param name="isUpdating"></param>
        /// <returns></returns>
        internal DataTable ToDataTable(RdbTable meta, IList<Entity> list, bool isUpdating = false)
        {
            //创建表格式
            var table = new DataTable();
            var columns = new List<RdbColumn>();

            if (list.Count > 0)
            {
                var first = list[0];//第一个实体的禁用状态，就表示了整个列表的禁用状态。

                var updateLOB = this.UpdateLOB;
                foreach (var column in meta.Columns)
                {
                    if (!first.IsDisabled(column.Info.Property) && (!isUpdating || updateLOB || !column.IsLOB))
                    {
                        var dataType = TypeHelper.IgnoreNullable(column.Info.PropertyType);
                        table.Columns.Add(new DataColumn(column.Name, dataType));
                        columns.Add(column);
                    }
                }
            }

            //从实体中读取数据
            var rows = table.Rows;
            for (int i = 0, c = list.Count; i < c; i++)
            {
                var entity = list[i];
                var row = table.NewRow();
                for (int j = 0, jc = columns.Count; j < jc; j++)
                {
                    var column = columns[j];
                    var property = column.Info.Property;
                    if (entity.IsDisabled(property)) ThrowInvalidPropertyException(entity, property, i);
                    row[j] = column.ReadDbParameterValue(entity);
                }
                rows.Add(row);

                if (isUpdating)
                {
                    row.AcceptChanges();
                    row.SetModified();
                }
            }

            return table;
        }

        /// <summary>
        /// 保存数据到数据库中。
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="meta">The meta.</param>
        /// <param name="keepIdentity">The meta.</param>
        internal void SaveBulk(DataTable table, EntityBatch meta, bool keepIdentity = true)
        {
            var opt = SqlBulkCopyOptions.CheckConstraints;
            if (keepIdentity) opt |= SqlBulkCopyOptions.KeepIdentity;

            using (var bulkCopy = new SqlBulkCopy(
                meta.DBA.Connection as SqlConnection,
                opt,
                meta.DBA.RawAccesser.Transaction as SqlTransaction
                ))
            {
                bulkCopy.DestinationTableName = meta.Table.Name;
                bulkCopy.BatchSize = table.Rows.Count;
                bulkCopy.BulkCopyTimeout = 10 * 60;

                try
                {
                    this.SetMappings(bulkCopy.ColumnMappings, table);

#if NET45
                    bulkCopy.WriteToServer(table);
#endif
#if NETSTANDARD2_0
                var reader = new DataTableReader(table);
                bulkCopy.WriteToServer(reader);
#endif
                }
                finally
                {
                    if (bulkCopy != null)
                    {
                        bulkCopy.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 设置DataTable到数据库中表的映射
        /// </summary>
        /// <param name="mappings">The mappings.</param>
        /// <param name="table">The table.</param>
        private void SetMappings(SqlBulkCopyColumnMappingCollection mappings, DataTable table)
        {
            foreach (DataColumn column in table.Columns)
            {
                mappings.Add(column.ColumnName, column.ColumnName);
            }

            //暂留：通过查询的真实列名来实现 Mapping。
            //var sql = string.Format("SELECT TOP 0 * FROM [{0}]", _table.Name);
            //var dbNames = new List<string>();
            //using (var reader = _meta.DBA.QueryDataReader(sql))
            //{
            //    for (int i = 0, c = reader.FieldCount; i < c; i++)
            //    {
            //        var name = reader.GetName(i);
            //        dbNames.Add(name);
            //    }
            //}
            //foreach (var column in _table.Columns)
            //{
            //    var correspondingDbName = dbNames.First(c => string.Compare(c, column.Name, true) == 0);
            //    mappings.Add(column.Name, correspondingDbName);
            //}
        }

        #endregion

        #region ImportUpdate

        /// <summary>
        /// 批量导入指定的实体或列表。
        /// </summary>
        /// <param name="batch"></param>
        protected override void ImportUpdate(EntityBatch batch)
        {
            var table = ToDataTable(batch.Table, batch.UpdateBatch, true);
            var updateColumns = table.Columns.Cast<DataColumn>()//顺序必须一致。
                .Select(dtc => batch.Table.Columns.First(c => c.Name == dtc.ColumnName))
                .Where(c => !c.Info.IsPrimaryKey)//不更新主键
                .ToList();

            var sql = GenerateUpdateSQL(batch.Table, this.UpdateLOB, '@', updateColumns);

            //生成对应的参数列表。
            var parameters = this.GenerateUpdateParameters(batch, updateColumns);

            var command = batch.DBA.RawAccesser.CommandFactory.CreateCommand(sql, CommandType.Text, parameters);
            var adapter = new SqlDataAdapter();
            adapter.UpdateCommand = command as SqlCommand;
            adapter.UpdateBatchSize = this.BatchSize;
            adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;

            adapter.Update(table);
        }

        /// <summary>
        /// 生成与 Sql 配套的参数列表。
        /// </summary>
        /// <param name="batch">The meta.</param>
        /// <param name="dataColumns"></param>
        /// <returns></returns>
        private IDbDataParameter[] GenerateUpdateParameters(EntityBatch batch, IReadOnlyList<RdbColumn> dataColumns)
        {
            var dba = batch.DBA.RawAccesser;
            var table = batch.Table;

            //把所有实体中所有属性的值读取到数组中，参数的值就是这个数组。
            var parameters = new List<IDbDataParameter>();

            for (int i = 0, c = dataColumns.Count; i < c; i++)
            {
                var column = dataColumns[i];
                var parameter = dba.ParameterFactory.CreateParameter();
                parameter.ParameterName = '@' + column.Name;
                parameter.SourceColumn = column.Name;//额外地，需要设置 SourceColumn
                parameter.DbType = column.Info.DbType;
                parameters.Add(parameter);
            }

            //主键列放在最后。
            var pkParameter = dba.ParameterFactory.CreateParameter();
            pkParameter.ParameterName = '@' + table.PKColumn.Name;
            pkParameter.SourceColumn = table.PKColumn.Name;
            pkParameter.DbType = table.PKColumn.Info.DbType;
            parameters.Add(pkParameter);

            return parameters.ToArray();
        }

        #endregion
    }
}
