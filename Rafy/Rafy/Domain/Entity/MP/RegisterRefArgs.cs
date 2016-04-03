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
        #region Args

        /// <summary>
        /// 对应的引用 Id 属性
        /// </summary>
        public IRefIdProperty RefIdProperty { get; set; }

        /// <summary>
        /// 是否需要序列化引用实体
        /// 
        /// 如果不设置任何值，默认情况下：引用实体对象只会从服务端序列化到客户端。从客户端传输到服务端时，不会序列化。
        /// （
        /// 这是因为服务端可以简单地查询出所有的数据，而不需要客户端传输过来。
        /// 客户端如何需要把多个实体一同传输到服务端时，应该使用服务把多个实体同时设置为服务的输入。
        /// ）
        /// </summary>
        public new bool? Serializable { get; set; }

        /// <summary>
        /// 实例加载器（使用外键拥有者作为加载上下文）
        /// </summary>
        public RefEntityLoader Loader { get; set; }

        #endregion
    }
}