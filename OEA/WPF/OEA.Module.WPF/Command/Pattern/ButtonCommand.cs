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
using System.Windows.Automation;
using OEA.WPF.Command;
using OEA.WPF;
using System.Collections.Generic;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    public class ButtonCommand
    {
        #region 静态配置

        static ButtonCommand()
        {
            UseCommandImage = true;
            ToolTipStyle = CommandToolTipStyle.EnabledWithKeyGesture;
            DisabledOpacity = 0.25;
        }

        public static bool UseCommandImage { get; set; }

        public static CommandToolTipStyle ToolTipStyle { get; set; }

        public static double DisabledOpacity { get; set; }

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

            button.Name = "btn" + command.CoreCommand.ProgramingName;
            button.Command = command;
            AutomationProperties.SetName(button, command.CoreCommand.Label);

            SetupContent(button, command);

            CommandToolTipService.SetupTooltip(button, command, ToolTipStyle);
        }

        private static void SetupContent(Button button, UICommand command)
        {
            button.DataContext = command.CoreCommand;

            bool setImage = false;

            // image
            if (UseCommandImage)
            {
                Uri imageUri = CommandImageService.GetCommandImageUri(command);
                if (imageUri != null)
                {
                    try
                    {
                        Label label = null;

                        button.Content = new StackPanel()
                        {
                            Orientation = Orientation.Horizontal,
                            Children =
                            {
                                new Image
                                {
                                    //if (bitmap.CanFreeze) bitmap.Freeze();//Freeze bitmap it to avoid memory leak
                                    Source = new BitmapImage(imageUri),
                                    Stretch = Stretch.None//图形不拉伸
                                },
                                (label = new Label() 
                                { 
                                    Padding = new Thickness(0) 
                                })
                            }
                        };

                        label.SetBinding(Label.ContentProperty, "Label");
                        setImage = true;
                    }
                    catch { }
                }
            }

            if (!setImage)
            {
                button.SetBinding(Button.ContentProperty, "Label");
            }
        }
    }
}