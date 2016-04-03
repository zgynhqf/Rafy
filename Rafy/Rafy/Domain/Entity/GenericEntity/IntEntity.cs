/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140506
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140506 20:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 以 Int 作为主键的实体基类。
    /// </summary>
    public abstract class IntEntity : Entity<int>
    {
        #region 构造函数

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected IntEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected IntEntity()
        {
            //不需要此行，所有新增的实体的 Id 都是 -1.
            //this.LoadProperty(IdProperty, RafyEnvironment.NewLocalId());
        }

        #endregion

        /// <summary>
        /// 树型父实体的 Id 属性
        /// 
        /// 默认使用存储于数据库中的字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        public virtual new int? TreePId
        {
            get { return (int?)this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }

        ///// <summary>
        ///// 获取指定引用 id 属性对应的 id 的可空类型返回值。
        ///// </summary>
        ///// <param name="property"></param>
        ///// <returns></returns>
        //public new int? GetRefNullableId(IRefIdProperty property)
        //{
        //    var value = this.GetRefId(property);
        //    return KeyProvider.HasId(value) ? value : default(int?);
        //}

        ///// <summary>
        ///// 设置指定引用 id 属性对应的 id 的可空类型值。
        ///// </summary>
        ///// <param name="property"></param>
        ///// <param name="value"></param>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public int? SetRefNullableId(IRefIdProperty property, int? value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        //{
        //    var finalValue = this.SetRefId(property, value.GetValueOrDefault(), source);
        //    return KeyProvider.HasId(finalValue) ? finalValue : default(int?);
        //}

        ///// <summary>
        ///// 获取指定引用 id 属性对应的 id 的返回值。
        ///// </summary>
        ///// <param name="property"></param>
        ///// <returns></returns>
        //public new int GetRefId(IRefIdProperty property)
        //{
        //    return (int)this.GetProperty(property);
        //}

        ///// <summary>
        ///// 设置指定引用 id 属性对应的 id 的值。
        ///// 
        ///// 在引用 id 变化时，会同步相应的引用实体属性。
        ///// </summary>
        ///// <param name="property"></param>
        ///// <param name="value"></param>
        ///// <param name="source"></param>
        ///// <returns></returns>
        //public int SetRefId(IRefIdProperty property, int value, ManagedPropertyChangedSource source = ManagedPropertyChangedSource.FromProperty)
        //{
        //    return (int)base.SetRefId(property, value, source);
        //}
    }
}