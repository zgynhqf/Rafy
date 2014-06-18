/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Linq.Expressions;
using Rafy.ManagedProperty;
using Rafy.Serialization;
using Rafy.Domain.Validation;
using System.Runtime.Serialization;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体类的一些不太重要的实现代码。
    /// </summary>
    public partial class Entity
    {
        /// <summary>
        /// 调试器显示文本。
        /// </summary>
        protected override string DebuggerDisplay
        {
            get
            {
                var name = base.DebuggerDisplay;

                if (this is IHasHame) { name += " IHasName:" + (this as IHasHame).Name; }
                if (this.SupportTree) { name += " TreeIndex:" + this.TreeIndex; }

                return name + " Id:" + this.Id;
            }
        }

        /// <summary>
        /// 实体所在的当前所在的列表对象。
        /// 
        /// 虽然一个实体可以存在于多个集合中，但是，它只保留一个主要集合的引用。
        /// <see cref="EntityList.SupressSetItemParent"/>
        /// </summary>
        public EntityList ParentList
        {
            get { return (this as IDomainComponent).Parent as EntityList; }
        }

        //获取Id属性太慢，去除以下属性。

        //public override int GetHashCode()
        //{
        //    return this.Id.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    var target = obj as T;

        //    if (target != null)
        //    {
        //        return this.Id.Equals(target.Id);
        //    }

        //    return base.Equals(obj);
        //}
    }
}
