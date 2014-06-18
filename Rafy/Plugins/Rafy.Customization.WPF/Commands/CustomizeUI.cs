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
using System.Windows;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Web;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.Customization.WPF.Commands
{
    [Command(Label = "界面设置", ToolTip = "实施人员使用的界面配置功能，只在调试期可见。",
        GroupType = CommandGroupType.System, ImageName = "Setting.png")]
    public class CustomizeUI : ViewCommand
    {
        public override void Execute(LogicalView view)
        {
            var blocks = UIModel.AggtBlocks.GetDefinedBlocks("ViewConfigurationModel模块界面");

            var ui = AutoUI.AggtUIFactory.GenerateControl(blocks);

            ui.MainView.DataLoader.LoadDataAsync(() =>
            {
                var model = RF.Concrete<ViewConfigurationModelRepository>()
                    .GetByName(new ViewConfigurationModelNameCriteria
                    {
                        EntityType = ClientEntities.GetClientName(view.EntityType),
                        ViewName = view.Meta.ExtendView
                    });

                return model;
            });

            App.Windows.ShowWindow(ui.Control, w =>
            {
                w.Title = "定制".Translate() + " " + view.Meta.Label.Translate();
                w.WindowClosedByUser += (o, e) =>
                {
                    if (e.Button == WindowButton.Yes)
                    {
                        //先执行保存。
                        ui.MainView.Commands[WPFCommandNames.SaveBill].TryExecute();

                        if (!App.Windows.HasPopup)
                        {
                            var res = App.MessageBox.Show("重新打开当前模块以使设置生效？".Translate(), MessageBoxButton.YesNo);
                            if (res == MessageBoxResult.Yes)
                            {
                                var ws = App.Current.Workspace;
                                var aw = ws.ActiveWindow;
                                if (aw != null && ws.TryRemove(aw))
                                {
                                    App.Current.OpenModuleOrAlert(aw.Title);
                                }
                            }
                        }
                        else
                        {
                            App.MessageBox.Show("下次打开此弹出窗口时生效。".Translate(), MessageBoxButton.OK);
                        }
                    }
                };
            });
        }
    }
}