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

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    [Command(Label = "清除条件", ToolTip = "清除条件", GroupType = CommandGroupType.Edit)]
    public class ClearQueryConditionCommand : ViewCommand
    {
        public override bool CanExecute(ObjectView view)
        {
            return base.CanExecute(view) && view.Current != null;
        }

        public override void Execute(ObjectView view)
        {
            view.CastTo<QueryObjectView>().AttachNewQueryObject();
        }
    }
}