using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.WPF.Command;
using Rafy.MetaModel.Attributes;
using Rafy.WPF;
using Rafy.WPF.Command.UI;
using Rafy.MetaModel.View;
using Rafy;
using Rafy.WPF.Editors;
using Rafy.MetaModel;
using System.Windows;

namespace JXC.Commands
{
    [Command(Label = "重置商品数据（调试）")]
    public class ResetProductAmountCommand : ClientCommand<ListLogicalView>
    {
        public override void Execute(ListLogicalView view)
        {
            foreach (Product item in view.SelectedEntities)
            {
                item.StorageAmount = 0;
            }
        }
    }

    [Command(Label = "刷新")]
    public class RefreshProductNavigation : ClientCommand<QueryLogicalView>
    {
        public override void Execute(QueryLogicalView view)
        {
            //var e = view.Current.CastTo<ProductNavigationCriteria>();
            //var oldSelection = e.ProductCategoryId;
            //editor.ListView.DataLoader.LoadDataAsync(() =>
            //{
            //    e.ProductCategoryId = oldSelection;
            //});

            var editor = view.FindPropertyEditor(ProductNavigationCriteria.ProductCategoryProperty)
                as TiledListReferenceEditor;
            editor.RefreshDataSource();
        }
    }

    [Command(Label = "维护分类")]
    public class OpenProductCategory : ClientCommand<QueryLogicalView>
    {
        public override void Execute(QueryLogicalView view)
        {
            var moduleMeta = CommonModel.Modules[typeof(ProductCategory)] as WPFModuleMeta;
            if (PermissionMgr.CanShowModule(moduleMeta))
            {
                var page = App.Current.CreateModule(moduleMeta);
                var btn = App.Windows.ShowDialog(page.WindowControl, w =>
                {
                    w.Title = "维护分类".Translate();
                });
                if (btn == WindowButton.Yes)
                {
                    view.Commands[typeof(RefreshProductNavigation)].TryExecute();
                }
            }
        }
    }
}