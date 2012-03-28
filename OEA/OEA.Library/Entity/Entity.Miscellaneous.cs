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
using SimpleCsla.Validation;
using System.Linq.Expressions;
using OEA.ManagedProperty;
using OEA.Serialization;

namespace OEA.Library
{
    /// <summary>
    /// 实体类的一些不太重要的实现代码。
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public partial class Entity
    {
        #region 属性规则

        public virtual void CheckRules()
        {
            this.ValidationRules.CheckRules();
        }

        #endregion

        #region 自动汇总数据

        protected void AutoCollectAsChanged(IManagedPropertyChangedEventArgs e, bool toTreeParent = true, IManagedProperty toParentProperty = null)
        {
            if (toTreeParent)
            {
                var parentTree = this.TreeParent;
                if (parentTree != null)
                {
                    parentTree.CalculateCollectValue(e.Property, e);
                    //如果已经向树型父汇总，则不向父对象汇总，直接返回
                    return;
                }
            }

            if (toParentProperty != null)
            {
                var parent = this.GetParentEntity();
                if (parent != null)
                {
                    parent.CalculateCollectValue(toParentProperty, e);
                }
            }
        }

        private void CalculateCollectValue(IManagedProperty property, IManagedPropertyChangedEventArgs args)
        {
            var distance = Convert.ToDouble(args.NewValue) - Convert.ToDouble(args.OldValue);
            var oldValue = Convert.ToDouble(this.GetProperty(property));
            this.SetProperty(property, oldValue + distance);
        }

        #endregion

        /// <summary>
        /// 通知本复制源，马上要对本对象进行复制操作了。
        /// </summary>
        public virtual void BeforeCopy() { }

        /// <summary>
        /// 通知本复制源，本对象复制操作完毕。
        /// </summary>
        public virtual void AfterCopy() { }

        protected virtual string DebuggerDisplay
        {
            get
            {
                var name = string.Empty;
                if (this.SupportTree) { name += this.TreeCode + " Name:"; }

                //尝试读取Name属性。
                var nameProperty = AllProperties().FirstOrDefault(p => p.Name == "Name");
                if (nameProperty != null)
                {
                    var value = this.GetProperty(nameProperty);
                    if (value != null)
                    {
                        return name + value;
                    }
                }

                return name + this.GetType().Name + " " + this.Id;
            }
        }

        public override string ToString()
        {
            //有时候实体的显示不走 DebuggerDisplay 属性（例如 GridTreeViewRow 中的 Header），
            //所以这里重写 ToString 方法，方便调试。
            return this.DebuggerDisplay;
        }

        public EntityList ParentList
        {
            get { return (this as IEntityOrList).Parent as EntityList; }
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
