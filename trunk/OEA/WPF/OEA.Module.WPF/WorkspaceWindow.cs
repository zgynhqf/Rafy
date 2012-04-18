/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120418
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120418
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 工作区窗口的可用属性
    /// </summary>
    public static class WorkspaceWindow
    {
        #region Title AttachedDependencyProperty

        public static readonly DependencyProperty TitleProperty = DependencyProperty.RegisterAttached(
            "Title", typeof(string), typeof(WorkspaceWindow)
            );

        public static string GetTitle(FrameworkElement element)
        {
            return (string)element.GetValue(TitleProperty);
        }

        public static void SetTitle(FrameworkElement element, string value)
        {
            element.SetValue(TitleProperty, value);
        }

        #endregion
    }
}
