using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.Attributes;
using OEA.WPF.Command;
using SimpleCsla;
using OEA.Library.Modeling.Web;
using OEA.Library;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    [Command(Label = "重置", ToolTip = "重置为原始配置")]
    public class BackupViewConfig : ViewCommand
    {
        protected virtual BackupViewConfigService CreateSVC()
        {
            return new BackupViewConfigService();
        }

        public override void Execute(ObjectView view)
        {
            var c = view.Current as ViewConfigurationModel;
            var svc = this.CreateSVC();
            svc.Model = c.EntityType;
            svc.ViewName = c.ViewName;
            svc.Invoke();

            view.Current = RF.Concreate<ViewConfigurationModelRepository>()
                .GetByName(new ViewConfigurationModelNameCriteria
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
            return new RestoreViewConfigService();
        }
    }

    [Command(Label = "XML", ToolTip = "打开XML文件进行编辑")]
    public class OpenConfigFile : ViewCommand
    {
        public override void Execute(ObjectView view)
        {
            var c = view.Current as ViewConfigurationModel;
            var svc = new GetBlockConfigFileService();
            svc.Model = c.EntityType;
            svc.ViewName = c.ViewName;
            svc.Invoke(out svc);

            if (!svc.Opened)
            {
                App.Current.MessageBox.Show("提示", "暂时还没有进行任何配置，没有找到对应的 XML 文件。");
            }
        }
    }
}