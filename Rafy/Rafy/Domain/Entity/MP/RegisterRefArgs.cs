/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121120 20:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121120 20:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using System.ComponentModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 声明引用实体属性的参数对象。
    /// </summary>
    public class RegisterRefArgs : PropertyMetadata<Entity>
    {
        public RegisterRefArgs()
        {
            //引用实体属性的变更，不会引起实体状态的变更。
            this.AffectStatus = false;
        }

        #region Args

        /// <summary>
        /// 实体引用的类型
        /// </summary>
        public ReferenceType ReferenceType { get; set; } = ReferenceType.Normal;

        /// <summary>
        /// 对应的引用 Id 属性
        /// </summary>
        public IManagedProperty RefKeyProperty { get; set; }

        /// <summary>
        /// 引用实体的键对应的托管属性。
        /// </summary>
        public IManagedProperty KeyPropertyOfRefEntity { get; set; }

        /// <summary>
        /// 实例加载器（使用外键拥有者作为加载上下文）
        /// </summary>
        public RefEntityLoader Loader { get; set; }

        #endregion
    }
}