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
using System.Linq;
using System.Text;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 程序集内部使用的事件“监听器”
    /// 
    /// 本对象需要其它对象来通知某些事件的发生。
    /// </summary>
    public interface IEventListener
    {
        /// <summary>
        ///  通知这个 ObjectView 控件发生了 MouseDoubleClick 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NotifyMouseDoubleClick(object sender, EventArgs e);

        /// <summary>
        ///  通知这个 ObjectView 控件发生了 SelectedItemChanged 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NotifySelectedItemChanged(object sender, SelectedEntityChangedEventArgs e);

        /// <summary>
        ///  通知这个 ObjectView 控件发生了 CheckChanged 事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void NotifyCheckChanged(object sender, CheckChangedEventArgs e);
    }
}
