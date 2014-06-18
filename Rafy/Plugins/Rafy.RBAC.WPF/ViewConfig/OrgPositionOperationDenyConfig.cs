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

namespace Rafy.RBAC.WPF.ViewConfig
{
    internal class OrgPositionOperationDenyConfig : WPFViewConfig<OrgPositionOperationDeny>
    {
        protected override void ConfigView()
        {
            View.DomainName("功能权限");
            View.ClearCommands().UseCommands(typeof(ExpandAllModules));
        }
    }
}