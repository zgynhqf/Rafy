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
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using Rafy.WPF.Controls;

namespace Rafy.WPF
{
    /// <summary>
    /// 视图 - TabControl
    /// 适配器
    /// </summary>
    internal static class ViewAdapter
    {
        /// <summary>
        /// 把view的行为适配到control上。
        /// 同时也把control的行为适配到view上。
        /// 
        /// LogicalView.IsVisible 和 TabItem.Visibility 的关系：
        /// 在目标上，两者实现双向绑定。
        /// 1. LogicalView.IsVisible 的改变，直接通过 IsVisibleChanged 事件的处理函数来设置对应的 TabItem 的 Visibility 属性。
        /// 2. TabItem.Visibility 先使用 OnWay 的 Binding 来直接绑定在实体类父对象的某个属性上，实现动态可见性。
        ///     然后，在 Binding 的 UpdateTarget 事件处理函数中，再设置 LogicalView.IsVisible 的值。
        ///     代码位于 ListLogicalView.ResetVisibility() 方法中。
        /// </summary>
        /// <param name="childView"></param>
        /// <param name="control"></param>
        public static void AdaptView(LogicalView childView, TabItem control)
        {
            //IsActive
            control.IsSelected = childView.IsActive;
            childView.IsActiveChanged += (o, e) =>
            {
                control.IsSelected = childView.IsActive;
            };

            //IsVisible
            control.Visibility = childView.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            childView.IsVisibleChanged += (o, e) =>
            {
                var tabControl = control.GetLogicalParent<TabControl>();
                if (tabControl != null)
                {
                    if (childView.IsVisible)
                    {
                        control.Visibility = Visibility.Visible;
                        tabControl.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        control.Visibility = Visibility.Collapsed;

                        if (tabControl.Items.OfType<TabItem>().All(i => i.Visibility == Visibility.Collapsed))
                        {
                            tabControl.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 把 childrenTab 的选择项同步到 View.IsActive 属性上。
        /// </summary>
        /// <param name="parentView"></param>
        /// <param name="childrenTab"></param>
        public static void AdaptView(LogicalView parentView, TabControl childrenTab)
        {
            //任何一个子 View 可见，整个控件都可见
            childrenTab.Visibility = parentView.ChildrenViews.Any(v => v.IsVisible) ?
                Visibility.Visible : Visibility.Collapsed;

            //在选择状态发生改变时，设置每个view的Active状态
            childrenTab.SelectionChanged += (sender, e) =>
            {
                //设置每个view的Active状态
                if (sender == e.OriginalSource && e.AddedItems.Count > 0)
                {
                    foreach (TabItem item in e.AddedItems)
                    {
                        var childView = WPFMeta.GetLogicalView(item);
                        childView.IsActive = true;
                    }

                    foreach (TabItem item in e.RemovedItems)
                    {
                        var childView = WPFMeta.GetLogicalView(item);
                        childView.IsActive = false;
                    }

                    e.Handled = true;
                }
            };
        }
    }
}
