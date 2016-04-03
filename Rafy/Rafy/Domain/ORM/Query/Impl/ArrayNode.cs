/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 11:41
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class ArrayNode : SqlArray, IArray
    {
        public ArrayNode() : base(false) { }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.Array; }
        }

        IList<IQueryNode> IArray.Items
        {
            get
            {
                return base.Items as IList<IQueryNode>;
            }
            set
            {
                base.Items = value as IList;
            }
        }
    }

    class AutoSelectionColumns : ArrayNode { }
}
