/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 12:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Rafy.Accounts
{
    /// <summary>
    /// 系统中的用户
    /// </summary>
    [RootEntity, Serializable]
    public partial class User : AccountsEntity, IRafyIdentity
    {
        #region 构造函数

        public User() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected User(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> UserNameProperty = P<User>.Register(e => e.UserName);
        /// <summary>
        /// 用户登录名
        /// </summary>
        public string UserName
        {
            get { return this.GetProperty(UserNameProperty); }
            set { this.SetProperty(UserNameProperty, value); }
        }

        public static readonly Property<string> RealNameProperty = P<User>.Register(e => e.RealName);
        /// <summary>
        /// 真实姓名。
        /// </summary>
        public string RealName
        {
            get { return this.GetProperty(RealNameProperty); }
            set { this.SetProperty(RealNameProperty, value); }
        }

        public static readonly Property<string> PasswordProperty = P<User>.Register(e => e.Password);
        /// <summary>
        /// 加密后的密码
        /// </summary>
        public string Password
        {
            get { return this.GetProperty(PasswordProperty); }
            set { this.SetProperty(PasswordProperty, value); }
        }

        public static readonly Property<DateTime> LastLoginTimeProperty = P<User>.Register(e => e.LastLoginTime);
        /// <summary>
        /// 最后一次登录的时间。
        /// </summary>
        public DateTime LastLoginTime
        {
            get { return this.GetProperty(LastLoginTimeProperty); }
            set { this.SetProperty(LastLoginTimeProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion

        #region IRafyIdentity 接口实现

        string IIdentity.AuthenticationType
        {
            get { return "Rafy Authentication."; }
        }

        bool IIdentity.IsAuthenticated
        {
            get { return true; }
        }

        string IIdentity.Name
        {
            get { return this.UserName; }
        }

        #endregion
    }

    /// <summary>
    /// 系统中的用户 列表类。
    /// </summary>
    [Serializable]
    public partial class UserList : AccountsEntityList { }

    /// <summary>
    /// 系统中的用户 仓库类。
    /// 负责 系统中的用户 类的查询、保存。
    /// </summary>
    public partial class UserRepository : AccountsEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected UserRepository() { }

        /// <summary>
        /// 通过用户名精确匹配指定的某个用户。
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public User GetByUserName(string userName)
        {
            return this.FetchFirst(new CommonQueryCriteria
            {
                new PropertyMatch(User.UserNameProperty, userName)
            });
        }
    }

    /// <summary>
    /// 系统中的用户 配置类。
    /// 负责 系统中的用户 类的实体元数据的配置。
    /// </summary>
    internal class UserConfig : AccountsEntityConfig<User>
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