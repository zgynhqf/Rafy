/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 11:39
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示作用于两个操作结点的二位运算结点。
    /// </summary>
    class SqlBinaryConstraint : SqlConstraint
    {
        private ISqlConstraint _left,_right;

        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlBinaryConstraint; }
        }

        /// <summary>
        /// 二位运算的左操作节点。
        /// </summary>
        public ISqlConstraint Left
        {
            get { return _left; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _left = value;
            }
        }

        /// <summary>
        /// 二位运算类型
        /// </summary>
        public SqlBinaryConstraintType Opeartor { get; set; }

        /// <summary>
        /// 二位运算的右操作节点。
        /// </summary>
        public ISqlConstraint Right
        {
            get { return _right; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _right = value;
            }
        }
    }

    /// <summary>
    /// 二位运算类型
    /// </summary>
    enum SqlBinaryConstraintType
    {
        And,
        Or
    }
}