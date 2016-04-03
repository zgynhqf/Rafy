/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 10:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 一个数据源与一个具体表的连接结果，同时它也是一个新的数据源。
    /// </summary>
    class SqlJoin : SqlSource
    {
        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlJoin; }
        }

        /// <summary>
        /// 左边需要连接的数据源。
        /// </summary>
        public SqlSource Left { get; set; }

        /// <summary>
        /// 连接方式
        /// </summary>
        public SqlJoinType JoinType { get; set; }

        /// <summary>
        /// 右边需要连接的数据源。
        /// </summary>
        public SqlTable Right { get; set; }

        /// <summary>
        /// 连接所使用的约束条件。
        /// </summary>
        public SqlConstraint Condition { get; set; }
    }

    /// <summary>
    /// 支持的连接方式。
    /// </summary>
    enum SqlJoinType
    {
        Inner,
        LeftOuter,
        //Cross,
        //CrossApply,
        //OuterApply
    }
}
