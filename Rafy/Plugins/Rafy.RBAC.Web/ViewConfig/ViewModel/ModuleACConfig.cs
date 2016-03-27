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

namespace Rafy.RBAC.Old.Web.ViewConfig.ViewModel
{
    internal class ModuleACConfig : WebViewConfig<ModuleAC>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(ModuleAC.KeyLabelProperty).DomainName("界面模块");

            View.Property(ModuleAC.KeyLabelProperty).HasLabel("模块").ShowIn(ShowInWhere.ListDropDown);
        }
    }
}
