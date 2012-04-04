using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.WPF.Command;
using OEA.Library;

namespace OEA.WPF.Command
{
    [Command(Label = "界面配置", GroupType = CommandGroupType.Edit)]
    public class CustomizeUI : ViewCommand
    {
        public override void Execute(ObjectView view)
        {
            var blocks = UIModel.AggtBlocks.GetDefinedBlocks("ViewConfigurationModel模块界面");

            var cr = AutoUI.AggtUIFactory.GenerateControl(blocks);

            App.Current.Windows.ShowWindow(cr.Control, w =>
            {
                w.Title = "定制" + view.Meta.Label;
                w.Width = 1000;
            });

            cr.MainView.DataLoader.LoadDataAsync(() =>
            {
                var model = RF.Concreate<ViewConfigurationModelRepository>()
                    .GetByName(new ViewConfigurationModelNameCriteria
                    {
                        EntityType = ClientEntityConverter.ToClientName(view.EntityType),
                        ViewName = view.Meta.ExtendView
                    });

                return model;
            });
        }
    }
}