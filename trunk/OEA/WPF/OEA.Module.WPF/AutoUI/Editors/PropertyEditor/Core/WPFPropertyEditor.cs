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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using OEA.Editors;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Library;
using System.Windows.Automation;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Automation;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// WPF下的属性编辑器
    /// 
    /// 建立LabelControl、焦点设置ReadOnly属性
    /// 
    /// 生成的控件，自动使用 DataContext 来作为当前对象。
    /// </summary>
    public abstract class WPFPropertyEditor : PropertyEditor, IWPFPropertyEditor
    {
        private InnerContext _context = new InnerContext();

        internal protected virtual void Initialize(EntityPropertyViewMeta propertyInfo)
        {
            this.Meta = propertyInfo;
        }

        public override IPropertyEditorContext Context
        {
            get { return this._context; }
        }

        /// <summary>
        /// 子类在这个方法里面生成一个用于Edit的WPF Control
        /// 
        /// 注意，此方法中可能需要：
        /// 1.调用 SetAutomationControl 方法：
        ///     由于返回的控件可能是一个大的窗口，所以应该在最终生成的控件上调用 SetAutomationControl 方法。
        /// 2.调用 BindElementReadOnly 方法：
        ///     调用此方法以支持只读属性的绑定。
        /// </summary>
        /// <returns></returns>
        protected abstract FrameworkElement CreateEditingElement();

        protected virtual void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs) { }

        #region Binding

        /// <summary>
        /// 当前的属性是否声明了 set 存储器。
        /// </summary>
        public bool PropertyCanWrite
        {
            get { return this.Meta.PropertyMeta.Runtime.CanWrite; }
        }

        protected abstract void ResetBinding(FrameworkElement editingControl);

        internal Binding CreateBindingInternal()
        {
            return this.CreateBinding();
        }

        protected virtual Binding CreateBinding()
        {
            var binding = new Binding(this.Meta.Name);

            if (!this.PropertyCanWrite)
            {
                binding.Mode = BindingMode.OneWay;
            }
            else
            {
                binding.ValidatesOnDataErrors = true;
                //binding.ValidationRules.Add(new DataErrorValidationRule());
            }

            return binding;
        }

        public void RebindEditingControl()
        {
            this.ResetBinding(this.Control);
        }

        #endregion

        /// <summary>
        /// CreateEditingElement 方法中对最核心的编辑控件，需要调用此方法以支持自动化查找。
        /// </summary>
        /// <param name="editingElement"></param>
        protected void SetAutomationElement(FrameworkElement editingElement)
        {
            AutomationHelper.SetEditingElement(editingElement);
        }

        /// <summary>
        /// 返回一个StackPanel，
        /// 里面是一个Label和一个PropertyStatus
        /// </summary>
        /// <returns></returns>
        protected virtual FrameworkElement CreateLabelElement()
        {
            var panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal
            };

            //Label，
            var label = new Label()
            {
                Content = this.Meta.Label,
                Target = Control
            };
            panel.Children.Add(label);

            ////PropertyStatus，绑定状态到对象属性
            //var ps = new PropertyStatus()
            //{
            //    Property = this.PropertyViewInfo.Name,
            //    Target = Control
            //};
            //ps.SetBinding(PropertyStatus.SourceProperty, new Binding());
            //panel.Children.Add(ps);

            return panel;
        }

        protected override void OnControlsCreated()
        {
            base.OnControlsCreated();

            this._context.control = this.Control;

            this.BindVisibility();
        }

        /// <summary>
        /// 在所有控件生成后，为他们的可见性进行属性绑定
        /// </summary>
        private void BindVisibility()
        {
            var visibilityIndicator = this.Meta.VisibilityIndicator;
            if (visibilityIndicator.IsDynamic)
            {
                Binding visibleBinding = new Binding(visibilityIndicator.PropertyName);
                visibleBinding.Mode = BindingMode.OneWay;
                visibleBinding.Converter = new BooleanToVisibilityConverter();

                this.Control.SetBinding(UIElement.VisibilityProperty, visibleBinding);
                this.LabelControl.SetBinding(UIElement.VisibilityProperty, visibleBinding);
            }
        }

        #region IsReadOnly

        /// <summary>
        /// 控制已经生成好的编辑控件的是否只读。
        /// 
        /// 注意，这里是对生成好的编辑控件的只读性进行设置，不能控制是否生成编辑控件。
        /// 
        /// 场景：
        /// 在树型表格控件中，已经对 IsReadonly 进行了控制，如果只读就不生成编辑控件。
        /// 这里的 IsReadonly 主要是用于控制在详细编辑面板中的属性编辑器只读状态。
        /// </summary>
        public override bool IsReadonly
        {
            get
            {
                if (this._readonlyElements.Count > 0)
                {
                    var first = this._readonlyElements.First();
                    return (bool)first.Element.GetValue(first.Property) == first.ReadonlyValue;
                }

                return false;
            }
            set
            {
                if (this._readonlyElements.Count == 0) throw new InvalidOperationException("没有任何元素支持此操作。子类请先调用 BindElementReadOnly 方法。");

                foreach (var item in this._readonlyElements)
                {
                    item.Element.SetValue(item.Property, item.ReadonlyValue);
                }
            }
        }

        private List<ReadonlyElement> _readonlyElements = new List<ReadonlyElement>();

        protected void BindElementReadOnly(FrameworkElement element)
        {
            this.BindElementReadOnly(element, UIElement.IsEnabledProperty, false);
        }

        /// <summary>
        /// 绑定某个框架元素的某个元素到 IsReadOnly 上。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="readOnlyProperty"></param>
        protected void BindElementReadOnly(FrameworkElement element, DependencyProperty readOnlyProperty, bool readonlyValue = true)
        {
            var readonlyMatrix = new ReadonlyElement
            {
                Element = element,
                Property = readOnlyProperty,
                ReadonlyValue = readonlyValue
            };
            this._readonlyElements.Add(readonlyMatrix);

            PropertyEditorHelper.BindElementReadOnly(readonlyMatrix, this.Meta);
        }

        #endregion

        #region 实现基类方法

        protected sealed override object CreateEditingControl()
        {
            var element = this.CreateEditingElement();

            if (this._readonlyElements.Count == 0)
            {
                this.BindElementReadOnly(element);
            }

            return element;
        }

        protected sealed override object CreateLabelControl()
        {
            return this.CreateLabelElement();
        }

        #endregion

        #region IWPFPropertyEditor Members

        public new FrameworkElement Control
        {
            get
            {
                return base.Control as FrameworkElement;
            }
        }

        public new FrameworkElement LabelControl
        {
            get
            {
                return base.LabelControl as FrameworkElement;
            }
        }

        public void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            this.PrepareElementForEditCore(editingElement, editingEventArgs);
        }

        #endregion

        #region private class InnerContext : IPropertyEditorContext

        private class InnerContext : IPropertyEditorContext
        {
            public FrameworkElement control;

            public Entity CurrentObject
            {
                get
                {
                    if (this.control == null) { return null; }

                    return this.control.DataContext as Entity;
                }
            }
        }

        #endregion
    }
}