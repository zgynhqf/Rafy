//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20101028
// * 说明：批量插入实体的实现
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20101028
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Rafy.MetaModel;
//using System.Data;
//using System.Data.SqlClient;
//using Rafy.Domain.ORM;
//using Rafy.Domain;
//using Rafy.Data;

//namespace Rafy.Domain.ORM.BatchSubmit.SqlServer
//{
//    /// <summary>
//    /// 批量插入实体的类
//    /// </summary>
//    internal class BatchInsert
//    {
//        private BatchImporter _importer;
//        private EntityBatch _meta;
//        private IList<Entity> _entityList;
//        private RdbTable _table;

//        /// <summary>
//        /// 为指定的实体列表构造一个批处理命令。
//        /// </summary>
//        /// <param name="batch">The batch.</param>
//        /// <param name="importer">The importer.</param>
//        public BatchInsert(EntityBatch batch, BatchImporter importer)
//        {
//            _importer = importer;
//            _entityList = batch.InsertBatch;
//            _meta = batch;
//            _table = batch.Table;
//        }

//        /// <summary>
//        /// 执行插入命令
//        /// </summary>
//        public void Execute()
//        {
//            if (_table.IdentityColumn != null)
//            {
//                this.GenerateId();
//            }

//            foreach (var batch in _importer.EnumerateAllBatches(_entityList))
//            {
//                var table = this.ToDataTable(batch);

//                this.SaveBulk(table);
//            }
//        }

//        #region 自动生成 Id

//        private static object _identityLock = new object();

//        private void GenerateId()
//        {
//            var startId = GetBatchIDs(_meta.DBA, _meta.Table, _entityList.Count);

//            for (int i = 0, c = _entityList.Count; i < c; i++)
//            {
//                var item = _entityList[i];
//                item.Id = startId++;
//            }
//        }

//        /// <summary>
//        /// 获取指定大小批量的连续的 Id 号。返回第一个 Id 号的值。
//        /// </summary>
//        /// <param name="dba">The dba.</param>
//        /// <param name="table">The table.</param>
//        /// <param name="size">需要连续的多少个 Id 号。</param>
//        /// <returns></returns>
//        private static int GetBatchIDs(IDbAccesser dba, RdbTable table, int size)
//        {
//            /*********************** 代码块解释 *********************************
//             * 算法解释：
//             * 1.先查出当前最大的 ID 值；
//             * 2.计算出所需要使用的区间的起始、终止值 [start,end]；
//             * 3.然后设置 IDENTITY 的种子值为终止值；
//             * 4.由于上两步 SQL 执行过程中有可能其它线程使用了这个 IDENTITY 而造成 [start,end] 中的值被使用，
//             * 所以需要对表中的数据进行检测，如果没有被使用，才把这些 ID 值返回。
//             * 如果被使用了，则上述过程重来一次即可。
//             * 
//             * IDENTITY 的操作方法：http://www.cnblogs.com/gaizai/archive/2013/04/23/3038318.html
//            **********************************************************************/

//            var tableName = table.Name;

//            lock (_identityLock)
//            {
//                while (true)
//                {
//                    var currentValue = Convert.ToInt32(dba.QueryValue(string.Format("SELECT IDENT_CURRENT('{0}')", tableName)));
//                    var start = currentValue + 1;
//                    var end = start + size - 1;
//                    dba.ExecuteText(string.Format("DBCC CHECKIDENT('{0}', RESEED, {1})", tableName, end));

//                    var concurrency = dba.QueryValue(string.Format(
//                        "SELECT 1 WHERE EXISTS (SELECT 1 FROM {0} WHERE {1} >= {2} AND {1} <= {3})",
//                        tableName, table.IdentityColumn.Name, start, end
//                        ));
//                    if (concurrency == null) return start;
//                }
//            }

//            throw new InvalidOperationException("");
//        }

//        #endregion

//        private DataTable ToDataTable(IList<Entity> list)
//        {
//            //创建表格式
//            var table = new DataTable();
//            var columns = _table.Columns;
//            foreach (var column in columns)
//            {
//                var dataType = column.Info.DataType;
//                if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>))
//                {
//                    dataType = dataType.GetGenericArguments()[0];
//                }
//                table.Columns.Add(new DataColumn(column.Name, dataType));
//            }

//            //从实体中读取数据
//            var rows = table.Rows;
//            for (int i = 0, c = list.Count; i < c; i++)
//            {
//                var entity = list[i];
//                var row = table.NewRow();
//                var j = 0;
//                foreach (var column in columns)
//                {
//                    row[j++] = column.ReadParameterValue(entity) ?? DBNull.Value;
//                }
//                rows.Add(row);
//            }

//            return table;
//        }

//        /// <summary>
//        /// 保存数据到数据库中。
//        /// </summary>
//        /// <param name="table"></param>
//        private void SaveBulk(DataTable table)
//        {
//            var bulkCopy = new SqlBulkCopy(
//                _meta.DBA.Connection as SqlConnection,
//                SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.CheckConstraints,
//                _meta.DBA.RawAccesser.Transaction as SqlTransaction
//                );
//            bulkCopy.DestinationTableName = _table.Name;
//            bulkCopy.BatchSize = table.Rows.Count;

//            try
//            {
//                this.SetMappings(bulkCopy.ColumnMappings);

//                bulkCopy.WriteToServer(table);
//            }
//            finally
//            {
//                if (bulkCopy != null)
//                {
//                    bulkCopy.Close();
//                }
//            }
//        }

//        /// <summary>
//        /// 设置DataTable到数据库中表的映射
//        /// </summary>
//        /// <param name="mappings"></param>
//        private void SetMappings(SqlBulkCopyColumnMappingCollection mappings)
//        {
//            foreach (var column in _table.Columns)
//            {
//                mappings.Add(column.Name, column.Name);
//            }

//            //暂留：通过查询的真实列名来实现 Mapping。
//            //var sql = string.Format("SELECT TOP 0 * FROM [{0}]", _table.Name);
//            //var dbNames = new List<string>();
//            //using (var reader = _meta.DBA.QueryDataReader(sql))
//            //{
//            //    for (int i = 0, c = reader.FieldCount; i < c; i++)
//            //    {
//            //        var name = reader.GetName(i);
//            //        dbNames.Add(name);
//            //    }
//            //}
//            //foreach (var column in _table.Columns)
//            //{
//            //    var correspondingDbName = dbNames.First(c => string.Compare(c, column.Name, true) == 0);
//            //    mappings.Add(column.Name, correspondingDbName);
//            //}
//        }
//    }

//    #region BatchInsert_By_GenerateSQL

//    //internal class BatchInsert_By_GenerateSQL
//    //{
//    //    private IList<Entity> _entityList;

//    //    private IDb _db;

//    //    private SqlTable _table;

//    //    public BatchInsert_By_GenerateSQL(IList<Entity> entityList, IDb db, SqlTable table)
//    //    {
//    //        if (!RafyEnvironment.IsOnServer()) throw new InvalidOperationException("!RafyEnvironment.IsOnServer() must be false.");
//    //        if (entityList.Count < 1) throw new ArgumentOutOfRangeException();

//    //        this._table = table;
//    //        this._entityList = entityList;
//    //        this._db = db;
//    //    }

//    //    public void Execute()
//    //    {
//    //        var sql = this.GenerateSQL();
//    //        this._db.ExecSql(sql);
//    //    }

//    //    /// <summary>
//    //    /// 生成格式：
//    //    /// Insert into [TableName] ([Column1],[Column2]...) values
//    //    /// (c1Value,'c2Value',...),
//    //    /// (c1Value,'c2Value',...),
//    //    /// (c1Value,'c2Value',...)
//    //    /// </summary>
//    //    /// <returns></returns>
//    //    private string GenerateSQL()
//    //    {
//    //        //Insert into [TableName]
//    //        var sql = new StringBuilder("Insert into ");
//    //        sql.Append("[");
//    //        sql.Append(this._table.Name);
//    //        sql.Append("](");

//    //        //([Column1],[Column2]...) values
//    //        var columns = this._table.Columns;
//    //        for (int i = 0, c = columns.Length; i < c; i++)
//    //        {
//    //            var column = columns[i];
//    //            if (i != 0)
//    //            {
//    //                sql.Append(',');
//    //            }
//    //            sql.Append('[');
//    //            sql.Append(column.Name);
//    //            sql.Append(']');
//    //        }
//    //        sql.Append(") values ");
//    //        sql.AppendLine();

//    //        //生成所有实体的数据SQL
//    //        var valueHost = new ColumnValueHost();
//    //        for (int i = 0, c = this._entityList.Count; i < c; i++)
//    //        {
//    //            var entity = this._entityList[i];
//    //            if (i != 0)
//    //            {
//    //                sql.Append(',');
//    //                sql.AppendLine();
//    //            }
//    //            sql.Append('(');

//    //            //生成一个实体的SQL
//    //            for (int j = 0, c2 = columns.Length; j < c2; j++)
//    //            {
//    //                if (j != 0)
//    //                {
//    //                    sql.Append(',');
//    //                }
//    //                var column = columns[j] as SqlColumn;
//    //                column.SetParameterValue(valueHost, entity);
//    //                var value = valueHost.Value;
//    //                if (value == DBNull.Value)
//    //                {
//    //                    sql.Append("NULL");
//    //                }
//    //                else if (value is Guid || value is string || value is char)
//    //                {
//    //                    sql.Append('\'');
//    //                    sql.Append(value);
//    //                    sql.Append('\'');
//    //                }
//    //                else if (value is bool)
//    //                {
//    //                    sql.Append(((bool)value) ? '1' : '0');
//    //                }
//    //                else if (value is Enum)
//    //                {
//    //                    sql.Append((int)value);
//    //                }
//    //                else
//    //                {
//    //                    sql.Append(value);
//    //                }
//    //            }

//    //            sql.Append(')');
//    //        }

//    //        sql.Append(';');

//    //        return sql.ToString();
//    //    }

//    //    private class ColumnValueHost : IDbDataParameter
//    //    {
//    //        public object Value { get; set; }

//    //        #region 没用到，没实现的字段

//    //        public byte Precision
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public byte Scale
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public int Size
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public DbType DbType
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public ParameterDirection Direction
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public bool IsNullable
//    //        {
//    //            get { throw new NotImplementedException(); }
//    //        }

//    //        public string ParameterName
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public string SourceColumn
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        public DataRowVersion SourceVersion
//    //        {
//    //            get
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //            set
//    //            {
//    //                throw new NotImplementedException();
//    //            }
//    //        }

//    //        #endregion
//    //    }
//    //}

//    #endregion
//}