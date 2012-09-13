/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120906 11:11
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120906 11:11
 * 
*******************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Xml.Linq;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;

namespace OEA.WPF.Command
{
    [Command(Label = "报表数据", ToolTip = "查看报表对应的列表数据", GroupType = CommandGroupType.System)]
    public class ShowReportData : ClientCommand<ReportObjectView>
    {
        public override bool CanExecute(ReportObjectView view)
        {
            return view.Data is EntityList;
        }

        public override void Execute(ReportObjectView view)
        {
            var listView = AutoUI.ViewFactory.CreateListObjectView(view.Meta);
            listView.IsReadOnly = true;

            App.Windows.ShowWindow(listView.Control, w =>
            {
                w.Title = this.Label;
                w.Buttons = ViewDialogButtons.None;
            });

            listView.Data = view.Data as EntityList;
            //listView.DataLoader.LoadDataAsync(() => view.Data);
        }
    }
}