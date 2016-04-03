/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140703
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140703 18:58
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain
{
    /// <summary>
    /// 二位运算类型
    /// </summary>
    public enum BinaryOperator
    {
        /// <summary>
        /// 使用 And 连接。
        /// </summary>
        And = SqlBinaryConstraintType.And,
        /// <summary>
        /// 使用 Or 连接。
        /// </summary>
        Or = SqlBinaryConstraintType.Or
    }
}
