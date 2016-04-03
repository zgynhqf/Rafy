/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120424
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120424
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration.Model;
using Rafy.Data;
using Rafy.Data.Providers;
using Rafy.DbMigration.SqlServer;

namespace Rafy.DbMigration.Oracle
{
    /// <summary>
    /// Oracle 的数据库迁移提供程序
    /// </summary>
    public class OracleMigrationProvider : DbMigrationProvider
    {
        public override IMetadataReader CreateSchemaReader()
        {
            return new OracleMetaReader(this.DbSetting);
        }

        public override RunGenerator CreateRunGenerator()
        {
            return new OracleRunGenerator();
        }

        public override IDbBackuper CreateDbBackuper()
        {
            throw new NotSupportedException("暂时不支持 Oracle 数据库的备份。");
        }

        /// <summary>
        /// Oracle 的标识符都不能超过 30 个字符。这个方法可以把传入的字符串剪裁到 30 个字符，并尽量保持信息。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string LimitOracleIdentifier(string identifier)
        {
            var toCut = identifier.Length - 30;

            if (toCut > 0)
            {
                ////保留 ID 字样
                //var newName = identifier.Replace("Id", "ID");

                ////从后面开始把多余的小写字母去除。
                //var list = newName.ToList();
                //for (int i = list.Count - 1; i >= 0 && toCut > 0; i--)
                //{
                //    var c = list[i];
                //    if (char.IsLower(c))
                //    {
                //        list.RemoveAt(i);
                //        toCut--;
                //    }
                //}
                ////如何还是太长，直接截取
                //for (int i = list.Count - 1; toCut > 0; i--)
                //{
                //    list.RemoveAt(i);
                //    toCut--;
                //}
                //newName = new string(list.ToArray());

                //return newName;

                //以上算法会导致一些缩写的名称重复。所以不如直接截取。这样，外层程序应该保证越在前面的字符，重要性越高。
                return identifier.Substring(0, 30);
            }

            return identifier;
        }

        /// <summary>
        /// 返回指定的表对应的序列的名称。
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="pkName"></param>
        /// <returns></returns>
        public static string SequenceName(string tableName, string pkName)
        {
            var name = string.Format("SEQ_{0}_{1}", PrepareIdentifier(tableName), PrepareIdentifier(pkName));
            name = LimitOracleIdentifier(name);
            return name;
        }

        /// <summary>
        /// 准备标识符名。
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public static string PrepareIdentifier(string identifier)
        {
            return identifier.ToUpper();
        }
    }
}