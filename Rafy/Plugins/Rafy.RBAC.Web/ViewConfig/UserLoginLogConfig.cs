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
using Rafy.Web;

namespace Rafy.RBAC.Web.ViewConfig
{
    internal class UserLoginLogConfig : WebViewConfig<UserLoginLog>
    {
        protected override void ConfigView()
        {
            View.DomainName("用户登录记录").DisableEditing();

            View.Property(UserLoginLog.UserNameProperty).HasLabel("用户").ShowIn(ShowInWhere.List);
            View.Property(UserLoginLog.IsInTextProperty).HasLabel("类型").ShowIn(ShowInWhere.List);
            View.Property(UserLoginLog.LogTimeProperty).HasLabel("时间");
        }
    }
}