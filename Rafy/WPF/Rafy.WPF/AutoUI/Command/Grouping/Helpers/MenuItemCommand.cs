/*******************************************************
 * 
 * 作者：http://www.codeproject.com/Articles/25445/WPF-Command-Pattern-Applied
 * 创建时间：周金根 2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 重新整理 胡庆访 20120518
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Controls;
using System.ComponentModel;
using Rafy.WPF.Command;
using System.Windows.Automation;
using Rafy.WPF;

namespace Rafy.WPF.Command.UI
{
    public class MenuItemCommand
    {
        #region 静态配置

        static MenuItemCommand()
        {
            UseCommandImage = true;
            ToolTipStyle = CommandToolTipStyle.None;
        }

        public static bool UseCommandImage { get; set; }

        public static CommandToolTipStyle ToolTipStyle { get; set; }

        #endregion

        #region CommandProperty

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command", typeof(ICommand), typeof(MenuItemCommand),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

        public static ICommand GetCommand(DependencyObject sender)
        {
            return sender.GetValue(CommandProperty) as ICommand;
        }

        public static void SetCommand(DependencyObject sender, ICommand command)
        {
            sender.SetValue(CommandProperty, command);
        }

        #endregion

        private static void OnCommandChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            MenuItem menuItem = dependencyObject as MenuItem;
            if (menuItem == null) { throw new InvalidOperationException("menu item required"); }

            var command = e.NewValue as UICommand;
            if (command == null) { throw new InvalidOperationException("CommandAdapter required"); }

            menuItem.Name = ClientCommand.GetProgrammingName(command.ClientCommand);
            menuItem.Command = command;
            AutomationProperties.SetName(menuItem, command.ClientCommand.Label);

            SetupContent(menuItem, command);

            CommandToolTipService.SetupTooltip(menuItem, command.ClientCommand, ToolTipStyle);
        }

        private static void SetupContent(MenuItem menuItem, UICommand command)
        {
            menuItem.DataContext = command.ClientCommand;
            menuItem.SetBinding(MenuItem.HeaderProperty, "Label");

            if (UseCommandImage && menuItem.Icon == null && command != null)
            {
                Uri imageUri = CommandImageService.GetCommandImageUri(command);
                if (imageUri != null)
                {
                    try
                    {
                        menuItem.Icon = new Image
                        {
                            Stretch = Stretch.None,
                            Source = new BitmapImage(imageUri)
                        };
                    }
                    catch { }
                }
            }
        }
    }
}