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
using System.Linq;
using System.Runtime.Serialization;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy;
using System.Security.Permissions;
using Rafy.Domain.Validation;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 用户
    /// </summary>
    [RootEntity, Serializable]
    public partial class User : IntEntity
    {
        public static readonly Property<string> CodeProperty = P<User>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<User>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> PasswordProperty = P<User>.Register(e => e.Password);
        public string Password
        {
            get { return this.GetProperty(PasswordProperty); }
            set { this.SetProperty(PasswordProperty, value); }
        }

        public static readonly Property<DateTime?> LastLoginTimeProperty = P<User>.Register(e => e.LastLoginTime);
        public DateTime? LastLoginTime
        {
            get { return this.GetProperty(LastLoginTimeProperty); }
            set { this.SetProperty(LastLoginTimeProperty, value); }
        }

        public static readonly Property<int> LoginCountProperty = P<User>.Register(e => e.LoginCount, int.MaxValue);
        public int LoginCount
        {
            get { return this.GetProperty(LoginCountProperty); }
            set { this.SetProperty(LoginCountProperty, value); }
        }

        public static readonly Property<int> MaxLoginCountProperty = P<User>.Register(e => e.MaxLoginCount);
        public int MaxLoginCount
        {
            get { return this.GetProperty(MaxLoginCountProperty); }
            set { this.SetProperty(MaxLoginCountProperty, value); }
        }
    }

    [Serializable]
    public partial class UserList : EntityList { }

    public partial class UserRepository : EntityRepository
    {
        protected UserRepository() { }

        public User GetBy(string code, string password)
        {
            var user = this.GetByCode(code);

            if (user != null && user.Password == password) { return user; }

            return null;
        }

        public User GetByCode(string code)
        {
            return this.GetFirstBy(new CommonQueryCriteria
            {
                new PropertyMatch(User.CodeProperty, PropertyOperator.Contains, code),
            });
        }
    }

    internal class UserConfig : EntityConfig<User>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(User.CodeProperty, new RequiredRule());
            rules.AddRule(User.NameProperty, new RequiredRule());
            rules.AddRule(User.CodeProperty, new NotDuplicateRule());
        }

        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            //映射 Users 表，避免和 SQLServer 的 User 关键字冲突。
            this.Meta.MapTable("Users").MapProperties(
                User.CodeProperty,
                User.NameProperty,
                User.PasswordProperty,
                User.LastLoginTimeProperty,
                User.LoginCountProperty,
                User.MaxLoginCountProperty
                );

            //Meta.SetSaveListServiceType(typeof(SaveUserService));
        }
    }
}