/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110414
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110414
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy;
using Rafy.Domain;
using System.Diagnostics;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Threading;

namespace Rafy.RBAC
{
    /// <summary>
    /// 用户登录日志
    /// </summary>
    [Serializable]
    [RootEntity]
    public partial class UserLoginLog : IntEntity
    {
        #region 构造函数

        public UserLoginLog() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected UserLoginLog(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty UserIdProperty =
            P<UserLoginLog>.RegisterRefId(e => e.UserId, ReferenceType.Normal);
        public int UserId
        {
            get { return (int)this.GetRefId(UserIdProperty); }
            set { this.SetRefId(UserIdProperty, value); }
        }
        public static readonly RefEntityProperty<User> UserProperty =
            P<UserLoginLog>.RegisterRef(e => e.User, UserIdProperty);
        public User User
        {
            get { return this.GetRefEntity(UserProperty); }
            set { this.SetRefEntity(UserProperty, value); }
        }

        public static readonly Property<string> UserNameProperty = P<UserLoginLog>.RegisterReadOnly(e => e.UserName, e => (e as UserLoginLog).GetUserName());
        public string UserName
        {
            get { return this.GetProperty(UserNameProperty); }
        }
        private string GetUserName()
        {
            return this.User.Name;
        }

        /// <summary>
        /// 是否表示登入操作。（true：登录；false：登出。）
        /// </summary>
        public static readonly Property<bool> IsInProperty = P<UserLoginLog>.Register(e => e.IsIn);
        public bool IsIn
        {
            get { return this.GetProperty(IsInProperty); }
            set { this.SetProperty(IsInProperty, value); }
        }

        public static readonly Property<string> IsInTextProperty = P<UserLoginLog>.RegisterReadOnly(e => e.IsInText, e => (e as UserLoginLog).GetIsInText());
        public string IsInText
        {
            get { return this.GetProperty(IsInTextProperty); }
        }
        private string GetIsInText()
        {
            return this.IsIn ? "登录" : "退出";
        }

        /// <summary>
        /// 记录时间
        /// </summary>
        public static readonly Property<DateTime> LogTimeProperty = P<UserLoginLog>.Register(e => e.LogTime);
        public DateTime LogTime
        {
            get { return this.GetProperty(LogTimeProperty); }
            set { this.SetProperty(LogTimeProperty, value); }
        }
    }

    [Serializable]
    public partial class UserLoginLogList : EntityList { }

    public partial class UserLoginLogRepository : EntityRepository
    {
        protected UserLoginLogRepository() { }
    }

    internal class UserLoginLogConfig : EntityConfig<UserLoginLog>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            this.Meta.MapTable().MapProperties(
                UserLoginLog.UserIdProperty,
                UserLoginLog.IsInProperty,
                UserLoginLog.LogTimeProperty
                );
        }
    }

    /// <summary>
    /// 为UserLoginLog提供了一些便利的服务方法。
    /// </summary>
    internal static class UserLoginLogService
    {
        /// <summary>
        /// 登录之后的用户。
        /// 
        /// 由于当前系统只让一个用户登录。
        /// 登录成功后，使用这个静态字段保存它。
        /// </summary>
        private static User _user;

        /// <summary>
        /// 外部系统使用本服务来记录登录操作
        /// </summary>
        /// <param name="user"></param>
        internal static void NotifyLogin(User user)
        {
            Debug.Assert(user != null, "user != null");

            _user = user;

            Log(user, true);
        }

        /// <summary>
        /// 外部系统使用本服务来记录出操作
        /// </summary>
        internal static void NotifyLogout()
        {
            if (_user != null) { Log(_user, false); }
        }

        /// <summary>
        /// 记录操作
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isIn">
        /// 登入还是登出
        /// </param>
        private static void Log(User user, bool isIn)
        {
            //AsyncHelper.InvokeSafe(() =>
            //{
            RF.Save(new UserLoginLog()
            {
                User = user,
                IsIn = isIn,
                LogTime = DateTime.Now
            });
            //});
        }
    }
}