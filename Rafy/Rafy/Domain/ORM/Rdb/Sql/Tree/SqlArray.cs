/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131210
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131210 17:41
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示一组树结点合成的一个集合结点。
    /// 这些结点之间，需要用逗号分隔开。
    /// </summary>
    class SqlArray : SqlNode
    {
        internal SqlArray() : this(true) { }

        internal SqlArray(bool initItems)
        {
            if (initItems)
            {
                this.Items = new List<SqlNode>();
            }
        }

        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlArray; }
        }

        /// <summary>
        /// 所有项。
        /// 其中每一个项必须是一个 SqlNode。
        /// </summary>
        public IList Items { get; set; }
    }
}