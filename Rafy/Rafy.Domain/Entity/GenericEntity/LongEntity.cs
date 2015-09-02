/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150829
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150829 18:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 以 Int64 作为主键的实体基类。
    /// </summary>
    public abstract class LongEntity : Entity<long>
    {
        #region 构造函数

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected LongEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        protected LongEntity()
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
        public virtual new long? TreePId
        {
            get { return (long?)this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }
    }
}
