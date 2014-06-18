/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121011 13:52
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121011 13:52
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF.Controls
{
    /// <summary>Defines the state or role of a <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> control.</summary>
    public enum TreeGridColumnHeaderRole
    {
        /// <summary>The column header displays above its associated column.</summary>
        Normal,
        /// <summary>The column header is the object of a drag-and-drop operation to move a column.</summary>
        Floating,
        /// <summary>The column header is the last header in the row of column headers and is used for padding.</summary>
        Padding
    }
}
