/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110621
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110621
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OEA.Library;

namespace OEA
{
    public class SelectedEntityChangedEventArgs : EventArgs
    {
        public SelectedEntityChangedEventArgs(Entity newItem, Entity oldItem)
        {
            this.NewItem = newItem;
            this.OldItem = oldItem;
        }

        public Entity NewItem { get; private set; }

        public Entity OldItem { get; private set; }

        public ObjectView View { get; set; }
    }
}
