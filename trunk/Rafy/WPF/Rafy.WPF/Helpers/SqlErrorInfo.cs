/*******************************************************
 * 
 * 作者：杜强
 * 创建日期：2010
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 杜强 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Rafy.WPF.Command;

namespace Rafy.WPF
{
    public class SqlErrorInfo
    {
        private static SqlErrorContext[] errorMap =
        {
            new SqlErrorContext(547 ,"数据被使用,无法进行此操作！")
        };

        public static SqlErrorContext GetSqlError(int errno)
        {
            return errorMap.FirstOrDefault(p => p.ErrorNo == errno);
        }

        /// <summary>
        /// 附加在命令发生 SqlException 异常时，直接提示用户的行为。
        /// </summary>
        public static void AttachAlertCommandSqlErrorBehavior()
        {
            CommandRepository.CommandCreated += (o, e) =>
            {
                e.Instance.ExecuteFailed += (oo, ee) =>
                {
                    if (!ee.Cancel)
                    {
                        //如果出现了已知的 SqlException，直接弹出 SqlException 的提示。
                        var sqlex = ee.Exception.GetBaseException() as SqlException;
                        if (sqlex != null)
                        {
                            var sqlerr = SqlErrorInfo.GetSqlError(sqlex.Number);
                            if (sqlerr == null) return;

                            App.MessageBox.Show(sqlerr.ErrorMessage.Translate());
                            ee.Cancel = true;
                        }
                    }
                };
            };
        }
    }

    public class SqlErrorContext
    {
        public SqlErrorContext(int errno, string errorMessage)
        {
            this.ErrorNo = errno;
            this.ErrorMessage = errorMessage;
        }

        public int ErrorNo { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}