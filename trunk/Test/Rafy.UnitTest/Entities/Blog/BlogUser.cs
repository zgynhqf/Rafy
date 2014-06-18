/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 16:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    /// <summary>
    /// 博客用户
    /// </summary>
    [RootEntity, Serializable]
    public partial class BlogUser : UnitTestEntity
    {
        #region 构造函数

        public BlogUser() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected BlogUser(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> UserNameProperty = P<BlogUser>.Register(e => e.UserName);
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName
        {
            get { return this.GetProperty(UserNameProperty); }
            set { this.SetProperty(UserNameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 博客用户 列表类。
    /// </summary>
    [Serializable]
    public partial class BlogUserList : UnitTestEntityList { }

    /// <summary>
    /// 博客用户 仓库类。
    /// 负责 博客用户 类的查询、保存。
    /// </summary>
    public partial class BlogUserRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected BlogUserRepository() { }
    }

    /// <summary>
    /// 博客用户 配置类。
    /// 负责 博客用户 类的实体元数据的配置。
    /// </summary>
    internal class BlogUserConfig : UnitTestEntityConfig<BlogUser>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}