/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111214
 * 
*******************************************************/

using Rafy;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace Rafy.WPF.Command
{
    [Command(Label = "清除条件", ToolTip = "清除条件", GroupType = CommandGroupType.Edit)]
    public class ClearQueryConditionCommand : ViewCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            return base.CanExecute(view) && view.Current != null;
        }

        public override void Execute(LogicalView view)
        {
            view.CastTo<QueryLogicalView>().AttachNewCriteria();
        }
    }
}