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
    /// 以 string 作为主键的实体基类。
    /// </summary>
    public abstract class StringEntity : Entity<string>
    {
        #region 构造函数

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected StringEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected StringEntity() { }

        #endregion

        //static StringEntity()
        //{
        //    //注册自关联属性，并设置它的默认为 null。
        //    TreePIdProperty = P<StringEntity>.Register(e => e.TreePId, (string)null);
        //    TreePIdProperty.OverrideMeta(typeof(StringEntity), new ManagedPropertyMetadata<string>
        //    {
        //        DefaultValue = null
        //    });
        //}

        /// <summary>
        /// 树型父实体的 Id 属性
        /// 
        /// 默认使用存储于数据库中的字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        public virtual new string TreePId
        {
            get { return (string)this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }
    }
}