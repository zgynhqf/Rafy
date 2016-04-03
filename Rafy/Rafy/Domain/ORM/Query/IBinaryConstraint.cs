/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 10:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query
{
    /// <summary>
    /// 二位操作符连接的节点
    /// </summary>
    public interface IBinaryConstraint : IConstraint
    {
        /// <summary>
        /// 二位运算的左操作结点。
        /// </summary>
        IConstraint Left { get; set; }

        /// <summary>
        /// 二位运算类型。
        /// </summary>
        BinaryOperator Opeartor { get; set; }

        /// <summary>
        /// 二位运算的右操作节点。
        /// </summary>
        IConstraint Right { get; set; }
    }
}