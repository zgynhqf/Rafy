using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.CommandAutoUI;
using OEA.MetaModel.View;
using OEA;
using OEA.Module.WPF.Editors;
using OEA.MetaModel;
using System.Windows;
using Itenso.Windows.Input;

namespace JXC.Commands
{
    [Command(Label = "刷新")]
    public class RefreshProductNavigation : ClientCommand<QueryObjectView>
    {
        public override void Execute(QueryObjectView view)
        {
            //var e = view.Current.CastTo<ProductNavigationCriteria>();
            //var oldSelection = e.ProductCategoryId;
            //editor.ListView.DataLoader.LoadDataAsync(() =>
            //{
            //    e.ProductCategoryId = oldSelection;
            //});

            var editor = view.FindPropertyEditor(ProductNavigationCriteria.ProductCategoryRefProperty)
                as TiledListReferenceEditor;
            editor.RefreshDataSource();
        }
    }

    [Command(Label = "维护分类")]
    public class OpenProductCategory : ClientCommand<QueryObjectView>
    {
        public override void Execute(QueryObjectView view)
        {
            var moduleMeta = CommonModel.Modules[typeof(ProductCategory)];
            if (PermissionMgr.Provider.CanShowModule(moduleMeta))
            {
                var page = App.Current.CreateModule(moduleMeta);
                var btn = App.Windows.ShowDialog(page as FrameworkElement, w =>
                {
                    w.Title = "维护分类";
                });
                if (btn == WindowButton.Yes)
                {
                    view.Commands[typeof(RefreshProductNavigation)].Execute(view);
                }
            }
        }
    }
}