/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 13:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain.ORM.SqlTree;

namespace Rafy.Domain.ORM.Query.Impl
{
    class SelectAll : SqlSelectAll, ISelectAll
    {
        INamedSource ISelectAll.Source
        {
            get
            {
                return base.Table as INamedSource;
            }
            set
            {
                base.Table = value as SqlNamedSource;
            }
        }

        QueryNodeType IQueryNode.NodeType
        {
            get { return QueryNodeType.SelectAll; }
        }
    }
}
