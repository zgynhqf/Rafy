using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Reflection;

using OEA.Module.WPF.Controls;
using System.Windows.Controls;

namespace OEA.Module.WPF
{
    public static class WPFExtension
    {
        /// <summary>
        /// 找到最上层的IFrameTemplate
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEntityWindow GetWorkspaceWindow(this FrameworkElement element)
        {
            DependencyObject currentElement = element;

            while (currentElement != null)
            {
                var win = currentElement as IEntityWindow;
                if (win != null) return win;

                currentElement = LogicalTreeHelper.GetParent(currentElement) ??
                    VisualTreeHelper.GetParent(currentElement);
            }

            return null;
        }

        /// <summary>
        /// 获取某个 view 所在的 工作区页签
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static IEntityWindow GetWorkspaceWindow(this ObjectView view)
        {
            return (view.Control as FrameworkElement).GetWorkspaceWindow();
        }
    }
}
