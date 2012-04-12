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
using OEA.MetaModel.View;
using System.Windows.Automation;
using OEA.ManagedProperty;
using OEA.Module.WPF.Editors;
using System.ComponentModel;

namespace OEA.Module.WPF.Controls
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
        }

        #region EntityProperty DependencyProperty

        public static readonly DependencyProperty EntityPropertyProperty = DependencyProperty.Register(
            "EntityProperty", typeof(object), typeof(EditorHost),
            new PropertyMetadata((d, e) => (d as EditorHost).OnEntityPropertyChanged(e))
            );

        /// <summary>
        /// 本属性可以设置为以下类型：ManagedProperty、EntityPropertyViewMeta、String(PropertyName)
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

        #region LabelWidth DependencyProperty

        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(
            "LabelWidth", typeof(GridLength), typeof(EditorHost)
            );

        public GridLength LabelWidth
        {
            get { return (GridLength)this.GetValue(LabelWidthProperty); }
            set { this.SetValue(LabelWidthProperty, value); }
        }

        #endregion

        #region DetailObjectView DependencyProperty

        internal static readonly DependencyProperty DetailObjectViewProperty = DependencyProperty.RegisterAttached(
            "DetailObjectView", typeof(DetailObjectView), typeof(EditorHost)
            );

        internal static DetailObjectView GetDetailObjectView(FrameworkElement element)
        {
            //从当前元素往上找，一直到找到为止。
            //如果到达逻辑树根，还没有找到，则返回 null。
            var value = (DetailObjectView)element.GetValue(DetailObjectViewProperty);

            if (value == null)
            {
                var p = element.Parent as FrameworkElement;
                if (p != null) { return GetDetailObjectView(p); }
            }

            return value;
        }

        internal static void SetDetailObjectView(FrameworkElement element, DetailObjectView value)
        {
            element.SetValue(DetailObjectViewProperty, value);
        }

        #endregion

        /// <summary>
        /// 本控件对应的运行时属性编辑器
        /// </summary>
        public IPropertyEditor PropertyEditor { get; private set; }

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

            var detailView = GetDetailObjectView(this);
            if (detailView == null) { return; }

            var property = this.GetPropertyViewMeta(detailView);
            if (property == null) { return; }

            //由于这个控件可以被所有属性使用，所以这里还需要对是否在 Detail 中显示进行判断。
            if (!property.CanShowIn(ShowInWhere.Detail))
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            var editor = AutoUI.BlockUIFactory.PropertyEditorFactory.Create(property);

            var labelContainer = this.Template.FindName("PART_LabelContainer", this) as ContentControl;
            var editorControlContainer = this.Template.FindName("PART_EditorControlContainer", this) as ContentControl;
            labelContainer.Content = editor.LabelControl;
            editorControlContainer.Content = editor.Control;

            this.PropertyEditor = editor;

            //处理需要根据实体状态变化的：动态属性编辑器
            var dynamicPE = editor as EntityDynamicPropertyEditor;
            if (dynamicPE != null)
            {
                detailView.CurrentObjectChanged += (o, e) =>
                {
                    editor.PrepareElementForEdit(dynamicPE.Control, new RoutedEventArgs());
                };
            }

            var view = detailView as QueryObjectView;
            if (view != null) { view.AddPropertyEditor(editor); }

            //支持UI Test
            var label = property.Label;
            if (label != null) { AutomationProperties.SetName(editor.Control, property.Label); }

            //LabelWidth
            var labelWidth = detailView.Meta.DetailLabelWidth;
            if (labelWidth != null) { this.LabelWidth = new GridLength(labelWidth.Value); }
        }

        private EntityPropertyViewMeta GetPropertyViewMeta(DetailObjectView detailView)
        {
            var content = this.Content;

            var res = content as EntityPropertyViewMeta;
            if (res != null) return res;

            var entityPropertyName = content as string;
            if (entityPropertyName != null) { return detailView.Meta.Property(entityPropertyName); }

            var mp = content as IManagedProperty;
            if (mp != null) { return detailView.Meta.Property(mp); }

            return null;
        }
    }
}