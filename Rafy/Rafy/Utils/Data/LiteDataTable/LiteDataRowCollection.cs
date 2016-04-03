/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130524
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130524 12:00
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Rafy.Data
{
    [CollectionDataContract]
    [Serializable]
    public class LiteDataRowCollection : Collection<LiteDataRow>
    {
        internal LiteDataTable _table;

        //protected override void InsertItem(int index, LiteDataRow item)
        //{
        //    if (item == null) throw new ArgumentNullException("item");

        //    item._table = _table;

        //    base.InsertItem(index, item);
        //}

        //protected override void SetItem(int index, LiteDataRow item)
        //{
        //    if (item == null) throw new ArgumentNullException("item");

        //    item._table = _table;

        //    base.SetItem(index, item);
        //}
    }
}