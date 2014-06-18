/*******************************************************
 * 
 * 作者：hardcodet
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 hardcodet 2008
 * 2.0 胡庆访 20120911 14:42
 * 
*******************************************************/

using System.Windows;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// Event arguments for the <see cref="TreeViewBase{object}.SelectedItemChangedEvent"/>
    /// routed event.
    /// </summary>
    /// <typeparam name="object">The type of the tree's items.</typeparam>
    public class SelectedItemChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// The currently selected item that caused the event. If
        /// the tree's <see cref="TreeViewBase{object}.SelectedItem"/>
        /// property is null, so is this parameter.
        /// </summary>
        public object NewItem { get; internal set; }

        /// <summary>
        /// The previously selected item, if any. Might be null
        /// if no item was selected before.
        /// </summary>
        public object OldItem { get; internal set; }
    }
}
