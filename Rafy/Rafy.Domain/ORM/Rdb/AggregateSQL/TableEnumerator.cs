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
    internal class TableEnumerator : IEnumerator<DataRow>
    {
        internal IDataTable _table;

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