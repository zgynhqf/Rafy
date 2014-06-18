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
using System.Windows.Input;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Behaviors;

namespace Rafy.WPF.Command
{
    [Command(ImageName = "Cancel.bmp", Label = "取消", ToolTip = "取消更改", GroupType = CommandGroupType.Edit)]
    public class CancelCommand : ViewCommand
    {
        public override bool CanExecute(LogicalView view)
        {
            var data = view.Data as IDirtyAware;
            return data != null && data.IsDirty && !view.DataLoader.IsLoadingData;
        }

        public override void Execute(LogicalView view)
        {
            //ListLogicalView 需要被特殊处理。
            object oldCurObjId = null;
            var listView = view as ListLogicalView;
            if (listView != null)
            {
                var e = view.Current;
                if (e != null) oldCurObjId = e.Id;
            }

            //再尝试对 SaveAsChangedBehavior 进行特殊处理。
            var b = view.FindBehavior<SaveAsChangedBehavior>();
            if (b != null) b.SuppressSaveAction = true;

            var loader = view.DataLoader;
            if (loader.AnyLoaded)
            {
                //重新获取数据
                view.DataLoader.ReloadDataAsync(() =>
                {
                    //如果之前已经选中了某一行，这里只需要再次设置为该行就可以了。
                    if (oldCurObjId != null)
                    {
                        listView.SetCurrentById(oldCurObjId);
                    }
                });
            }
            else
            {
                //当数据不是由 DataLoader 加载的时候，说明这些数据来自于手工设置，调用 CancelCustomData 方法撤消。
                this.CancelCustomData(view);
            }

            if (b != null) b.SuppressSaveAction = false;
        }

        /// <summary>
        /// 子类重写此方法实现撤消手工设置的数据。
        /// 
        /// 默认实现是直接设置 view.Data 为 null。
        /// </summary>
        /// <param name="view"></param>
        protected virtual void CancelCustomData(LogicalView view)
        {
            view.Data = null;
        }
    }
}
