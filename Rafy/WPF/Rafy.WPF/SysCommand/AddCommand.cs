/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110303
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100303
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;

using Rafy.WPF;
using Rafy.WPF.Behaviors;
using Rafy.WPF.Editors;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Command
{
    [Command(ImageName = "Add.bmp", Label = "添加", GroupType = CommandGroupType.Edit)]
    public class AddCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.CanAddItem();
        }

        public override void Execute(ListLogicalView view)
        {
            view.AddNew(true);
        }
    }

    //[Command("SaveToXaml", Label = "SaveToXaml", ToolTip = "SaveToXaml")]
    //public class SaveToXamlCommand : WPFListViewCommand
    //{
    //    public override void Execute(ListLogicalView view)
    //    {
    //        TaskDialog.Show("Xaml", XamlWriter.Save(
    //            (view.DetailView.Control as FrameworkElement).GetFrameTemplate() as UserControl), "Xaml");
    //    }
    //}
}