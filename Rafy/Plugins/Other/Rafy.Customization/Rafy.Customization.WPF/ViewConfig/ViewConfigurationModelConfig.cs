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
using Rafy.Customization.WPF.Commands;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.Customization.WPF.ViewConfig
{
    internal class ViewConfigurationModelConfig : WPFViewConfig<ViewConfigurationModel>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(ViewConfigurationModel.ViewNameProperty);//.HasLabel("界面配置信息");

            //使用自定义的聚合保存按钮。
            View.ClearCommands()
                .UseCommands(
                    WPFCommandNames.SaveBill, WPFCommandNames.Cancel,
                    typeof(BackupViewConfig),
                    typeof(RestoreViewConfig),
                    typeof(OpenConfigFile)
                //如果 WPF 是一个单独的 dll，则使用使用字符串的方式来声明。
                //"Rafy.WPF.Command.BackupViewConfig",
                //"Rafy.WPF.Command.RestoreViewConfig",
                //"Rafy.WPF.Command.OpenConfigFile"
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
