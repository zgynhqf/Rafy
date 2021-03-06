﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：单个命令的文件菜单项生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System.Windows;
using System.Windows.Controls;
using Rafy.WPF.Command;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 单个命令的文件菜单项生成器
    /// 
    /// 生成TextBox + MenuItem
    /// </summary>
    public class TextBoxMenuItemGenerator : ItemGenerator
    {
        /// <summary>
        /// 生成 TextBox + MenuItem
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement CreateCommandUI(ClientCommand command)
        {
            var tb = CreateTextBox(command);

            //使用MenuItem加入
            MenuItem menuItem = this.CreateMenuItem(command);

            var panel = new DockPanel();

            panel.Children.Add(menuItem);
            panel.Children.Add(new Separator());
            panel.Children.Add(tb);

            return panel;
        }
    }
}
