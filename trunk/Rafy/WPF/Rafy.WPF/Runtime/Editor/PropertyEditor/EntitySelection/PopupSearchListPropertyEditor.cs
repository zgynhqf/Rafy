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
using Rafy.MetaModel.View;
using Rafy.WPF.Command;
using Rafy.MetaModel.Attributes;
using Rafy.WPF.Command.UI;
using System.Windows.Media;
using Rafy.Domain;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 一个使用弹出列表界面并进行搜索的引用实体属性编辑器。
    /// </summary>
    public class PopupSearchListPropertyEditor : EntitySelectionPropertyEditor
    {
        protected override FrameworkElement CreateEditingElement()
        {
            var propertyName = this.Meta.Name;

            var textbox = new TextBox()
            {
                Name = propertyName,
                IsReadOnly = true,
                Style = RafyResources.StringPropertyEditor_TextBox
            };

            textbox.PreviewMouseLeftButtonDown += (o, e) =>
            {
                this.PopupSelection();
            };

            this.ResetBinding(textbox);

            this.SetAutomationElement(textbox);

            return textbox;
        }

        protected override DependencyProperty BindingProperty()
        {
            return TextBox.TextProperty;
        }

        private void PopupSelection()
        {
            var refInfo = this.Meta.SelectionViewMeta;

            //界面生成
            var block = new Block(refInfo.SelectionEntityType);
            var title = block.ViewMeta.TitleProperty;
            if (title == null) throw new InvalidOperationException("该实体没有标题属性，不能使用此控件进行编辑。");
            block.ViewMeta.DisableEditing();
            block.ViewMeta.AsWPFView().ClearCommands().UseCommands(typeof(ClearReferenceCommand), typeof(SearchListCommand));
            var ui = AutoUI.AggtUIFactory.GenerateControl(block);
            var listView = ui.MainView as ListLogicalView;
            listView.DataLoader.LoadDataAsync(() =>
            {
                this.SyncValueToSelection(listView);
            });

            //弹出
            var res = App.Windows.ShowDialog(ui.Control, w =>
            {
                w.Title = "选择".Translate() + " " + title.Label.Translate();
                w.Width = 600;
                w.Height = 300;

                listView.Control.MouseDoubleClick += (o, e) =>
                {
                    w.DialogResult = true;
                };
            });

            //确定
            if (res == WindowButton.Yes)
            {
                this.SyncSelectionToValue(listView.SelectedEntities);
            }
        }
    }

    [Command(Label = "过滤", UIAlgorithm = typeof(GenericItemAlgorithm<TextBoxButtonItemGenerator>))]
    public class SearchListCommand : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            var txt = TextBoxButtonItemGenerator.GetTextBoxParameter(this);

            if (string.IsNullOrEmpty(txt))
            {
                view.Filter = null;
            }
            else
            {
                var titleManagedProperty = view.Meta.TitleProperty.PropertyMeta.ManagedProperty;
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
        public override void Execute(ListLogicalView view)
        {
            view.Current = null;
        }
    }
}