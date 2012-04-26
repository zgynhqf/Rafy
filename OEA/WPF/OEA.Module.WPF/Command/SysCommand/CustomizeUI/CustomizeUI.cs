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
    [Command(Label = "界面设置", ToolTip = "实施人员使用的界面配置功能，只在调试期可见。", GroupType = CommandGroupType.Edit)]
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

            App.Windows.ShowWindow(ui.Control, w =>
            {
                w.Title = "定制" + view.Meta.Label;
                w.WindowClosedByUser += (o, e) =>
                {
                    if (e.Button == WindowButton.Yes)
                    {
                        if (!App.Windows.HasPopup)
                        {
                            var res = App.MessageBox.Show("重新打开当前模块以使设置生效？", MessageBoxButton.YesNo);
                            if (res == MessageBoxResult.Yes)
                            {
                                var ws = App.Current.Workspace;
                                var aw = ws.ActiveWindow;
                                if (aw != null && ws.TryRemove(aw))
                                {
                                    var title = WorkspaceWindow.GetTitle(aw);
                                    App.Current.OpenModuleOrAlert(title);
                                }
                            }
                        }
                        else
                        {
                            App.MessageBox.Show("下次打开此弹出窗口时生效。", MessageBoxButton.OK);
                        }
                    }
                };
            });
        }
    }
}