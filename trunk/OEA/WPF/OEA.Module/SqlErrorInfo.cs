using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.WPF.Command
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
    }

    public class SqlErrorContext
    {
        public SqlErrorContext(int errno, string errorDesc)
        {
            this.ErrorNo = errno;
            this.ErrorMessage = errorDesc;
        }

        public int ErrorNo { get; private set; }

        public string ErrorMessage { get; private set; }
    }
}