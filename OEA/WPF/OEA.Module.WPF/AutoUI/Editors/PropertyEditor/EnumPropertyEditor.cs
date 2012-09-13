/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Linq;
using OEA.Reflection;
using OEA.Utils;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 使用下拉实现的枚举属性编辑器
    /// </summary>
    public class EnumPropertyEditor : WPFPropertyEditor
    {
        private ComboBox _cb;

        protected EnumPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            this._cb = new ComboBox()
            {
                Name = Meta.Name
            };

            this.ResetBinding(this._cb);

            //cb.Items.Clear();
            var propertyType = this.Meta.PropertyMeta.Runtime.PropertyType;
            foreach (var item in EnumViewModel.GetByEnumType(propertyType))
            {
                this._cb.Items.Add(item);
            }
            this._cb.SelectedValuePath = "EnumValue";
            this._cb.DisplayMemberPath = "Label";
            if (TypeHelper.IsNullable(propertyType)) { this._cb.IsEditable = true; }

            this._cb.SelectionChanged += On_cb_SelectionChanged;
            this._cb.DropDownOpened += On_cb_DropDownOpened;

            this.BindElementReadOnly(this._cb, ComboBox.IsEnabledProperty, false);

            this._cb.DataContextChanged += (o, e) =>
            {
                if (this.Context.CurrentObject != null)
                {
                    var value = this.PropertyValue;
                    if (value == null)
                    {
                        this._cb.SelectedIndex = -1;
                    }
                    else
                    {
                        this._cb.SelectedIndex =
                            this._cb.Items.IndexOf(new EnumViewModel(value as Enum));
                    }
                }
            };

            this.SetAutomationElement(this._cb);

            return this._cb;
        }

        protected override DependencyProperty BindingProperty()
        {
            return ComboBox.TextProperty;
        }

        private void On_cb_DropDownOpened(object sender, EventArgs e)
        {
            if (this.IsReadonly) { this._cb.IsDropDownOpen = false; }
        }

        private void On_cb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as EnumViewModel;
                if (item != null)
                {
                    this.PropertyValue = item.EnumValue;
                }
            }
            else
            {
                this.PropertyValue = null;
            }

            e.Handled = true;
        }

        //public class EnumConverter : IValueConverter
        //{
        //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        //    {
        //        if (value == null) { return string.Empty; }

        //        return new EnumViewModel((Enum)value).Label;
        //    }

        //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        //    {
        //        var strValue = value.ToString();

        //        if (string.IsNullOrEmpty(strValue)) return null;

        //        return EnumViewModel.LabelToEnum(strValue, targetType);
        //    }
        //}

        //protected override Binding CreateBinding()
        //{
        //    var textBinding = base.CreateBinding();
        //    textBinding.Converter = new EnumConverter();
        //    return textBinding;
        //}
    }
}