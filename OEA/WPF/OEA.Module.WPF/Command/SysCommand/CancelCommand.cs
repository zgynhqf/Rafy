/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110302
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100302
 * 
*******************************************************/

using System;
using System.Linq;
using SimpleCsla.Core;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;

using OEA.Module.WPF.Behaviors;

namespace OEA.WPF.Command
{
    [Command(ImageName = "Cancel.bmp", Label = "取消", ToolTip = "取消更改", GroupType = CommandGroupType.Edit)]
    public class CancelCommand : ViewCommand
    {
        public override bool CanExecute(ObjectView view)
        {
            var currentObject = view.Data as IDirtyAware;
            return currentObject != null &&
                currentObject.IsDirty;
        }

        public override void Execute(ObjectView view)
        {
            int? oldCurObjId = null;
            var listView = view as ListObjectView;
            if (listView != null)
            {
                var e = view.Current;
                if (e != null) oldCurObjId = e.Id;
            }

            var b = view.FindBehavior<SaveAsChangedBehavior>();
            if (b != null) b.SuppressSaveAction = true;

            //重新获取数据
            view.DataLoader.ReloadData();

            if (b != null) b.SuppressSaveAction = false;

            //如果之前已经选中了某一行，这里只需要再次设置为该行就可以了。
            if (listView != null && oldCurObjId.HasValue)
            {
                var curObj = listView.Data.OfType<Entity>().FirstOrDefault(e => e.Id == oldCurObjId.Value);
                if (curObj != null) listView.Current = curObj;
            }
        }
    }
}
