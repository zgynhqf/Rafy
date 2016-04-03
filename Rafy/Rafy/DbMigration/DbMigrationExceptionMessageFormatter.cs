/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130413 16:26
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130413 16:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Rafy.DbMigration
{
    class DbMigrationExceptionMessageFormatter
    {
        public static string FormatMessage(DbException ex, SqlMigrationRun sqlRun)
        {
            string errorMsg = string.Empty;

            if (ex is SqlException)
            {
                var sqlEx = ex as SqlException;
                var no = sqlEx.Number;
                switch (no)
                {
                    case 515:
                        errorMsg = "把一个可空字段变更为非空字段时，由于存在不可空约束，所以数据库该表中不能有该字段为空的数据行。\r\n";
                        break;
                    case 547:
                        errorMsg = "把一个字段变更为非空外键字段时，由于外键约束，所以数据库该表中不能有该字段数据不满足引用约束的数据行。\r\n";
                        break;
                    default:
                        break;
                }
            }

            errorMsg += ex.Message;

            var error = "执行数据库迁移时出错：" + errorMsg;

            if (sqlRun != null)
            {
                error += Environment.NewLine + "对应的 SQL：" + sqlRun.Sql;
            }

            return error;
        }
    }
}
