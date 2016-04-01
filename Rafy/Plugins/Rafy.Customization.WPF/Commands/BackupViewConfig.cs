/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201202
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201202
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.Customization.WPF.Commands
{
    [Command(Label = "重置", ToolTip = "重置为原始配置")]
    public class BackupViewConfig : ViewCommand
    {
        protected virtual BackupViewConfigService CreateSVC()
        {
            return ServiceFactory.Create<BackupViewConfigService>();
        }

        public override void Execute(LogicalView view)
        {
            var c = view.Current as ViewConfigurationModel;
            var svc = this.CreateSVC();
            svc.Model = c.EntityType;
            svc.ViewName = c.ViewName;
            svc.Invoke();

            view.Current = RF.Concrete<ViewConfigurationModelRepository>()
                .GetBy(new ViewConfigurationModelNameCriteria
                {
                    EntityType = c.EntityType,
                    ViewName = c.ViewName
                });
        }
    }

    [Command(Label = "还原", ToolTip = "还原为最新的配置")]
    public class RestoreViewConfig : BackupViewConfig
    {
        protected override BackupViewConfigService CreateSVC()
        {
            return ServiceFactory.Create<RestoreViewConfigService>();
        }
    }

    [Command(Label = "XML", ToolTip = "打开XML文件进行编辑")]
    public class OpenConfigFile : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            var c = view.Current as ViewConfigurationModel;
            var svc = ServiceFactory.Create<GetBlockConfigFileService>();
            svc.Model = c.EntityType;
            svc.ViewName = c.ViewName;
            svc.Invoke();

            if (!svc.Opened)
            {
                App.MessageBox.Show("暂时还没有进行任何配置，没有找到对应的 XML 文件。".Translate());
            }
        }
    }
}