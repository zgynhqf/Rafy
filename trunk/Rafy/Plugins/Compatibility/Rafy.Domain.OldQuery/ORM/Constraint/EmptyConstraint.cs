/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130528
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130528 17:27
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rafy.Data;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 一个无用的约束。是条件的默认值。
    /// </summary>
    internal class EmptyConstraint : Constraint
    {
        public override ConstraintType Type
        {
            get { return ConstraintType.Empty; }
        }

        //public override void GetSql(TextWriter sql, FormattedSqlParameters parameters)
        //{
        //    //do nothing;
        //}
    }
}