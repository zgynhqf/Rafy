using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OEA.MetaModel.View;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF.CommandAutoUI;
using System.Windows.Media;
using OEA.Library;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 一个使用平铺的列表来实现的引用实体属性编辑器。
    /// </summary>
    public class TiledListReferenceEditor : ReferencePropertyEditor
    {
        protected override FrameworkElement CreateEditingElement()
        {
            var refInfo = this.PropertyViewInfo.ReferenceViewInfo;

            var listView = AutoUI.ViewFactory.CreateListObjectView(refInfo.RefTypeDefaultView, true);
            listView.DataLoader.LoadDataAsync(() =>
            {
                this.SyncValueToSelection(listView);
            });
            listView.CurrentObjectChanged += (o, e) =>
            {
                this.SyncSelectionToValue(listView.SelectedObjects);
            };

            return listView.Control;
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //do nothing
        }
    }
}