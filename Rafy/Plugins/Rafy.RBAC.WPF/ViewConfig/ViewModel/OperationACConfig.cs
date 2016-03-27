/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130830
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130830 11:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.RBAC.Old.WPF.ViewConfig.ViewModel
{
    internal class OperationACConfig : WPFViewConfig<OperationAC>
    {
        protected override void ConfigView()
        {
            View.DisableEditing();

            View.DomainName("权限控制项")
                .HasDelegate(OperationAC.OperationKeyProperty)
                .GroupBy(OperationAC.ScopeKeyLabelProperty);

            View.Property(OperationAC.LabelProperty).HasLabel("名称").ShowIn(ShowInWhere.List);
        }
    }
}