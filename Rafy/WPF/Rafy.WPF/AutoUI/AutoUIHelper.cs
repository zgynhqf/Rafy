/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110316
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100316
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.WPF.Controls;

namespace Rafy.WPF
{
    /// <summary>
    /// 控件生成帮助方法
    /// </summary>
    public static class AutoUIHelper
    {
        /// <summary>
        /// 使用 View 创建一个带 “Busy” 控件的控件。
        /// </summary>
        /// <param name="mainView"></param>
        /// <returns></returns>
        public static ControlResult CreateBusyControlResult(LogicalView mainView)
        {
            var busy = TryWrapWithBusyControl(mainView);
            return new ControlResult(busy, mainView);
        }

        /// <summary>
        /// 使用 View 的数据加载器，尝试为 control 创建一个带 “Busy”控件的控件。
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static FrameworkElement TryWrapWithBusyControl(LogicalView target)
        {
            var viewController = target.DataLoader as ViewDataLoader;
            if (viewController != null)
            {
                var busy = CreateBusyControl(viewController);

                var busyContent = new Grid();
                busyContent.Children.Add(target.Control);
                busyContent.Children.Add(busy);

                return busyContent;
            }
            else
            {
                return target.Control;
            }
        }

        /// <summary>
        /// 为某个数据加载器建立一个 BusyControl。
        /// 用于在数据加载的时候，显示一个不停转动的 Busy 控件。
        /// </summary>
        /// <param name="viewController"></param>
        /// <returns></returns>
        internal static FrameworkElement CreateBusyControl(ViewDataLoader viewController)
        {
            //生成busy控件
            var busy = new BusyAnimation();

            //绑定TextBox到对象属性
            var bdIsRunning = new Binding("IsBusy");
            bdIsRunning.Source = viewController.DataProvider;
            bdIsRunning.BindsDirectlyToSource = true;

            busy.SetBinding(BusyAnimation.IsRunningProperty, bdIsRunning);

            //绑定IsVisible
            var bdIsVisible = new Binding("IsBusy");
            bdIsVisible.Source = viewController.DataProvider;
            bdIsVisible.BindsDirectlyToSource = true;
            bdIsVisible.Converter = new BooleanToVisibilityConverter();

            busy.SetBinding(BusyAnimation.VisibilityProperty, bdIsVisible);

            return busy;
        }
    }
}
