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
using System.Windows.Input;
using System.Windows.Controls;
using Rafy.WPF.Command;

namespace Rafy.WPF.Command.UI
{
    internal static class CommandToolTipService
    {
        static CommandToolTipService()
        {
            ToolTipKeyGestureFormat = "{0} ({1})";
        }

        public static string ToolTipKeyGestureFormat { get; set; }

        public static void SetupTooltip(FrameworkElement item, ClientCommand command, CommandToolTipStyle toolTipStyle)
        {
            // tooltip
            if (item.ToolTip == null && command != null)
            {
                item.ToolTip = GetCommandToolTip(command, toolTipStyle);
            }
            if (item.ToolTip is string && command != null)
            {
                item.ToolTip = FormatToolTip(command, item.ToolTip as string, toolTipStyle);
            }

            SetShowOnDisabled(item, toolTipStyle);
        }

        private static object GetCommandToolTip(ClientCommand command, CommandToolTipStyle toolTipStyle)
        {
            if (command == null) { throw new ArgumentNullException("command"); }

            if (toolTipStyle == CommandToolTipStyle.None) { return null; }

            return command.Meta.ToolTip.Translate();
        }

        private static object FormatToolTip(ClientCommand command, string toolTip, CommandToolTipStyle toolTipStyle)
        {
            if (command == null) { throw new ArgumentNullException("command"); }

            if (toolTipStyle == CommandToolTipStyle.None || string.IsNullOrEmpty(toolTip)) { return null; }

            if (toolTipStyle != CommandToolTipStyle.AlwaysWithKeyGesture &&
                    toolTipStyle != CommandToolTipStyle.EnabledWithKeyGesture) { return toolTip; }

            if (command.UICommand.InputGestures.Count > 0)
            {
                KeyGesture keyGesture = command.UICommand.InputGestures[0] as KeyGesture;
                if (keyGesture != null && keyGesture.DisplayString.Length > 0)
                {
                    toolTip = string.Format(ToolTipKeyGestureFormat, toolTip, keyGesture.DisplayString);
                }
            }

            return toolTip;
        }

        private static void SetShowOnDisabled(DependencyObject dependencyObject, CommandToolTipStyle toolTipStyle)
        {
            if (toolTipStyle == CommandToolTipStyle.Always || toolTipStyle == CommandToolTipStyle.AlwaysWithKeyGesture)
            {
                ToolTipService.SetShowOnDisabled(dependencyObject, true);
            }
        }
    }
}