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
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA;

namespace OEA.RBAC
{
    [RootEntity, Serializable]
    public class User : Entity
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

        #region  Data Access

        protected override void OnInsert()
        {
            this.CheckUniqueCode();
            base.OnInsert();
        }

        protected override void OnUpdate()
        {
            this.CheckUniqueCode();
            base.OnUpdate();
        }

        private void CheckUniqueCode()
        {
            using (var db = this.CreateDb())
            {
                var count = db.Select(db.Query(typeof(User))
                    .Constrain(User.CodeProperty).Equal(this.Code)
                    .And().Constrain(User.IdProperty).NotEqual(this.Id)
                    ).Count;
                if (count > 0) { throw new FriendlyMessageException("已经有这个编码的用户了。"); }
            }
        }

        #endregion
    }

    [Serializable]
    public partial class UserList : EntityList
    {
        protected void QueryBy(string code)
        {
            this.QueryDb(q => q.Constrain(User.CodeProperty).Equal(code));
        }
    }

    public class UserRepository : EntityRepository
    {
        protected UserRepository() { }

        public User GetByCode(string code)
        {
            return this.FetchFirstAs<User>(code);
        }

        public User GetBy(string code, string password)
        {
            var user = this.GetByCode(code);

            if (user != null && user.Password == password) { return user; }

            return null;
        }
    }

    internal class UserConfig : EntityConfig<User>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            this.Meta.MapTable().HasColumns(
                User.CodeProperty,
                User.NameProperty,
                User.PasswordProperty,
                User.LastLoginTimeProperty,
                User.LoginCountProperty,
                User.MaxLoginCountProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasDelegate(User.NameProperty).DomainName("用户");

            if (!OEAEnvironment.IsWeb)
            {
                View.UseWPFCommands(
                    "RBAC.ModifyUserPasswordCommand"
                    );
            }

            View.Property(User.CodeProperty).HasLabel("登录代号").ShowIn(ShowInWhere.ListDropDown);
            View.Property(User.NameProperty).HasLabel("姓名").ShowIn(ShowInWhere.List);
            View.Property(User.PasswordProperty).HasLabel("密码").UseEditor(WPFEditorNames.Password);
            View.Property(User.LastLoginTimeProperty).HasLabel("最后登录时间");
            View.Property(User.LoginCountProperty).HasLabel("剩余登录次数").UseEditor(WPFEditorNames.Password);
        }
    }
}