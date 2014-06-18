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
    internal class OrgPositionConfig : WebViewConfig<OrgPosition>
    {
        protected override void ConfigView()
        {
            View.DomainName("岗位").DisableEditing();

            View.UseDefaultCommands().RemoveCommands(WebCommandNames.Add);
            View.UseCommand("Rafy.rbac.cmd.LookupSelectAddOrgPosition")
                .HasLabel("选择岗位")
                .SetExtendedProperty("targetClass", ClientEntities.GetClientName(typeof(Position)));
            View.UseCommands("Rafy.rbac.cmd.ChoosePermissions");

            View.Property(OrgPosition.PositionProperty).HasLabel("岗位");
            View.Property(OrgPosition.View_CodeProperty).ShowIn(ShowInWhere.List).HasLabel("编码");
            View.Property(OrgPosition.View_NameProperty).ShowIn(ShowInWhere.List).HasLabel("名称");
        }
    }
}