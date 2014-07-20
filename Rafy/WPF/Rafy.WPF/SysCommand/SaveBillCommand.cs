/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111215
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 保存一条记录
    /// </summary>
    [Command(ImageName = "Save.bmp", Label = "保存", GroupType = CommandGroupType.Edit)]
    public class SaveBillCommand : BaseSaveCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            var data = view.Current;
            return data != null && data.IsDirty;
        }

        public override void Execute(LogicalView view)
        {
            //检测条件
            var current = view.Current;
            var brokenRules = current.Validate();
            if (brokenRules.Count > 0)
            {
                App.MessageBox.Show(brokenRules.ToString(), "保存出错".Translate());
                return;
            }

            RF.Save(current, EntitySaveType.DiffSave);
            this.OnSaveSuccessed();
        }
    }
}