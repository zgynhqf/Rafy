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

using System.Linq;

using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 保存整个列表
    /// </summary>
    [Command(ImageName = "Save.bmp", Label = "保存", GroupType = CommandGroupType.Edit)]
    public class SaveListCommand : BaseSaveCommand
    {
        public override void Execute(ObjectView view)
        {
            var listView = view.CastTo<ListObjectView>();

            var list = listView.Data;
            if (list.Count > 0)
            {
                //检测条件
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var item = list[i];
                    var brokenRules = item.ValidationRules.Validate();
                    if (brokenRules.Count > 0)
                    {
                        var msg = string.Format("第 {0} 行数据验证不通过：\r\n{1}", i + 1, brokenRules);
                        App.Current.MessageBox.Show("保存出错", msg);
                        return;
                    }
                }

                RF.Save(list);
            }
        }
    }
}