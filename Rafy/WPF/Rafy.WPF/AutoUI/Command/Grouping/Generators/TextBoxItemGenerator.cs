/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121108 12:37
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121108 12:37
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Rafy.WPF.Command;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 文本框命令生成器。
    /// </summary>
    public class TextBoxItemGenerator : ItemGenerator
    {
        public TextBoxItemGenerator()
        {
            this.TriggerMode = TextBoxCommandTriggerMode.EnterPressed;
        }

        /// <summary>
        /// 文本框触发命令的模式。
        /// </summary>
        public TextBoxCommandTriggerMode TriggerMode { get; set; }

        protected override FrameworkElement CreateCommandUI(ClientCommand cmd)
        {
            var textBox = CreateTextBox(cmd);

            if (this.TriggerMode == TextBoxCommandTriggerMode.EnterPressed)
            {
                //在textBox上按下回车时，执行命令。
                textBox.KeyDown += (o, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        cmd.TryExecute(textBox);
                    }
                };
            }
            else
            {
                textBox.TextChanged += (o, e) =>
                {
                    cmd.TryExecute(textBox);
                };
            }

            return textBox;
        }

        /// <summary>
        /// 文本框触发命令的模式。
        /// </summary>
        public enum TextBoxCommandTriggerMode
        {
            /// <summary>
            /// 文本变更时触发
            /// </summary>
            TextChanged,
            /// <summary>
            /// 回车后触发
            /// </summary>
            EnterPressed
        }
    }
}