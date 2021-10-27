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
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlClient;
using System.ServiceModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Rafy;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;

namespace Rafy.WPF
{
    public static class ExceptionExtension
    {
        /// <summary>
        /// 用于处理程序中所有的异常。
        /// 
        /// 如果当前是客户端，则会把这个异常弹出。
        /// </summary>
        /// <param name="ex"></param>
        /// <returns>如果在弹出框时，还发生了新的异常，则直接返回该异常。</returns>
        public static Exception Alert(this Exception ex)
        {
            Exception inner = null;

            if (RafyEnvironment.IsOnClient())
            {
                var app = Application.Current;
                if (app != null && app.Dispatcher != null && !app.CheckAccess())
                {
                    app.Dispatcher.Invoke(new Action(() =>
                    {
                        inner = SafeShowError(ex);
                    }));
                }
                else
                {
                    inner = SafeShowError(ex);
                }
            }

            return inner;
        }

        /// <summary>
        /// 以不出异常的方式显示异常信息。
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private static Exception SafeShowError(Exception ex)
        {
            Exception exception = null;

            try
            {
                //记录异常信息
                Logger.LogError("系统未捕获异常", ex);

                if (WPFConfigHelper.ShowErrorDetail.ToBoolean())
                {
                    ShowErrorDetail(ex);
                }
                else
                {
                    ShowKnownException(ex);
                }
            }
            catch (Exception innerException)
            {
                exception = innerException;
            }

            return exception;
        }

        /// <summary>
        /// 显示一些已知的异常。
        /// </summary>
        /// <param name="ex"></param>
        private static void ShowKnownException(Exception ex)
        {
            var baseException = ex.GetBaseException();

            string msg = null;

            if (baseException is SqlException)
            {
                var sqlEx = baseException as SqlException;
                var sqlErr = SqlErrorInfo.GetSqlError(sqlEx.Number);
                if (sqlErr != null) msg = sqlErr.ErrorMessage;
                else { msg = "数据库访问出现异常。"; }
            }
            else if (baseException is DbException)
            {
                msg = "数据库访问出现异常。";
            }
            else if (baseException is TimeoutException)
            {
                msg = "网络连接超时，请检查网络连接是否正常";
            }
            else if (baseException is Win32Exception || baseException is EndpointNotFoundException)
            {
                msg = "网络连接出现异常，请检查连接地址是否正确或连接是否正常";
            }

            msg = msg ?? @"遇到未知错误，我们对此引起的不便表示抱歉。系统已经产生了一个关于此错误的报告，希望您将问题反馈给我们以帮助改善质量。";

            App.MessageBox.Show(msg.Translate(), "异常提示".Translate(), MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        /// <summary>
        /// 以对话框的形式显示错误信息
        /// </summary>
        /// <param name="error"></param>
        private static void ShowErrorDetail(Exception error)
        {
            var baseException = error.GetBaseException();
            string msg = baseException.Message;

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
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            DockPanel.SetDock(msgBlock, Dock.Top);
            content.Children.Add(detailTxtBox);

            App.Windows.ShowDialog(content, w =>
            {
                w.Width = 600;
                w.Height = 400;
                w.Title = "异常提示（调试状态）".Translate();
            });
        }
    }
}