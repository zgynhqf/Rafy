/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150821
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150821 14:23
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
    /// 表示一组排序条件。
    /// </summary>
    class SqlOrderByList : SqlNode, IEnumerable
    {
        private IList _items;

        public override SqlNodeType NodeType
        {
            get { return SqlNodeType.SqlOrderByList; }
        }

        public int Count
        {
            get
            {
                if (_items == null) { return 0; }

                return _items.Count;
            }
        }

        public IList Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<SqlOrderBy>();
                }
                return _items;
            }
            set { _items = value; }
        }

        public void Add(object item)
        {
            this.Items.Add(item);
        }

        public IEnumerator GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
    }
}
