/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120710 16:15
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120710 16:15
 * 
*******************************************************/

using System.Linq;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.Editors;

namespace OEA.WPF.Command
{
    [Command(Label = "反选", GroupType = CommandGroupType.View)]
    public class SelectReverseCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return view.SelectedEntities.Count > 0;
        }

        public override void Execute(ListObjectView view)
        {
            var selection = view.SelectedEntities;

            var toSelect = view.Data.Except(selection).ToArray();

            selection.Clear();
            foreach (var item in toSelect) { selection.Add(item); }
        }
    }
}
