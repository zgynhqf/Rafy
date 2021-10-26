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
    internal class ViewConfigurationModelConfig : WebViewConfig<ViewConfigurationModel>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(ViewConfigurationModel.ViewNameProperty);//.HasLabel("界面配置信息");

            View.WithoutPaging();

            View.Property(ViewConfigurationModel.PageSizeProperty).HasLabel("分页条数").ShowIn(ShowInWhere.All);
            //使用自定义的聚合保存按钮。
            View.RemoveCommands(WebCommandNames.CommonCommands)
                .UseCommands(
                "Rafy.customization.cmd.SaveViewConfig",
                "Rafy.customization.cmd.BackupViewConfig",
                "Rafy.customization.cmd.RestoreViewConfig",
                "Rafy.customization.cmd.OpenConfigFile"
                );

            using (View.OrderProperties())
            {
                View.Property(ViewConfigurationModel.EntityTypeProperty).HasLabel("实体").ShowIn(ShowInWhere.All).Readonly();
                View.Property(ViewConfigurationModel.ViewNameProperty).HasLabel("界面名称").ShowIn(ShowInWhere.All).Readonly();
                View.Property(ViewConfigurationModel.GroupByProperty).HasLabel("按属性分组").ShowIn(ShowInWhere.All);
            }
        }
    }
}