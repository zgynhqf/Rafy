using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace OEA.Module.WPF
{
    /// <summary>
    /// ObjectView 使用的逻辑控件
    /// </summary>
    public interface IWPFControlWrapper : IControlWrapper
    {
        new FrameworkElement Control { get; }
        event MouseButtonEventHandler MouseDoubleClick;

        //void NotifyMouseDoubleClick(object sender, MouseButtonEventArgs e);
        //void NotifySelectedItemChanged(object sender, SelectedItemChangedEventArgs e);
    }
    public interface IWPFObjectView : IObjectView
    {
        /// <summary>
        /// 逻辑上的控件
        /// </summary>
        new IWPFControlWrapper Control { get; }
    }


    public interface IWPFControlGenerator : IControlGenerator
    {
        new FrameworkElement CreateControl();
    }
}