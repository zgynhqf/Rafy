/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110328
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace OEA
{
    public class RelationViewCollection : Collection<RelationView>
    {
        private ObjectView _owner;

        internal RelationViewCollection(ObjectView owner)
        {
            if (owner == null) throw new ArgumentNullException("owner");

            this._owner = owner;
        }

        public ObjectView Owner
        {
            get { return this._owner; }
        }

        protected override void InsertItem(int index, RelationView item)
        {
            base.InsertItem(index, item);

            item.Owner = this._owner;
        }
    }
}