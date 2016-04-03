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

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 这是个子表格。
    /// 
    /// 它表示的是某一表格中的一些指定的行。
    /// </summary>
    internal class SubTable : IDataTable
    {
        private IDataTable _table;

        private int _startRow;

        private int _endRow;

        /// <summary>
        /// 构造一个指定table的子表。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="startRow">这个表在table中的开始行。</param>
        /// <param name="endRow">这个表在table中的结束行。</param>
        public SubTable(IDataTable table, int startRow, int endRow)
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
}