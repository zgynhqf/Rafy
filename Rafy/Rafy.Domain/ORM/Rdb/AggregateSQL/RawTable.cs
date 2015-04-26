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
    /// 封装了DataRowCollection的一般Table
    /// </summary>
    internal class RawTable : IDataTable
    {
        private DataRowCollection _table;

        public RawTable(DataTable table)
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
}