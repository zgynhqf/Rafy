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
    //实体类的一些不太重要的实现代码。
    public partial class Entity
    {
        /// <summary>
        /// 调试器显示文本。
        /// </summary>
        internal new string DebuggerDisplay
        {
            get
            {
                var name = base.DebuggerDisplay;

                if (this is IHasHame) { name += " IHasName:" + (this as IHasHame).Name; }
                if (this.SupportTree) { name += " TreeIndex:" + this.TreeIndex; }

                return name + " Id:" + this.Id;
            }
        }

        internal void NotifyRevalidate(IProperty property)
        {
            //目前直接用属性变更事件来通知上层的 Binding 重新
            this.OnPropertyChanged(property.Name);
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
