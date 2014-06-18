/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120706 11:21
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120706 11:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rafy.MetaModel;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 表单控件
    /// </summary>
    public class Form : ContentControl
    {
        #region DetailView DependencyProperty

        public static readonly DependencyProperty DetailViewProperty = DependencyProperty.Register(
            "DetailView", typeof(DetailLogicalView), typeof(Form),
            new PropertyMetadata((d, e) => (d as Form).OnDetailViewChanged(e))
            );

        /// <summary>
        /// 表单控件对应的 DetailLogicalView
        /// </summary>
        public DetailLogicalView DetailView
        {
            get { return (DetailLogicalView)this.GetValue(DetailViewProperty); }
            set { this.SetValue(DetailViewProperty, value); }
        }

        private void OnDetailViewChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (DetailLogicalView)e.NewValue;
        }

        #endregion

        #region IsReadOnly DependencyProperty

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly", typeof(ReadOnlyStatus), typeof(Form),
            new PropertyMetadata(ReadOnlyStatus.Dynamic, (d, e) => (d as Form).OnIsReadOnlyChanged(e))
            );

        public ReadOnlyStatus IsReadOnly
        {
            get { return (ReadOnlyStatus)this.GetValue(IsReadOnlyProperty); }
            set { this.SetValue(IsReadOnlyProperty, value); }
        }

        #endregion

        #region WrppingEditorStyle DependencyProperty

        public static readonly DependencyProperty WrppingEditorStyleProperty = DependencyProperty.Register(
            "WrppingEditorStyle", typeof(Style), typeof(Form)
            );

        /// <summary>
        /// 本表单中所有在换行布局控件 WrapPanel 中的 EditorHost 的样式。
        /// </summary>
        public Style WrppingEditorStyle
        {
            get { return (Style)this.GetValue(WrppingEditorStyleProperty); }
            set { this.SetValue(WrppingEditorStyleProperty, value); }
        }

        #endregion

        #region AutoGridEditorStyle DependencyProperty

        public static readonly DependencyProperty AutoGridEditorStyleProperty = DependencyProperty.Register(
            "AutoGridEditorStyle", typeof(Style), typeof(Form)
            );

        /// <summary>
        /// 本表单中所有在自动表格布局控件 AutoGrid 中的 EditorHost 的样式。
        /// </summary>
        public Style AutoGridEditorStyle
        {
            get { return (Style)this.GetValue(AutoGridEditorStyleProperty); }
            set { this.SetValue(AutoGridEditorStyleProperty, value); }
        }

        #endregion

        internal List<EditorHost> Editors = new List<EditorHost>(10);

        private void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (ReadOnlyStatus)e.NewValue;
            foreach (var editor in this.Editors)
            {
                editor.PropertyEditor.IsReadOnly = value;
            }
        }

        /// <summary>
        /// 获取当前元素所有的表单。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Form GetForm(FrameworkElement element)
        {
            return element.GetLogicalParent<Form>();
        }
    }
}