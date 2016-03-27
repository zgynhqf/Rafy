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
    internal class AuditItemConditionCriteriaConfig : WPFViewConfig<AuditItemConditionCriteria>
    {
        protected override void ConfigView()
        {
            View.HasDetailColumnsCount(1);

            using (View.OrderProperties())
            {
                View.Property(AuditItemConditionCriteria.StartTimeProperty).ShowIn(ShowInWhere.Detail).HasLabel("起始时间");
                View.Property(AuditItemConditionCriteria.EndTimeProperty).ShowIn(ShowInWhere.Detail).HasLabel("终止时间");
                View.Property(AuditItemConditionCriteria.ModuleACProperty).ShowIn(ShowInWhere.Detail).HasLabel("模块名");
                View.Property(AuditItemConditionCriteria.AuditLogTypeProperty).ShowIn(ShowInWhere.Detail).HasLabel("日志类型");
                View.Property(AuditItemConditionCriteria.TitleKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("标题关键字");
                View.Property(AuditItemConditionCriteria.ContentKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("内容关键字");
                View.Property(AuditItemConditionCriteria.UserKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("用户名关键字");
                View.Property(AuditItemConditionCriteria.MachineKeyWordsProperty).ShowIn(ShowInWhere.Detail).HasLabel("机器名关键字");
            }
        }
    }
}