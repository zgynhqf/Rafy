/*******************************************************
 * 
 * 作者：杜强
 * 创建时间：201011
 * 说明：提供公共的类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 杜强 201011
 * 
*******************************************************/

using System.Windows;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 使用用WPF扩展的MessageBox
    /// </summary>
    public class OEAMessageBox
    {
        public static MessageBoxResult Show(string messageText)
        {
            return Microsoft.Windows.Controls.MessageBox.Show(messageText);
        }

        public static MessageBoxResult Show(string messageText, string caption)
        {
            return Microsoft.Windows.Controls.MessageBox.Show(messageText, caption);
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button)
        {
            return Microsoft.Windows.Controls.MessageBox.Show(messageText, caption, button);
        }

        public static MessageBoxResult Show(string messageText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Microsoft.Windows.Controls.MessageBox.Show(messageText, caption, button, icon);
        }
    }
    //public static class OEATaskDialog
    //{
    //    public static TaskDialogResult Show(string header, string title, TaskDialogButtons standardButtons)
    //    {
    //        return Show(header, title, string.Empty, standardButtons);
    //    }

    //    public static TaskDialogResult Show(string header, string title, string content, TaskDialogButtons standardButtons)
    //    {
    //        return TaskDialog.Show(header, title, content, standardButtons, TaskDialogIcon.None, false,
    //            new TaskDialogButtonData(TaskDialogButtons.Yes)
    //            {
    //                Content = "是"
    //            },
    //            new TaskDialogButtonData(TaskDialogButtons.No)
    //            {
    //                Content = "否"
    //            });
    //    }
    //}
}
