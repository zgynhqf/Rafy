/*******************************************************
 * 
 * 作者：Steven
 * 创建日期：2006-03-17
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件、删除自定义序列化方法。 胡庆访 20130524 09:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Data;

namespace Rafy.Data
{
    /// <summary>
    /// 一个轻量级的数据表格容器。
    /// 
    /// LiteDataTable 中的空值不是 DBNull
    /// <remarks>
    /// 用于跨进程传递数据。
    /// </remarks>
    /// </summary>
    /// Title: DataContainer
    /// Author: Steven
    /// Version: 1.0
    /// History:
    ///     2006-03-17 Steven [创建] 
    [DataContract]
    [Serializable]
    public sealed class LiteDataTable
    {
        [DataMember(Name = "Columns")]
        private LiteDataColumnCollection _columns = new LiteDataColumnCollection();
        [DataMember(Name = "Rows")]
        private LiteDataRowCollection _rows = new LiteDataRowCollection();

        public LiteDataTable()
        {
            _rows = new LiteDataRowCollection
            {
                _table = this
            };
        }

        /// <summary>
        /// 列定义集合
        /// </summary>
        public LiteDataColumnCollection Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }

        /// <summary>
        /// 行集合
        /// </summary>
        public LiteDataRowCollection Rows
        {
            get { return _rows; }
        }

        /// <summary>
        /// 创建一个拥有同样列数的行对象。
        /// </summary>
        /// <returns></returns>
        public LiteDataRow NewRow()
        {
            return new LiteDataRow(this);
        }

        #region 获取、设置数据的方法

        /// <summary>
        /// 根据行号获取对应的行。
        /// </summary>
        /// <param name="rowIndex">行索引号</param>
        /// <returns></returns>
        public LiteDataRow this[int rowIndex]
        {
            get
            {
                return _rows[rowIndex];
            }
            set
            {
                _rows[rowIndex] = value;
            }
        }

        /// <summary>
        /// 根据行列号获取数据。
        /// </summary>
        /// <param name="rowIndex">行索引号</param>
        /// <param name="columnIndex">列索引号</param>
        /// <returns></returns>
        public object this[int rowIndex, int columnIndex]
        {
            get
            {
                return _rows[rowIndex][columnIndex];
            }
            set
            {
                _rows[rowIndex][columnIndex] = value;
            }
        }

        /// <summary>
        /// 根据行、列获取数据。
        /// </summary>
        /// <param name="rowIndex">行索引号</param>
        /// <param name="columnName">列名。忽略大小写。</param>
        /// <returns></returns>
        public object this[int rowIndex, string columnName]
        {
            get
            {
                int columnIndex = _columns.GetIndex(columnName);
                return this[rowIndex, columnIndex];
            }
            set
            {
                int columnIndex = _columns.GetIndex(columnName);
                this[rowIndex, columnIndex] = value;
            }
        }

        #endregion

        /// <summary>
        /// 此方法暂时不公开。以后再说。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal object[] GetRowData(int[] index)
        {
            var rows = new object[_rows.Count];
            for (int i = 0; i < _rows.Count; i++)
            {
                rows[i] = _rows[i].GetValuesByIndex(index);
            }
            return rows;
        }

        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            _rows._table = this;
            for (int i = 0, c = _rows.Count; i < c; i++)
            {
                var row = _rows[i];
                row._table = this;
            }
        }

        /// <summary>
        /// 构造一个 DataTable 并把数据拷贝到其中。
        /// </summary>
        /// <returns></returns>
        public DataTable ToDataTable()
        {
            var table = new DataTable();

            foreach (var column in _columns)
            {
                table.Columns.Add(column.ColumnName, column.Type);
            }

            foreach (var row in _rows)
            {
                var dtRow = table.NewRow();
                dtRow.ItemArray = row.Values;
                table.Rows.Add(dtRow);
            }

            return table;
        }
    }
}