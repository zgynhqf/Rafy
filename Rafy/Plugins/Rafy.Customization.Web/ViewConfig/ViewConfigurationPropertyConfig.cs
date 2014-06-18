/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 13:57
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.Customization.Web.ViewConfig
{
    internal class ViewConfigurationPropertyConfig : WebViewConfig<ViewConfigurationProperty>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(ViewConfigurationProperty.LabelProperty);

            View.WithoutPaging();
            View.ClearCommands().UseCommands(WebCommandNames.Edit);

            View.Property(ViewConfigurationProperty.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All).Readonly();
            View.Property(ViewConfigurationProperty.LabelProperty).HasLabel("标题").ShowIn(ShowInWhere.All);
            View.Property(ViewConfigurationProperty.ShowInWhereProperty).HasLabel("显示信息").ShowIn(ShowInWhere.All);
            View.Property(ViewConfigurationProperty.OrderNoProperty).HasLabel("排序字段").ShowIn(ShowInWhere.All);
        }
    }
}