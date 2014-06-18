/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121029 10:56
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121029 10:56
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF
{
    /// <summary>
    /// 工作区的窗口定义。
    /// </summary>
    public class WorkspaceWindow
    {
        private FrameworkElement _windowControl;

        internal WorkspaceWindow() { }

        public WorkspaceWindow(string title, FrameworkElement windowControl)
        {
            if (string.IsNullOrEmpty(title)) throw new ArgumentNullException("title");
            if (windowControl == null) throw new ArgumentNullException("windowControl");

            this.Title = title;
            this.WindowControl = windowControl;
        }

        /// <summary>
        /// 窗口显示的标题
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// 模块对应的窗口控件
        /// </summary>
        public FrameworkElement WindowControl
        {
            get { return this._windowControl; }
            internal set
            {
                this._windowControl = value;

                //设置关系
                SetWorkspaceWindow(value, this);
            }
        }

        #region WorkspaceWindow AttachedDependencyProperty

        /// <summary>
        /// 获取 <see cref="WindowControl"/> 属性表示的控件所在的 <see cref="WorkspaceWindow"/>
        /// </summary>
        public static readonly DependencyProperty WorkspaceWindowProperty = DependencyProperty.RegisterAttached(
            "WorkspaceWindow", typeof(WorkspaceWindow), typeof(WorkspaceWindow)
            );

        /// <summary>
        /// 获取 <see cref="WindowControl"/> 属性表示的控件所在的 <see cref="WorkspaceWindow"/>
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static WorkspaceWindow GetWorkspaceWindow(DependencyObject element)
        {
            return (WorkspaceWindow)element.GetValue(WorkspaceWindowProperty);
        }

        private static void SetWorkspaceWindow(DependencyObject element, WorkspaceWindow value)
        {
            element.SetValue(WorkspaceWindowProperty, value);
        }

        #endregion

        /// <summary>
        /// 找到指定控件外层中最近的一个 WorkspaceWindow
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static WorkspaceWindow GetOuterWorkspaceWindow(DependencyObject element)
        {
            var currentElement = element;

            while (currentElement != null)
            {
                var win = GetWorkspaceWindow(currentElement);
                if (win != null) return win;

                currentElement = LogicalTreeHelper.GetParent(currentElement) ??
                    VisualTreeHelper.GetParent(currentElement);
            }

            return null;
        }
    }
}