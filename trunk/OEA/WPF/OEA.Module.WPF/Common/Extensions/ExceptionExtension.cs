/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120120
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120120
 * 
*******************************************************/

using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;

namespace OEA.Module.WPF
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// 用以管理程序的异常信息
        /// </summary>
        /// <param name="ex"></param>
        public static void Alert(this Exception ex)
        {
            var baseException = ex.GetBaseException();

            if (OEAEnvironment.IsDebuggingEnabled)
            {
                ShowErrorDetail(baseException.Message, ex);
            }
            else
            {
                string msg = null;

                if (baseException is FriendlyMessageException)
                {
                    msg = baseException.Message;
                }
                else if (baseException is SqlException)
                {
                    var sqlEx = baseException as SqlException;
                    var sqlErr = SqlErrorInfo.GetSqlError(sqlEx.Number);
                    if (sqlErr != null) msg = sqlErr.ErrorMessage;
                    else { msg = "数据库访问出现异常。"; }
                }
                else if (ex is System.Data.Common.DbException)
                    msg = "数据库访问出现异常。";
                else if (ex is TimeoutException)
                    msg = "网络连接超时，请检查网络连接是否正常";
                else if (ex is FriendlyMessageException)
                    msg = ex.Message;
                else if (ex is System.ComponentModel.Win32Exception || ex is System.ServiceModel.EndpointNotFoundException)
                    msg = "网络连接出现异常，请检查连接地址是否正确或连接是否正常";
                msg = msg ?? @"遇到未知错误，我们对此引起的不便表示抱歉。系统已经产生了一个关于此错误的报告，希望您将问题反馈给我们以帮助改善质量。";

                App.MessageBox.Show(msg, "异常提示", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            //记录异常信息
            Logger.LogError("系统未捕获异常", ex);
        }

        /// <summary>
        /// 以对话框的形式显示错误信息
        /// </summary>
        /// <param name="error"></param>
        private static void ShowErrorDetail(string msg, Exception error)
        {
            var content = new DockPanel();

            var msgBlock = new TextBlock
            {
                Text = msg,
                Margin = new Thickness(8)
            };
            DockPanel.SetDock(msgBlock, Dock.Top);
            content.Children.Add(msgBlock);

            var detailTxtBox = new TextBox
            {
                AcceptsReturn = true,
                Text = error.ToString(),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            DockPanel.SetDock(msgBlock, Dock.Top);
            content.Children.Add(detailTxtBox);

            App.Windows.ShowDialog(content, w =>
            {
                w.Width = 600;
                w.Height = 400;
                w.Title = "异常提示（调试状态）";
            });
        }
    }
}