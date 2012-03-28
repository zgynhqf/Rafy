using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.WPF.Command
{
    public class SqlErrorInfo
    {
        public class SqlErrorContext
        {
            private int _errno;
            private string _errorDesc;
            public SqlErrorContext(int errno, string errorDesc)
            {
                this._errno = errno;
                this._errorDesc = errorDesc;
            }

            public int ErrorNo
            {
                get { return this._errno; }
            }

            public string ErrorMessage
            {
                get { return this._errorDesc; }
            }
        }

        private const int DB_ER_CANT_Update = 547; //数据被使用,无法进行此操作

        private static SqlErrorContext[] errorMap =
        {
            new SqlErrorContext(DB_ER_CANT_Update ,"数据被使用,无法进行此操作！")
        };

        public static string GetErrorMessage(int errno)
        {
            var errinfo = GetSqlError(errno);
            if (errinfo != null)
            {
                return errinfo.ErrorMessage;
            }
            return String.Empty;
        }

        public static SqlErrorContext GetSqlError(int errno)
        {
            return errorMap.FirstOrDefault(p => p.ErrorNo == errno);
        }
    }
}
