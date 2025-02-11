﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120629 11:02
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120629 11:02
 * 
*******************************************************/

using System;
using System.Data;
using System.Collections.Generic;
using Rafy.Data;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 直接使用 SQL 编写的数据库条件表达式。
    /// </summary>
    internal class SqlWhereConstraint : Constraint
    {
        public override ConstraintType Type
        {
            get { return ConstraintType.Sql; }
        }

        /// <summary>
        /// 参数化 sql 语句
        /// </summary>
        public string FormatSql { get; set; }

        /// <summary>
        /// 对应的参数值列表
        /// </summary>
        public object[] Parameters { get; set; }

        //public override void GetSql(TextWriter sql, FormattedSqlParameters parameters)
        //{
        //    var formatSql = this.FormatSql;

        //    if (this.Parameters != null && this.Parameters.Length > 0)
        //    {
        //        formatSql = Regex.Replace(formatSql, @"\{(?<index>\d+)\}", m =>
        //        {
        //            var index = Convert.ToInt32(m.Groups["index"].Value);
        //            var value = this.Parameters[index];
        //            index = parameters.Add(value);
        //            return "{" + index + "}";
        //        });
        //    }

        //    sql.Write(formatSql);
        //}
    }
}