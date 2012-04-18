/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110426
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110426
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OEA.Module.WPF
{
    public class WorkspaceWindowChangedEventArgs : EventArgs
    {
        public WorkspaceWindowChangedEventArgs(FrameworkElement deactiveWindow, FrameworkElement activeWindow)
        {
            this.DeactiveWindow = deactiveWindow;
            this.ActiveWindow = activeWindow;
        }

        public FrameworkElement DeactiveWindow { get; private set; }

        public FrameworkElement ActiveWindow { get; private set; }
    }
}
