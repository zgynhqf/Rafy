/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：单个命令的文本按钮生成器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Itenso.Windows.Input;
using OEA.WPF.Command;
using System.Windows.Media;
using System.Windows.Automation;
using System.Windows.Input;
using OEA.Command;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 单个命令的文本按钮生成器
    /// 
    /// 生成TextBox + Button
    /// </summary>
    public class TextBoxButtonItemGenerator : ItemGenerator
    {
        public TextBoxButtonItemGenerator(CommandGroup group, CommandAutoUIContext context) : base(group, context) { }

        protected override ItemControlResult CreateItemControl()
        {
            var commandInfo = this.CommandItem;
            var cmd = CommandRepository.NewCommand(commandInfo);

            var textBox = CreateTextBox(cmd.CoreCommand);

            var btn = this.CreateAButton(cmd);

            this.PrepareControls(textBox, btn);

            //在textBox上按下回车时，执行命令。
            textBox.KeyDown += (o, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    TryExcuteCommand(btn);
                }
            };

            var panel = new DockPanel();
            panel.Children.Add(btn);
            panel.Children.Add(new Separator());
            panel.Children.Add(textBox);

            Border border = new Border();
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Colors.Silver);
            border.Child = panel;

            return new ItemControlResult(border, cmd);
        }

        protected virtual void PrepareControls(TextBox textBox, Button btn) { }
    }
}