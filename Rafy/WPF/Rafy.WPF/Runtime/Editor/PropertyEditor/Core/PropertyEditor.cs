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
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Automation;
using Rafy.WPF.Controls;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// WPF下的属性编辑器
    /// 
    /// <remarks>
    /// PropertyEditor 与 EditingElement 的关系：
    /// 
    /// 一个 PropertyEditor 应该对应一个 EditingElement，这是因为属性编辑器的概念就只是一个属性的编辑器，同时也是封装的控件的控制器；
    /// 如果一个 PropertyEditor 可以生成多个编辑控件的话，那它的角色就不再是一个控制器，而变成了一个工厂。
    /// 这样，在表格中，一列中的不同行需要不同的 EditingElement 时，应该生成不同的 PropertyEditor。
    /// </remarks>
    /// </summary>
    public abstract class PropertyEditor : IPropertyEditor
    {
        private WPFEntityPropertyViewMeta _meta;

        /// <summary>
        /// 被编辑的属性
        /// </summary>
        public WPFEntityPropertyViewMeta Meta
        {
            get
            {
                Debug.Assert(this._meta != null, "在使用Editor之前必须给PropertyInfo赋值");
                return this._meta;
            }
            protected set { this._meta = value; }
        }

        internal protected virtual void Initialize(WPFEntityPropertyViewMeta propertyInfo)
        {
            this.Meta = propertyInfo;
        }

        #region PropertyValue

        private bool _raisePropertyChangeEvents = true;

        /// <summary>
        /// 是否发生 PropertyValueChanging 及 PropertyValueChanged 事件。
        /// </summary>
        protected bool RaisePropertyChangeEvents
        {
            get { return this._raisePropertyChangeEvents; }
            set { this._raisePropertyChangeEvents = value; }
        }

        /// <summary>
        /// 被编辑的属性的当前值
        /// "View.CurrentObject.(PropertyInfo.Name)"
        /// </summary>
        public object PropertyValue
        {
            get
            {
                var currentObject = this.Context.CurrentObject;
                if (currentObject != null)
                {
                    var mp = this._meta.PropertyMeta.ManagedProperty;

                    var rp = mp as IRefProperty;
                    if (rp != null) { return currentObject.GetRefNullableId(rp.RefIdProperty); }

                    return currentObject.GetProperty(mp);
                }

                return null;
            }
            set
            {
                var currentObject = this.Context.CurrentObject;
                //已经选中了，再设置值。
                if (currentObject != null)
                {
                    var mp = this._meta.PropertyMeta.ManagedProperty;
                    if (!mp.IsReadOnly)
                    {
                        this.OnPropertyValueChanging();

                        var rp = mp as IRefProperty;
                        if (rp != null)
                        {
                            currentObject.SetRefNullableId(rp.RefIdProperty, value);
                        }
                        else
                        {
                            currentObject.SetProperty(mp, value);
                        }

                        this.OnPropertyValueChanged();
                    }
                }
            }
        }

        public event EventHandler PropertyValueChanging;

        public event EventHandler PropertyValueChanged;

        protected virtual void OnPropertyValueChanging()
        {
            if (this._raisePropertyChangeEvents)
            {
                var handler = this.PropertyValueChanging;
                if (handler != null) { handler(this, EventArgs.Empty); }
            }
        }

        protected virtual void OnPropertyValueChanged()
        {
            if (this._raisePropertyChangeEvents)
            {
                var handler = this.PropertyValueChanged;
                if (handler != null) { handler(this, EventArgs.Empty); }
            }
        }

        #endregion

        internal InnerContext _context = new InnerContext();

        public IPropertyEditorContext Context
        {
            get { return this._context; }
        }

        /// <summary>
        /// 属性编辑器是否可见
        /// </summary>
        public bool IsVisible
        {
            get
            {
                var visibilityIndicator = this.Meta.VisibilityIndicator;
                if (visibilityIndicator.IsDynamic)
                {
                    var curObj = this.Context.CurrentObject;
                    if (curObj != null)
                    {
                        return (bool)curObj.GetProperty(visibilityIndicator.Property);
                    }
                }

                return visibilityIndicator.VisiblityType == VisiblityType.AlwaysShow;
            }
        }

        #region IsReadOnly

        private List<ReadOnlyMatrix> _readonlyElements = new List<ReadOnlyMatrix>();

        /// <summary>
        /// 控制已经生成好的编辑控件的是否只读。
        /// 
        /// true  表示属性为只读状态。
        /// false 表示属性均为可编辑状态。
        /// null  则表示不强制只读，各属性按照自己相应的规则来计算自己的只读性。
        /// 
        /// 注意，这里是对生成好的编辑控件的只读性进行设置，不能控制是否生成编辑控件。
        /// 
        /// 场景：
        /// 在树型表格控件中，已经对 IsReadonly 进行了控制，如果只读就不生成编辑控件。
        /// 这里的 IsReadonly 主要是用于控制在详细编辑面板中的属性编辑器只读状态。
        /// </summary>
        public ReadOnlyStatus IsReadOnly
        {
            get
            {
                if (this._readonlyElements.Count > 0)
                {
                    return this._readonlyElements[0].IsReadOnly;
                }

                return ReadOnlyStatus.Dynamic;
            }
            set
            {
                if (this._readonlyElements.Count == 0) throw new InvalidOperationException("没有任何元素支持此操作。子类请先调用 BindElementReadOnly 方法。");

                foreach (var item in this._readonlyElements) { item.IsReadOnly = value; }
            }
        }

        /// <summary>
        /// 在动态只读的情况下，获取当前的只读性。
        /// </summary>
        /// <returns></returns>
        public bool GetCurrentReadOnly()
        {
            var isReadOnly = this.IsReadOnly;
            if (isReadOnly != ReadOnlyStatus.Dynamic)
            {
                return isReadOnly == ReadOnlyStatus.ReadOnly;
            }

            if (this._readonlyElements.Count > 0)
            {
                return this._readonlyElements[0].GetRuntimeReadOnly();
            }

            return false;
        }

        /// <summary>
        /// 绑定某个 UI 元素的 IsEnabled 属性作为只读的实现。
        /// </summary>
        /// <param name="element"></param>
        protected void AddReadOnlyComponent(FrameworkElement element)
        {
            this.AddReadOnlyComponent(element, UIElement.IsEnabledProperty, false);
        }

        /// <summary>
        /// 绑定某个 UI 元素的某个指定的属性作为只读的实现。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="readOnlyProperty"></param>
        protected void AddReadOnlyComponent(FrameworkElement element, DependencyProperty readOnlyProperty, bool readonlyValue = true)
        {
            var readonlyMatrix = new ReadOnlyMatrix
            {
                Element = element,
                ReadOnlyProperty = readOnlyProperty,
                Meta = this.Meta,
                ReadOnlyValue = readonlyValue
            };
            this._readonlyElements.Add(readonlyMatrix);

            readonlyMatrix.SetInitVallue();
        }

        #endregion

        #region CreateControl

        private FrameworkElement _control;

        private FrameworkElement _labelControl;

        /// <summary>
        /// 当前使用的control
        /// </summary>
        public FrameworkElement Control
        {
            get
            {
                if (this._control == null) throw new InvalidOperationException("请先调用EnsureControlsCreated方法！");
                return _control;
            }
        }

        /// <summary>
        /// 当前的LabelControl
        /// </summary>
        public FrameworkElement LabelControl
        {
            get
            {
                if (this._labelControl == null) throw new InvalidOperationException("请先调用EnsureControlsCreated方法！");
                return _labelControl;
            }
        }

        /// <summary>
        /// 确保已经生成所有需要使用的控件
        /// </summary>
        /// <returns></returns>
        internal void EnsureControlsCreated()
        {
            if (this._control == null || this._labelControl == null)
            {
                //var view = this.Context;
                if (this._meta == null) throw new InvalidOperationException("view != null && this._propertyInfo != null");

                //放在CreateControlCore后,因为CreateLabelControl可能会用刚才生成的Control
                this._control = this.CreateEditingControl();
                this._labelControl = this.CreateLabelElement();

                this.OnControlsCreated();

                this._context.control = this.Control;
            }
        }

        /// <summary>
        /// 控件生成完成事件。
        /// </summary>
        public event EventHandler ControlsCreated;

        /// <summary>
        /// 控件创建完成后的事件
        /// </summary>
        protected virtual void OnControlsCreated()
        {
            var handler = this.ControlsCreated;
            if (handler != null) handler(this, EventArgs.Empty);
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
                Content = this.Meta.Label.Translate(),
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

        private FrameworkElement CreateEditingControl()
        {
            var element = this.CreateEditingElement();

            if (this._readonlyElements.Count == 0)
            {
                this.AddReadOnlyComponent(element);
            }

            return element;
        }

        #endregion

        #region Binding

        /// <summary>
        /// 当前的属性是否声明了 set 存储器。
        /// </summary>
        public bool PropertyCanWrite
        {
            get { return this.Meta.PropertyMeta.Runtime.CanWrite; }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal Binding CreateBindingInternal()
        {
            return this.CreateBinding();
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DependencyProperty GetBindingProperty()
        {
            return this.BindingProperty();
        }

        /// <summary>
        /// 重置指定编辑控件的绑定。
        /// </summary>
        /// <param name="editingElement"></param>
        protected virtual void ResetBinding(FrameworkElement editingElement)
        {
            var property = this.GetBindingProperty();
            var binding = this.CreateBinding();
            editingElement.SetBinding(property, binding);
        }

        /// <summary>
        /// 返回本编辑器中控件所需要使用绑定的控件属性。
        /// </summary>
        /// <returns></returns>
        protected abstract DependencyProperty BindingProperty();

        /// <summary>
        /// 创建本编辑器需要使用的绑定。
        /// </summary>
        /// <returns></returns>
        protected virtual Binding CreateBinding()
        {
            var bindingMode = this.PropertyCanWrite ? BindingMode.TwoWay : BindingMode.OneWay;

            var binding = this.CreateBinding(bindingMode);

            return binding;
        }

        internal Binding CreateBinding(BindingMode bindingMode)
        {
            var meta = this.Meta;

            var mp = meta.PropertyMeta.ManagedProperty;

            //如果是引用实体属性，则使用对应的引用 Id 属性作为验证的参数。
            if (mp is IRefEntityProperty) { mp = (mp as IRefEntityProperty).RefIdProperty; }

            var binding = new Binding()
            {
                Mode = bindingMode,
                Path = new PropertyPath(meta.DisplayPath(), mp),
                ValidationRules = { ManagedProeprtyValidationRule.Instance },
            };
            //binding.ValidatesOnDataErrors = true;
            //binding.ValidationRules.Add(new DataErrorValidationRule());

            return binding;
        }

        #endregion

        #region IWPFPropertyEditor Members

        /// <summary>
        /// 把生成的某个编辑控件准备好，马上开始编辑。
        /// </summary>
        /// <param name="editingElement"></param>
        /// <param name="editingEventArgs"></param>
        internal void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            this.PrepareElementForEditCore(editingElement, editingEventArgs);
        }

        #endregion

        #region private class InnerContext : IPropertyEditorContext

        internal class InnerContext : IPropertyEditorContext
        {
            public FrameworkElement control;

            /// <summary>
            /// 当前“激活/显示/选中”的对象
            /// </summary>
            public Entity CurrentObject
            {
                get
                {
                    if (this.control == null) { return null; }

                    if (this.control.CheckAccess())
                    {
                        return this.control.DataContext as Entity;
                    }

                    Func<Entity> action = () => this.control.DataContext as Entity;
                    return this.control.Dispatcher.Invoke(action) as Entity;
                }
            }

            /// <summary>
            /// 是否显示在详细视图中
            /// </summary>
            public bool IsForList { get; internal set; }
        }

        #endregion
    }
}