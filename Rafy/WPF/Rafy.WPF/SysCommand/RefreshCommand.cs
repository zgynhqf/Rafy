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
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    [Command(ImageName = "Refresh.bmp", Label = "刷新", ToolTip = "刷新列表", GroupType = CommandGroupType.Edit)]
    public class RefreshCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Data != null && !view.Data.IsDirty && view.DataLoader.AnyLoaded;
        }
        /// <summary>
        /// 如果存在查询和导航面板，则刷新查询和导航面板的下拉列表框数据
        /// </summary>
        /// <param name="view"></param>
        public override void Execute(ListLogicalView view)
        {
            view.DataLoader.ReloadDataAsync();

            //QueryLogicalView queryView = view.NavigateQueryView;
            //if (queryView == null) { queryView = view.CondtionQueryView; }

            ////刷新查询或导航面板的下拉列表框数据
            //if (queryView != null)
            //{
            //    queryView.TryExecuteQuery(view);

            //    foreach (var item in queryView.PropertyEditors)
            //    {
            //        if (item.PropertyViewInfo.EditorName == WPFEditorNames.LookupDropDown)
            //        {
            //            (item as LookupListPropertyEditor).DataSourse = null;
            //        }
            //    }
            //    queryView.AttachNewQueryObject();
            //}
            //else
            //{
            //    view.DataLoader.LoadDataAsync();
            //}
        }
    }
}
