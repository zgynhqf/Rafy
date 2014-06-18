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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// Boolean 值对应的表格列
    /// </summary>
    public class BooleanTreeColumn : TreeColumn
    {
        protected BooleanTreeColumn() { }

        #region DisplayCheckBoxStyle DependencyProperty

        public static readonly DependencyProperty DisplayCheckBoxStyleProperty = DependencyProperty.Register(
            "DisplayCheckBoxStyle", typeof(Style), typeof(BooleanTreeColumn)
            );

        public Style DisplayCheckBoxStyle
        {
            get { return (Style)this.GetValue(DisplayCheckBoxStyleProperty); }
            set { this.SetValue(DisplayCheckBoxStyleProperty, value); }
        }

        #endregion

        /// <summary>
        /// BooleanTreeColumn 用于显示的元素也直接是一个 CheckBox
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        protected override FrameworkElementFactory GenerateDefaultDisplayTemplate(BindingBase binding)
        {
            var cb = new FrameworkElementFactory(typeof(CheckBox));

            cb.SetBinding(CheckBox.IsCheckedProperty, binding);

            var style = this.DisplayCheckBoxStyle;
            if (style != null) cb.SetValue(CheckBox.StyleProperty, style);

            //把 TreeGrid.IsReadOnly 属性反向绑定到 IsEanbled 属性上。
            //这是由于这个列直接使用显示的控件作为编辑的控件，所以这个显示控件的只读性，需要与 TreeGrid 的只读性保持同步。
            cb.SetBinding(CheckBox.IsEnabledProperty, new Binding
            {
                Path = new PropertyPath(TreeGrid.IsReadOnlyProperty),
                Converter = new ReverseBooleanConverter(),
                RelativeSource = new RelativeSource
                {
                    Mode = RelativeSourceMode.FindAncestor,
                    AncestorType = typeof(TreeGrid)
                }
            });

            return cb;
        }

        protected override bool CanEnterEditing(object dataItem)
        {
            //CheckBox 列不需要再进入编辑状态了，因为默认状态已经可以编辑。
            //另外，如果这里不进行重写，而让它可以重新生成编辑控件时，ViewConfigurationCommand.IsVisible 属性在编辑时会出现问题，原因不明！！
            return false;
        }
    }
}