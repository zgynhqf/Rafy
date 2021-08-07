/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110104
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Data;
using Rafy.Reflection;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Data.Common;

namespace Rafy.DbMigration.SqlServerCe
{
    [DebuggerDisplay("CREATE DATABASE : {Database}")]
    public class CreateDbMigrationRun : MigrationRun
    {
        protected override void RunCore(IDbAccesser db)
        {
            bool isSqlceCon = IsSqlceConnection(db);
            if (!isSqlceCon) throw new InvalidOperationException("需要使用 SqlCe 的连接，才能正确创建数据库。");

            //保存目录存在。
            var type = Type.GetType("System.Data.SqlServerCe.SqlCeConnectionStringBuilder, System.Data.SqlServerCe");
            var csb = Activator.CreateInstance(type, db.Connection.ConnectionString);
            var path = CommonUtils.ReplaceDataDirectory(ObjectHelper.GetPropertyValue(csb, "DataSource") as string);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
            else
            {
                Directory.CreateDirectory("\\");
            }

            //调用引擎创建数据库
            var engineType = Type.GetType("System.Data.SqlServerCe.SqlCeEngine, System.Data.SqlServerCe");
            var engine = Activator.CreateInstance(engineType, db.Connection.ConnectionString);
            engineType.GetMethod("CreateDatabase").Invoke(engine, null);

            //var engine = new System.Data.SqlServerCe.SqlCeEngine(db.Connection.ConnectionString);
            //engine.CreateDatabase();
        }

        internal static bool IsSqlceConnection(IDbAccesser db)
        {
            return db.Connection.GetType().AssemblyQualifiedName.Contains(DbSetting.Provider_SqlCe);
        }

        /// <summary>
        /// 本类完全 Copy 自 System.Data.SqlServerCe 程序集
        /// </summary>
        internal static class CommonUtils
        {
            private const string DataDirectoryMacro = "|DataDirectory|";
            private const string DataDirectory = "DataDirectory";
            public static string ReplaceDataDirectory(string inputString)
            {
                string result = inputString.Trim();
                if (!string.IsNullOrEmpty(inputString) && inputString.StartsWith(DataDirectoryMacro, StringComparison.InvariantCultureIgnoreCase))
                {
                    string text = AppDomain.CurrentDomain.GetData(DataDirectory) as string;
                    if (string.IsNullOrEmpty(text))
                    {
                        text = (AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory);
                    }
                    if (string.IsNullOrEmpty(text))
                    {
                        text = string.Empty;
                    }
                    int num = DataDirectoryMacro.Length;
                    if (inputString.Length > DataDirectoryMacro.Length && '\\' == inputString[DataDirectoryMacro.Length])
                    {
                        num++;
                    }
                    result = Path.Combine(text, inputString.Substring(num));
                }
                return result;
            }
        }
    }
}
