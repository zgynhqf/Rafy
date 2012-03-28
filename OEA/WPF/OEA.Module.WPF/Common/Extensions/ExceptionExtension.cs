using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OEA.Module.WPF.Controls;

using System.Data.SqlClient;
using OEA.WPF.Command;

namespace OEA.Module.WPF
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// 以对话框的形式显示错误信息
        /// </summary>
        /// <param name="error"></param>
        public static void ShowError(this Exception error)
        {
            var content = new DockPanel();

            var label = new TextBlock
            {
                Text = error.GetBaseException().Message,
                Margin = new Thickness(8)
            };
            DockPanel.SetDock(label, Dock.Top);
            content.Children.Add(label);

            var label2 = new TextBox
            {
                AcceptsReturn = true,
                Text = error.ToString(),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            DockPanel.SetDock(label, Dock.Top);
            content.Children.Add(label2);

            App.Current.Windows.ShowDialog(content, w =>
            {
                w.Width = 600;
                w.Height = 400;
                w.Title = "错误";
            });
        }

        /// <summary>
        /// 用以管理程序的异常信息
        /// </summary>
        /// <param name="ex"></param>
        public static void ManageException(this Exception ex)
        {
            string msg = @"遇到未知错误，我们对此引起的不便表示抱歉。系统已经产生了一个关于此错误的报告，希望您将问题反馈给我们以帮助改善质量。";

            var baseException = ex.GetBaseException();
            if (baseException is FriendlyMessageException)
            {
                msg = baseException.Message;
            }
            else if (baseException is SqlException)
            {
                var sqlex = baseException as SqlException;
                var sqlerr = SqlErrorInfo.GetSqlError(sqlex.Number);
                if (sqlerr != null) msg = sqlerr.ErrorMessage;
            }
            else if (ex is System.Data.Common.DbException)
                msg = "数据库访问出现异常";
            else if (ex is TimeoutException)
                msg = "网络连接超时，请检查网络连接是否正常";
            else if (ex is FriendlyMessageException)
                msg = ex.Message;
            else if (ex is System.ComponentModel.Win32Exception)
                msg = "网络连接出现异常，请检查连接地址是否正确或连接是否正常";
            else if (ex is System.ServiceModel.EndpointNotFoundException)
                msg = "网络连接出现异常，请检查连接地址是否正确";

            App.Current.MessageBox.Show("异常提示", msg, MessageBoxButton.OK, MessageBoxImage.Exclamation);

            //记录异常信息
            Logger.LogError("系统未捕获异常", ex);
        }
    }
}