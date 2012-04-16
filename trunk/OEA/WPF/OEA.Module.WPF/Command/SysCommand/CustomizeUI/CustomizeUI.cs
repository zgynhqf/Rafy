using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.WPF.Command;
using OEA.Library;
using System.Windows;
using OEA.MetaModel;

namespace OEA.WPF.Command
{
    [Command(Label = "界面设置", GroupType = CommandGroupType.Edit)]
    public class CustomizeUI : ViewCommand
    {
        public override void Execute(ObjectView view)
        {
            var blocks = UIModel.AggtBlocks.GetDefinedBlocks("ViewConfigurationModel模块界面");

            var ui = AutoUI.AggtUIFactory.GenerateControl(blocks);

            ui.MainView.DataLoader.LoadDataAsync(() =>
            {
                var model = RF.Concreate<ViewConfigurationModelRepository>()
                    .GetByName(new ViewConfigurationModelNameCriteria
                    {
                        EntityType = ClientEntityConverter.ToClientName(view.EntityType),
                        ViewName = view.Meta.ExtendView
                    });

                return model;
            });

            App.Current.Windows.ShowWindow(ui.Control, w =>
            {
                w.Title = "定制" + view.Meta.Label;
                w.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width * 0.8;
                w.WindowClosedByUser += (o, e) =>
                {
                    if (e.Button == WindowButton.Yes)
                    {
                        var res = App.Current.MessageBox.Show("提示", "重新打开当前模块以使设置生效？", MessageBoxButton.YesNo);
                        if (res == MessageBoxResult.Yes)
                        {
                            var ws = App.Current.Workspace;
                            var aw = ws.ActiveWindow;
                            if (ws.TryRemove(aw))
                            {
                                App.Current.OpenModuleOrAlert(aw.Title);
                            }
                        }
                    }
                };
            });
        }
    }
}