/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

using Rafy.WPF;

namespace Rafy
{
    /// <summary>
    /// 区域（有名字的控件块）。
    /// </summary>
    [DebuggerDisplay("Name:{Name} Label:{Label}")]
    public class Region
    {
        public Region(string name, string childrenLabel, ControlResult control)
            : this(name, control)
        {
            this.ChildrenLabel = childrenLabel;
        }

        public Region(string name, ControlResult control)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("name");
            if (control == null) throw new ArgumentNullException("control");

            this.Name = name;
            this.ControlResult = control;
        }

        /// <summary>
        /// 此控件的名字。
        /// 
        /// 在布局时，会在控件组中挑选合适名字的控件，然后放入合适的位置进行布局。
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 控件
        /// </summary>
        public ControlResult ControlResult { get; private set; }

        /// <summary>
        /// 如果当前区域是一个聚合子实体区域，则这个属性表示该子区域应该显示的标题。
        /// </summary>
        public string ChildrenLabel { get; set; }
    }
}
