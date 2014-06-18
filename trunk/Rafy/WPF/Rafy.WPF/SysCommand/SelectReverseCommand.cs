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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    [Command(Label = "反选", GroupType = CommandGroupType.View, ImageName = "SelectNone.bmp")]
    public class SelectReverseCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.SelectedEntities.Count > 0;
        }

        public override void Execute(ListLogicalView view)
        {
            using (view.BatchSelection())
            {
                var selection = view.SelectedEntities;

                var toSelect = view.Data.Except(selection).ToArray();

                selection.Clear();
                foreach (var item in toSelect) { selection.Add(item); }
            }
        }
    }
}
