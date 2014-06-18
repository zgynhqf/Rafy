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
using System.Windows.Controls;
using System.Windows;
using Rafy.MetaModel.View;
using System.Windows.Automation;
using Rafy.ManagedProperty;
using Rafy.WPF.Editors;
using System.ComponentModel;
using System.Windows.Data;
using Rafy.MetaModel;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// DetailPanel 中属性编辑器的容器控件
    /// 
    /// （继承自 ContentControl 是为了简单地支持设计时。EntityProperty 会同步到 Content 上。）
    /// </summary>
    [TemplatePart(Name = "PART_LabelContainer", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_EditorControlContainer", Type = typeof(ContentControl))]
    public class EditorHost : ContentControl
    {
        public EditorHost()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.HorizontalContentAlignment = HorizontalAlignment.Center;
            }

            this.ListenLayoutUpdatedOnce(this.OnLayoutUpdated);
        }

        #region EntityProperty DependencyProperty

        public static readonly DependencyProperty EntityPropertyProperty = DependencyProperty.Register(
            "EntityProperty", typeof(object), typeof(EditorHost),
            new PropertyMetadata((d, e) => (d as EditorHost).OnEntityPropertyChanged(e))
            );

        /// <summary>
        /// 本属性可以设置为以下类型：ManagedProperty、WPFEntityPropertyViewMeta、String(PropertyName)
        /// 
        /// 此属性与 Content 属性将保持一致，是为了简单地支持设计时。
        /// </summary>
        [TypeConverter(typeof(StringConverter))]
        public object EntityProperty
        {
            get { return (object)this.GetValue(EntityPropertyProperty); }
            set { this.SetValue(EntityPropertyProperty, value); }
        }

        private void OnEntityPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            this.Content = e.NewValue;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            this.EntityProperty = newContent;
        }

        #endregion

        #region Orientation DependencyProperty

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(EditorHost),
            new PropertyMetadata((d, e) => (d as EditorHost).OnOrientationChanged(e))
            );

        public Orientation Orientation
        {
            get { return (Orientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        private void OnOrientationChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (Orientation)e.NewValue;
        }

        #endregion

        #region ContentWidth DependencyProperty

        public static readonly DependencyProperty ContentWidthProperty = DependencyProperty.Register(
            "ContentWidth", typeof(GridLength), typeof(EditorHost),
            new PropertyMetadata(new GridLength(1000, GridUnitType.Star), (d, e) => (d as EditorHost).OnContentWidthChanged(e))
            );

        /// <summary>
        /// 本属性所占的格子宽度。
        /// 
        /// 默认是 1000*
        /// （它旁边有一列占有 1*。）
        /// </summary>
        public GridLength ContentWidth
        {
            get { return (GridLength)this.GetValue(ContentWidthProperty); }
            set { this.SetValue(ContentWidthProperty, value); }
        }

        private void OnContentWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (GridLength)e.NewValue;
        }

        #endregion

        #region LabelWidth DependencyProperty

        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            "LabelWidth", typeof(double), typeof(EditorHost),
            new PropertyMetadata(double.NaN, (d, e) => (d as EditorHost).OnLabelWidthChanged(e))
            );

        public double LabelWidth
        {
            get { return (double)this.GetValue(LabelWidthProperty); }
            set { this.SetValue(LabelWidthProperty, value); }
        }

        private void OnLabelWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (double)e.NewValue;
        }

        #endregion

        #region LabelHeight DependencyProperty

        public static readonly DependencyProperty LabelHeightProperty = DependencyProperty.Register(
            "LabelHeight", typeof(double), typeof(EditorHost),
            new PropertyMetadata(double.NaN, (d, e) => (d as EditorHost).OnLabelHeightChanged(e))
            );

        public double LabelHeight
        {
            get { return (double)this.GetValue(LabelHeightProperty); }
            set { this.SetValue(LabelHeightProperty, value); }
        }

        private void OnLabelHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (double)e.NewValue;
        }

        #endregion

        /// <summary>
        /// 本控件对应的运行时属性编辑器
        /// </summary>
        public IPropertyEditor PropertyEditor { get; private set; }

        #region 对应的 Form 对象

        private Form _form;

        /// <summary>
        /// 当前编辑器所在的表单控件
        /// </summary>
        public Form Form
        {
            get { return this._form; }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            //在变更父对象时，同步 Form 属性
            if (this._form != null)
            {
                this._form.Editors.Remove(this);
                this._form = null;
            }

            this.InitForm();
        }

        /// <summary>
        /// 设置新的 Form
        /// </summary>
        private void InitForm()
        {
            if (this._form == null)
            {
                this._form = Form.GetForm(this);
                if (this._form != null) { this._form.Editors.Add(this); }
            }
        }

        #endregion

        #region 根据父容器的类型 同步 Style

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            //根据父容器的类型 同步 Style
            var parent = this.GetContainerPanel();
            if (parent != null)
            {
                Style style = null;

                if (parent is AutoGrid) { style = this._form.AutoGridEditorStyle; }
                else if (parent is WrapPanel) { style = this._form.WrppingEditorStyle; }

                if (style != null) { this.SetIfNonLocal(StyleProperty, style); }
            }
        }

        private Panel GetContainerPanel()
        {
            for (var parent = LogicalTreeHelper.GetParent(this); parent != null && !(parent is Form); parent = LogicalTreeHelper.GetParent(parent))
            {
                if (parent is Panel) return parent as Panel;
            }

            return null;
        }

        #endregion

        #region 应用模板后实现控件生成

        public override void OnApplyTemplate()
        {
            /*********************** 代码块解释 *********************************
             * 
             * 在应用模板时：
             * 找到属性界面元数据；
             * 生成属性编辑器；
             * 把属性编辑器中的控件加入到本控件模板的相应位置；
             * 
            **********************************************************************/

            base.OnApplyTemplate();

            this.InitForm();
            if (this._form == null) { return; }

            var detailView = this._form.DetailView;
            if (detailView == null) { return; }

            var property = this.GetPropertyViewMeta(detailView);
            if (property == null) { return; }

            //由于这个控件可以被所有属性使用，所以这里还需要对是否在 Detail 中显示进行判断。
            if (!property.CanShowIn(ShowInWhere.Detail))
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.BindVisibility(property);

            #region 生成编辑器

            var editor = AutoUI.BlockUIFactory.PropertyEditorFactory.Create(property, false);

            //如果还没有给 Form 赋值，则不能给 editor 设置值。
            //否则 PropertyEditorFactory 在创建 editor 时为其设置的只读性会在此处被清除。
            var ro = this._form.IsReadOnly;
            if (ro != ReadOnlyStatus.Dynamic) { editor.IsReadOnly = ro; }

            this.PropertyEditor = editor;
            detailView.AddPropertyEditor(editor);

            #endregion

            #region 把编辑器中的两个控件放到模板控件中

            var labelContainer = this.Template.FindName("PART_LabelContainer", this) as ContentControl;
            var editorControlContainer = this.Template.FindName("PART_EditorControlContainer", this) as ContentControl;
            if (labelContainer != null) labelContainer.Content = editor.LabelControl;
            if (editorControlContainer != null) editorControlContainer.Content = editor.Control;

            #endregion

            this.SetLayoutValues(detailView, property);

            #region 处理需要根据实体状态变化的：动态属性编辑器

            var dynamicPE = editor as EntityDynamicPropertyEditor;
            if (dynamicPE != null)
            {
                detailView.CurrentChanged += (o, e) =>
                {
                    editor.PrepareElementForEdit(dynamicPE.Control, new RoutedEventArgs());
                };
            }

            #endregion

            #region 支持 UI Test

            var label = property.Label;
            if (label != null) { AutomationProperties.SetName(editor.Control, property.Label); }

            #endregion
        }

        /// <summary>
        /// 在所有控件生成后，为他们的可见性进行属性绑定
        /// </summary>
        private void BindVisibility(WPFEntityPropertyViewMeta meta)
        {
            var visibilityIndicator = meta.VisibilityIndicator;
            if (visibilityIndicator.IsDynamic)
            {
                Binding visibleBinding = new Binding(visibilityIndicator.Property.Name);
                visibleBinding.Mode = BindingMode.OneWay;
                visibleBinding.Converter = new BooleanToVisibilityConverter();

                this.SetBinding(UIElement.VisibilityProperty, visibleBinding);
            }
        }

        private void SetLayoutValues(DetailLogicalView detailView, WPFEntityPropertyViewMeta property)
        {
            if (property.DetailAsHorizontal != null)
            {
                var direction = property.DetailAsHorizontal.Value ? Orientation.Horizontal : Orientation.Vertical;
                this.SetIfNonLocal(OrientationProperty, direction);
            }

            if (property.DetailColumnsSpan != null)
            {
                this.SetIfNonLocal(Grid.ColumnSpanProperty, property.DetailColumnsSpan.Value);
            }

            var labelSize = property.DetailLabelSize ?? detailView.Meta.DetailLabelSize;
            if (labelSize != null)
            {
                if (this.Orientation == Orientation.Horizontal)
                {
                    this.SetIfNonLocal(LabelWidthProperty, labelSize.Value);
                }
                else
                {
                    this.SetIfNonLocal(LabelHeightProperty, labelSize.Value);
                }
            }

            if (property.DetailContentWidth != null)
            {
                if (!this.IsLocalValue(ContentWidthProperty))
                {
                    var value = property.DetailContentWidth.Value;

                    //0-1 之间表示相对值，否则表示绝对值。
                    if (value > 0 && value < 1)
                    {
                        value = value / (1 - value);
                        this.ContentWidth = new GridLength(value, GridUnitType.Star);
                    }
                    else
                    {
                        this.ContentWidth = new GridLength(value);
                    }
                }
            }

            if (property.DetailHeight != null)
            {
                this.SetIfNonLocal(HeightProperty, property.DetailHeight.Value);
            }
        }

        private WPFEntityPropertyViewMeta GetPropertyViewMeta(DetailLogicalView detailView)
        {
            var content = this.Content;

            var res = content as WPFEntityPropertyViewMeta;
            if (res != null) return res;

            var entityPropertyName = content as string;
            if (entityPropertyName != null) { return detailView.Meta.Property(entityPropertyName); }

            var mp = content as IManagedProperty;
            if (mp != null) { return detailView.Meta.Property(mp); }

            return null;
        }

        #endregion

        #region 其它

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == WidthProperty)
            {
                throw new InvalidOperationException("本对象不支持更改 Width 属性，请使用 ContentWidth 属性。");
            }

            base.OnPropertyChanged(e);
        }

        #endregion
    }
}