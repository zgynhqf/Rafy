/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130830
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130830 15:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC.Audit;
using Rafy.RBAC.WPF;
using Rafy.Web;

namespace Rafy.RBAC.WPF.ViewConfig
{
    internal class UserConfig : WPFViewConfig<User>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(User.NameProperty).DomainName("用户");

            View.UseDefaultCommands().UseCommands(typeof(ModifyUserPasswordCommand));

            View.Property(User.CodeProperty).HasLabel("登录代号").ShowIn(ShowInWhere.All);
            View.Property(User.NameProperty).HasLabel("姓名").ShowIn(ShowInWhere.ListDetail);
            View.Property(User.PasswordProperty).HasLabel("密码").UseEditor(WPFEditorNames.Password);
            View.Property(User.LastLoginTimeProperty).HasLabel("最后登录时间");
            View.Property(User.LoginCountProperty).HasLabel("剩余登录次数").UseEditor(WPFEditorNames.Password);
        }
    }
}