///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110217
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100217
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using OEA.Editors;
//using OEA.MetaModel;
using OEA.MetaModel.View;
//using OEA.Module.WPF;
//using System.Windows.Input;

//namespace OEA.Module.WPF.Editors
//{
//    public class GDataGridBooleanColumn : OpenDataGridColumn
//    {
//        protected GDataGridBooleanColumn() { }

//        protected override IWPFPropertyEditor CreateEditorCore()
//        {
//            return this.EditorFactory.Create<BooleanPropertyEditor>(this.PropertyInfo);
//        }

//        private CheckBox NewCheckBox()
//        {
//            var newEditor = this.CreateEditorCore();
//            return newEditor.Control as CheckBox;
//        }

//        #region Copy From .NET Framework - DataGridCheckBoxColumn

//        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        {
//            var control = this.GenerateCheckBox(cell);

//            control.IsHitTestVisible = false;
//            control.Focusable = false;

//            return control;
//        }

//        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
//        {
//            var control = this.GenerateCheckBox(cell);

//            control.IsHitTestVisible = true;
//            control.Focusable = true;

//            return control;
//        }

//        private CheckBox GenerateCheckBox(DataGridCell cell, bool isEditing = false)
//        {
//            CheckBox element = (cell != null) ? (cell.Content as CheckBox) : null;

//            if (element == null)
//            {
//                element = NewCheckBox();
//            }

//            var binding = BindingOperations.GetBinding(element, CheckBox.IsCheckedProperty);
//            BindingOperations.SetBinding(element, CheckBox.IsCheckedProperty, binding);

//            return element;
//        }

//        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
//        {
//            var checkBox = editingElement as CheckBox;

//            checkBox.Focus();

//            var isChecked = checkBox.IsChecked;

//            if ((IsMouseLeftButtonDown(editingEventArgs) && IsMouseOver(checkBox, editingEventArgs)) || IsSpaceKeyDown(editingEventArgs))
//            {
//                checkBox.IsChecked = new bool?(isChecked != true);
//            }

//            return base.PrepareCellForEdit(editingElement, editingEventArgs);
//        }

//        private static bool IsMouseLeftButtonDown(RoutedEventArgs e)
//        {
//            var args = e as MouseButtonEventArgs;
//            return (((args != null) && (args.ChangedButton == MouseButton.Left)) && (args.ButtonState == MouseButtonState.Pressed));
//        }

//        private static bool IsMouseOver(CheckBox checkBox, RoutedEventArgs e)
//        {
//            return (checkBox.InputHitTest((e as MouseButtonEventArgs).GetPosition(checkBox)) != null);
//        }

//        private static bool IsSpaceKeyDown(RoutedEventArgs e)
//        {
//            KeyEventArgs args = e as KeyEventArgs;
//            return (((args != null) && (((byte)(args.KeyStates & KeyStates.Down)) == 1)) && (args.Key == Key.Space));
//        }

//        #endregion
//    }
//}