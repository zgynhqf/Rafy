/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101028
 * 说明：批量插入实体的实现
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101028
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using System.Data;
using System.Data.SqlClient;
using OEA.ORM;

namespace OEA.Library
{
    /// <summary>
    /// 批量插入实体的类
    /// </summary>
    internal class BatchInsert
    {
        private IList<Entity> _entityList;

        private SqlConnection _db;

        private ITable _table;

        /// <summary>
        /// 为指定的实体列表构造一个批处理命令。
        /// </summary>
        /// <param name="entityList"></param>
        /// <param name="db"></param>
        /// <param name="table"></param>
        public BatchInsert(IList<Entity> entityList, SqlConnection db, ITable table)
        {
            if (!OEAEnvironment.Location.IsOnServer()) throw new InvalidOperationException("!OEAEnvironment.IsOnServer() must be false.");
            if (entityList.Count < 1) throw new ArgumentOutOfRangeException();

            this._table = table;
            this._entityList = entityList;
            this._db = db;
        }

        /// <summary>
        /// 执行插入命令
        /// </summary>
        public void Execute()
        {
            //创建表格式
            DataTable table = new DataTable();
            var columns = this._table.Columns;
            foreach (var column in columns)
            {
                var dataType = column.DataType;
                if (dataType.IsGenericType && dataType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    dataType = dataType.GetGenericArguments()[0];
                }
                table.Columns.Add(new DataColumn(column.Name, dataType));
            }

            //从实体中读取数据
            var rows = table.Rows;
            for (int i = 0, c = this._entityList.Count; i < c; i++)
            {
                var entity = this._entityList[i];
                var row = table.NewRow();
                var j = 0;
                foreach (var column in columns)
                {
                    row[j++] = column.GetValue(entity) ?? DBNull.Value;
                }
                rows.Add(row);
            }

            this.SaveBulk(table);
        }

        /// <summary>
        /// 保存数据到数据库中。
        /// </summary>
        /// <param name="table"></param>
        private void SaveBulk(DataTable table)
        {
            var sqlConn = this._db;
            var bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.CheckConstraints, null);
            bulkCopy.DestinationTableName = this._table.Name;
            bulkCopy.BatchSize = table.Rows.Count;

            var mappings = bulkCopy.ColumnMappings;

            bool opened = false;

            try
            {
                if (sqlConn.State != ConnectionState.Open)
                {
                    opened = true;
                    sqlConn.Open();
                }

                this.SetMappings(mappings);

                bulkCopy.WriteToServer(table);
            }
            finally
            {
                if (opened)
                {
                    sqlConn.Close();
                }

                if (bulkCopy != null)
                {
                    bulkCopy.Close();
                }
            }
        }

        /// <summary>
        /// 设置DataTable到数据库中表的映射
        /// </summary>
        /// <param name="mappings"></param>
        private void SetMappings(SqlBulkCopyColumnMappingCollection mappings)
        {
            var cmd = this._db.CreateCommand();
            cmd.CommandText = string.Format("select top 1 * from [{0}]", this._table.Name);
            var dbNames = new List<string>();
            using (var reader = cmd.ExecuteReader())
            {
                for (int i = 0, c = reader.FieldCount; i < c; i++)
                {
                    var name = reader.GetName(i);
                    dbNames.Add(name);
                }
            }
            foreach (var column in this._table.Columns)
            {
                var correspondingDbName = dbNames.First(c => string.Compare(c, column.Name, true) == 0);
                mappings.Add(column.Name, correspondingDbName);
            }
        }
    }

    #region BatchInsert_By_GenerateSQL

    //internal class BatchInsert_By_GenerateSQL
    //{
    //    private IList<Entity> _entityList;

    //    private IDb _db;

    //    private SqlTable _table;

    //    public BatchInsert_By_GenerateSQL(IList<Entity> entityList, IDb db, SqlTable table)
    //    {
    //        if (!OEAEnvironment.IsOnServer()) throw new InvalidOperationException("!OEAEnvironment.IsOnServer() must be false.");
    //        if (entityList.Count < 1) throw new ArgumentOutOfRangeException();

    //        this._table = table;
    //        this._entityList = entityList;
    //        this._db = db;
    //    }

    //    public void Execute()
    //    {
    //        var sql = this.GenerateSQL();
    //        this._db.ExecSql(sql);
    //    }

    //    /// <summary>
    //    /// 生成格式：
    //    /// Insert into [TableName] ([Column1],[Column2]...) values
    //    /// (c1Value,'c2Value',...),
    //    /// (c1Value,'c2Value',...),
    //    /// (c1Value,'c2Value',...)
    //    /// </summary>
    //    /// <returns></returns>
    //    private string GenerateSQL()
    //    {
    //        //Insert into [TableName]
    //        var sql = new StringBuilder("Insert into ");
    //        sql.Append("[");
    //        sql.Append(this._table.Name);
    //        sql.Append("](");

    //        //([Column1],[Column2]...) values
    //        var columns = this._table.Columns;
    //        for (int i = 0, c = columns.Length; i < c; i++)
    //        {
    //            var column = columns[i];
    //            if (i != 0)
    //            {
    //                sql.Append(',');
    //            }
    //            sql.Append('[');
    //            sql.Append(column.Name);
    //            sql.Append(']');
    //        }
    //        sql.Append(") values ");
    //        sql.AppendLine();

    //        //生成所有实体的数据SQL
    //        var valueHost = new ColumnValueHost();
    //        for (int i = 0, c = this._entityList.Count; i < c; i++)
    //        {
    //            var entity = this._entityList[i];
    //            if (i != 0)
    //            {
    //                sql.Append(',');
    //                sql.AppendLine();
    //            }
    //            sql.Append('(');

    //            //生成一个实体的SQL
    //            for (int j = 0, c2 = columns.Length; j < c2; j++)
    //            {
    //                if (j != 0)
    //                {
    //                    sql.Append(',');
    //                }
    //                var column = columns[j] as SqlColumn;
    //                column.SetParameterValue(valueHost, entity);
    //                var value = valueHost.Value;
    //                if (value == DBNull.Value)
    //                {
    //                    sql.Append("NULL");
    //                }
    //                else if (value is Guid || value is string || value is char)
    //                {
    //                    sql.Append('\'');
    //                    sql.Append(value);
    //                    sql.Append('\'');
    //                }
    //                else if (value is bool)
    //                {
    //                    sql.Append(((bool)value) ? '1' : '0');
    //                }
    //                else if (value is Enum)
    //                {
    //                    sql.Append((int)value);
    //                }
    //                else
    //                {
    //                    sql.Append(value);
    //                }
    //            }

    //            sql.Append(')');
    //        }

    //        sql.Append(';');

    //        return sql.ToString();
    //    }

    //    private class ColumnValueHost : IDbDataParameter
    //    {
    //        public object Value { get; set; }

    //        #region 没用到，没实现的字段

    //        public byte Precision
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public byte Scale
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public int Size
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public DbType DbType
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public ParameterDirection Direction
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public bool IsNullable
    //        {
    //            get { throw new NotImplementedException(); }
    //        }

    //        public string ParameterName
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public string SourceColumn
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        public DataRowVersion SourceVersion
    //        {
    //            get
    //            {
    //                throw new NotImplementedException();
    //            }
    //            set
    //            {
    //                throw new NotImplementedException();
    //            }
    //        }

    //        #endregion
    //    }
    //}

    #endregion
}