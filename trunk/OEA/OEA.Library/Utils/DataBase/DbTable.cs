/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100402
 * 说明：此文件包括表格数据的类型。表、子表等。
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100402
 * 
*******************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace OEA.Library
{
    /// <summary>
    /// 一个存储表格数据的对象
    /// 
    /// 注意：
    /// 以此为参数的方法只能在服务端执行
    /// </summary>
    public interface IDbTable : IEnumerable<DataRow>
    {
        /// <summary>
        /// 行数
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取指定的行。
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        DataRow this[int rowIndex] { get; }
    }

    /// <summary>
    /// 封装了DataRowCollection的一般Table
    /// </summary>
    public class DbTable : IDbTable
    {
        private DataRowCollection _table;

        public DbTable(DataTable table)
        {
            if (table == null) throw new ArgumentNullException("table");

            this._table = table.Rows;
        }

        public DataRowCollection Rows
        {
            get
            {
                return this._table;
            }
        }

        #region IDbTable Members

        public int Count
        {
            get
            {
                return this._table.Count;
            }
        }

        public DataRow this[int rowIndex]
        {
            get
            {
                return this._table[rowIndex];
            }
        }

        #endregion

        #region IEnumerable<DataRow> Members

        IEnumerator<DataRow> IEnumerable<DataRow>.GetEnumerator()
        {
            return new TableEnumerator
            {
                _table = this
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableEnumerator
            {
                _table = this
            };
        }

        #endregion
    }

    /// <summary>
    /// 这是个子表格。
    /// 
    /// 它表示的是某一表格中的一些指定的行。
    /// </summary>
    public class SubTable : IDbTable
    {
        private IDbTable _table;

        private int _startRow;

        private int _endRow;

        /// <summary>
        /// 构造一个指定table的子表。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="startRow">这个表在table中的开始行。</param>
        /// <param name="endRow">这个表在table中的结束行。</param>
        public SubTable(IDbTable table, int startRow, int endRow)
        {
            if (table == null) throw new ArgumentNullException("table");
            if (startRow < 0 || startRow >= table.Count) throw new InvalidOperationException("startRow < 0 || startRow >= table.RowsCount must be false.");
            if (endRow >= table.Count) throw new InvalidOperationException("endRow >= table.RowsCount must be false.");

            this._table = table;
            this._startRow = startRow;
            this._endRow = endRow;
        }

        #region IDbTable Members

        public int Count
        {
            get
            {
                return this._endRow - this._startRow + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowIndex">
        /// 这里的索引其实是相对startRow的偏移量。
        /// </param>
        /// <returns></returns>
        public DataRow this[int rowIndex]
        {
            get
            {
                if (rowIndex < 0 || rowIndex >= this.Count) throw new InvalidOperationException("rowIndex < 0 || rowIndex >= this.RowsCount must be false.");

                int realIndex = this._startRow + rowIndex;

                return this._table[realIndex];
            }
        }

        #endregion

        #region IEnumerable<DataRow> Members

        IEnumerator<DataRow> IEnumerable<DataRow>.GetEnumerator()
        {
            return new TableEnumerator
            {
                _table = this
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TableEnumerator
            {
                _table = this
            };
        }

        #endregion
    }

    internal class TableEnumerator : IEnumerator<DataRow>
    {
        internal IDbTable _table;

        private int _curIndex;

        public DataRow Current
        {
            get { return this._table[this._curIndex]; }
        }

        public void Dispose() { }

        object IEnumerator.Current
        {
            get { return this.Current; }
        }

        public bool MoveNext()
        {
            this._curIndex++;
            return this._table.Count > this._curIndex + 1;
        }

        public void Reset()
        {
            this._curIndex = 0;
        }
    }
}