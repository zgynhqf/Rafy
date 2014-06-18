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
    internal class ViewConfigurationCommandConfig : WebViewConfig<ViewConfigurationCommand>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(ViewConfigurationCommand.LabelProperty);

            View.WithoutPaging();
            View.ClearCommands().UseCommands(WebCommandNames.Edit);

            using (View.OrderProperties())
            {
                View.Property(ViewConfigurationCommand.NameProperty).HasLabel("命令类型").ShowIn(ShowInWhere.All).Readonly();
                View.Property(ViewConfigurationCommand.LabelProperty).HasLabel("命令名称").ShowIn(ShowInWhere.All);
                View.Property(ViewConfigurationCommand.IsVisibleProperty).HasLabel("是否可见").ShowIn(ShowInWhere.All);
            }
        }
    }
}