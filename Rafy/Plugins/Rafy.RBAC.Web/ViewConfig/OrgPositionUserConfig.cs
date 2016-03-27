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
using Rafy.RBAC.Old.Audit;
using Rafy.Web;

namespace Rafy.RBAC.Old.Web.ViewConfig
{
    internal class OrgPositionUserConfig : WebViewConfig<OrgPositionUser>
    {
        protected override void ConfigView()
        {
            View.DomainName("岗位用户").DisableEditing();

            View.UseDefaultCommands().UseCommand("Rafy.rbac.cmd.LookupSelectAddOrgPositionUser")
                .HasLabel("选择用户")
                .SetExtendedProperty("targetClass", ClientEntities.GetClientName(typeof(User)));
            View.RemoveCommands(WebCommandNames.Add);

            View.Property(OrgPositionUser.View_CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.List);
            View.Property(OrgPositionUser.View_NameProperty).HasLabel("名称").ShowIn(ShowInWhere.List);
        }
    }
}