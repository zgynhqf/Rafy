/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150821
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150821 14:13
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.ORM.SqlTree
{
    /// <summary>
    /// 表示一组结点对象
    /// 
    /// SqlNodeList 需要从 SqlConstraint 上继承，否则将不可用于 Where 语句。
    /// </summary>
    class SqlNodeList : SqlConstraint, IEnumerable, ISqlLiteral
    {
        private List<ISqlNode> _items = new List<ISqlNode>();

        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlNodeList; }
        }

        public void Add(ISqlNode item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// 所有节点。
        /// </summary>
        public List<ISqlNode> Items
        {
            get { return _items; }
        }

        public IEnumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}