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

namespace Rafy.RBAC.Old.Web.ViewConfig
{
    internal class AuditItemConfig : WebViewConfig<AuditItem>
    {
        protected override void ConfigView()
        {
            View.DomainName("日志").DisableEditing();

            View.UseCommands(
                WebCommandNames.Delete,
                WebCommandNames.Save,
                WebCommandNames.Refresh,
                "Rafy.rbac.cmd.ClearAuditLogCommand"
                );

            using (View.OrderProperties())
            {
                View.Property(AuditItem.TitleProperty).ShowIn(ShowInWhere.All).HasLabel("标题");
                View.Property(AuditItem.ContentProperty).ShowIn(ShowInWhere.All).HasLabel("内容");
                View.Property(AuditItem.UserProperty).ShowIn(ShowInWhere.All).HasLabel("用户");
                View.Property(AuditItem.MachineNameProperty).ShowIn(ShowInWhere.All).HasLabel("机器名");
                View.Property(AuditItem.ModuleNameProperty).ShowIn(ShowInWhere.List).HasLabel("模块名");
                View.Property(AuditItem.TypeProperty).ShowIn(ShowInWhere.List).HasLabel("类型");
                View.Property(AuditItem.LogTimeProperty).ShowIn(ShowInWhere.List).HasLabel("时间");
            }
        }
    }
}