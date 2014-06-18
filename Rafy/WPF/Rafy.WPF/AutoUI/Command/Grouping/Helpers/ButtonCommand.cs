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
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Command.UI
{
    public class ButtonCommand
    {
        #region 静态配置

        static ButtonCommand()
        {
            ToolTipStyle = CommandToolTipStyle.EnabledWithKeyGesture;
        }

        public static CommandToolTipStyle ToolTipStyle { get; set; }

        #endregion

        #region CommandProperty

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
                "Command", typeof(ICommand), typeof(ButtonCommand),
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
            Button button = dependencyObject as Button;
            if (button == null) { throw new InvalidOperationException("button required"); }

            var command = e.NewValue as UICommand;
            if (command == null) { throw new InvalidOperationException("CommandAdapter required"); }

            button.Name = ClientCommand.GetProgrammingName(command.ClientCommand);
            button.Command = command;
            AutomationProperties.SetName(button, command.ClientCommand.Label);

            SetupContent(button, command);

            CommandToolTipService.SetupTooltip(button, command.ClientCommand, ToolTipStyle);
        }

        private static void SetupContent(Button button, UICommand command)
        {
            var contentControl = new CommandContentControl();
            contentControl.SetBinding(CommandContentControl.LabelProperty, "Label");
            contentControl.DataContext = command.ClientCommand;

            var uri = CommandImageService.GetCommandImageUri(command);
            if (uri != null) { contentControl.ImageSource = new BitmapImage(uri); }

            button.Content = contentControl;
        }
    }
}