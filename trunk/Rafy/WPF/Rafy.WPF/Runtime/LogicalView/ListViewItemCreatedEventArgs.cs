/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130514
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130514 11:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// 列表视图创建新实体时的事件参数。
    /// </summary>
    public class ListViewItemCreatedEventArgs : EventArgs
    {
        public ListViewItemCreatedEventArgs(Entity item)
        {
            this.Item = item;
        }

        /// <summary>
        /// 新创建的实体。
        /// </summary>
        public Entity Item { get; private set; }
    }
}
