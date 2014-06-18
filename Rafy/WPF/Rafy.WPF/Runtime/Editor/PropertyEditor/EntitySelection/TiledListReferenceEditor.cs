using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.MetaModel.View;
using Rafy.WPF.Command;
using Rafy.MetaModel.Attributes;
using Rafy.WPF.Command.UI;
using System.Windows.Media;
using Rafy.Domain;
using Rafy.MetaModel;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 一个使用平铺的列表来实现的引用实体属性编辑器。
    /// </summary>
    public class TiledListReferenceEditor : EntitySelectionPropertyEditor
    {
        /// <summary>
        /// 手动设置下拉的数据源
        /// </summary>
        public ListLogicalView ListView { get; private set; }

        /// <summary>
        /// 外部可以通过这个方法重新刷新编辑器的数据。
        /// </summary>
        public void RefreshDataSource()
        {
            this.ListView.DataLoader.LoadDataAsync(() =>
            {
                this.SyncValueToSelection(this.ListView);
            });
        }

        protected override FrameworkElement CreateEditingElement()
        {
            var refInfo = this.Meta.SelectionViewMeta;

            var listView = AutoUI.ViewFactory.CreateListView(refInfo.RefTypeDefaultView.AsWPFView(), true);
            this.ListView = listView;

            listView.IsReadOnly = ReadOnlyStatus.ReadOnly;
            this.RefreshDataSource();
            listView.CurrentChanged += (o, e) =>
            {
                this.SyncSelectionToValue(listView.SelectedEntities);
            };

            return listView.Control;
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //do nothing
        }

        protected override DependencyProperty BindingProperty()
        {
            return null;
        }
    }
}