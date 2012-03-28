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

using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 保存一条记录
    /// </summary>
    [Command(ImageName = "Save.bmp", Label = "保存", GroupType = CommandGroupType.Edit)]
    public class SaveBillCommand : BaseSaveCommand
    {
        protected override IDirtyAware GetCheckData(ObjectView view)
        {
            return view.Current as IDirtyAware;
        }

        public override void Execute(ObjectView view)
        {
            var currentObject = view.Current as Entity;

            //检测条件
            var denpendentObject = currentObject as IDenpendentObject;
            if (denpendentObject != null)
            {
                denpendentObject.CheckRules();
                if (denpendentObject.BrokenRulesCollection.Count > 0)
                {
                    App.Current.MessageBox.Show("保存出错", denpendentObject.BrokenRulesCollection[0].Description);
                    return;
                }
            }

            RF.Save(currentObject);
        }
    }
}