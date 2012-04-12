/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120408
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120408
 * 
*******************************************************/

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
    /// 一个使用弹出列表界面并进行搜索的引用实体属性编辑器。
    /// </summary>
    public class PopupSearchListPropertyEditor : ReferencePropertyEditor
    {
        protected override FrameworkElement CreateEditingElement()
        {
            var propertyName = this.PropertyViewInfo.Name;

            var textbox = new TextBox()
            {
                Name = propertyName,
                IsReadOnly = true,
                Style = OEAStyles.StringPropertyEditor_TextBox
            };

            textbox.PreviewMouseLeftButtonDown += (o, e) =>
            {
                this.PopupSelection();
            };

            this.ResetBinding(textbox);

            this.SetAutomationElement(textbox);

            return textbox;
        }

        protected override void ResetBinding(FrameworkElement editingControl)
        {
            editingControl.SetBinding(TextBox.TextProperty, this.CreateBinding());
        }

        private void PopupSelection()
        {
            var refInfo = this.PropertyViewInfo.ReferenceViewInfo;

            //界面生成
            var block = new Block(refInfo.RefType);
            var title = block.EVM.TitleProperty;
            if (title == null) throw new InvalidOperationException("该实体没有标题属性，不能使用此控件进行编辑。");
            block.EVM.NotAllowEdit().ClearWPFCommands()
                .UseWPFCommands(typeof(ClearReferenceCommand), typeof(SearchListCommand));
            var ui = AutoUI.AggtUIFactory.GenerateControl(block);
            var listView = ui.MainView as IListObjectView;
            listView.DataLoader.LoadDataAsync(() =>
            {
                this.SyncValueToSelection(listView);
            });

            //弹出
            var res = App.Current.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = "选择" + title.Label;
                w.Width = 600;
                w.Height = 300;

                listView.MouseDoubleClick += (o, e) =>
                {
                    w.DialogResult = true;
                };
            });

            //确定
            if (res == WindowButton.Yes)
            {
                this.SyncSelectionToValue(listView.SelectedObjects);
            }
        }
    }

    [Command(Label = "过滤", UIAlgorithm = typeof(GenericItemAlgorithm<TextBoxButtonItemGenerator>))]
    public class SearchListCommand : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            var txt = this.TryGetCustomParams<string>(CommandCustomParams.TextBox);

            var titleManagedProperty = view.Meta.TitleProperty.PropertyMeta.ManagedProperty;
            if (titleManagedProperty == null) throw new InvalidOperationException("标题属性应该使用托管属性进行编写。");

            if (string.IsNullOrEmpty(txt))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = e =>
                {
                    var title = e.GetProperty(titleManagedProperty) as string;
                    return title.Contains(txt);
                };
            }
        }
    }

    [Command(Label = "清空")]
    public class ClearReferenceCommand : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            view.Current = null;
        }
    }
}