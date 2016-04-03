/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140625
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140625 16:50
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy
{
    /// <summary>
    /// 这个类解决了基类在 Clear 时，不提供所有 OldItems 的问题。
    /// </summary>
    public interface INotifyChangedCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged
    {
        ///// <summary>
        ///// 被清空的项
        ///// </summary>
        //IList ClearedItems { get; }

        /// <summary>
        /// 获取被清空的项，并清空这个缓存。
        /// </summary>
        IList PopClearedItems();
    }
}
