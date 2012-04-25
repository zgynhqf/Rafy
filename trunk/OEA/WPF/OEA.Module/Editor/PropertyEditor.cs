using System;
using System.Diagnostics;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Editors
{
    /// <summary>
    /// 属性编辑器
    /// 
    /// 所有的属性编辑器的基类
    /// </summary>
    public abstract class PropertyEditor : IPropertyEditor
    {
        private EntityPropertyViewMeta _propertyInfo;

        /// <summary>
        /// 所处的视图
        /// </summary>
        public abstract IPropertyEditorContext Context { get; }

        /// <summary>
        /// 被编辑的属性
        /// </summary>
        public EntityPropertyViewMeta Meta
        {
            get
            {
                Debug.Assert(this._propertyInfo != null, "在使用Editor之前必须给PropertyInfo赋值");
                return this._propertyInfo;
            }
            protected set { this._propertyInfo = value; }
        }

        #region PropertyValue

        /// <summary>
        /// 被编辑的属性的当前值
        /// "View.CurrentObject.(PropertyInfo.Name)"
        /// </summary>
        public object PropertyValue
        {
            get
            {
                var currentObject = this.Context.CurrentObject;
                if (currentObject != null) { return currentObject.GetPropertyValue(this._propertyInfo.Name); }

                return null;
            }
            set
            {
                var currentObject = this.Context.CurrentObject;
                //已经选中了，再设置值。
                if (currentObject != null)
                {
                    //if ((PropertyValue != value) && (null != PropertyChanged))
                    //    PropertyChanged(this, new PropertyChangedEventArgs(_propertyInfo.Name));
                    this.OnPropertyValueChanging();
                    currentObject.SetPropertyValue(this._propertyInfo.Name, value);
                    this.OnPropertyValueChanged();
                }
            }
        }

        public event EventHandler PropertyValueChanging;

        public event EventHandler PropertyValueChanged;

        protected virtual void OnPropertyValueChanging()
        {
            var handler = this.PropertyValueChanging;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        protected virtual void OnPropertyValueChanged()
        {
            var handler = this.PropertyValueChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region IsVisible, IsReadOnly

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

        /// <summary>
        /// 这个属性是否是只读的
        /// </summary>
        public abstract bool IsReadonly { get; set; }

        #endregion

        #region CreateControl

        private object _control;

        private object _labelControl;

        /// <summary>
        /// 当前使用的control
        /// </summary>
        public object Control
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
        public object LabelControl
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
        public void EnsureControlsCreated()
        {
            if (this._control == null || this._labelControl == null)
            {
                //var view = this.Context;
                if (this._propertyInfo == null) throw new InvalidOperationException("view != null && this._propertyInfo != null");

                //放在CreateControlCore后,因为CreateLabelControl可能会用刚才生成的Control
                this._control = this.CreateEditingControl();
                this._labelControl = this.CreateLabelControl();

                this.OnControlsCreated();
            }
        }

        /// <summary>
        /// 创建一个新的 control
        /// </summary>
        /// <returns></returns>
        protected abstract object CreateEditingControl();

        /// <summary>
        /// 返回一个 StackPanel，
        /// 里面是一个 Label 和一个 PropertyStatus
        /// </summary>
        /// <returns></returns>
        protected abstract object CreateLabelControl();

        public event EventHandler ControlsCreated;

        /// <summary>
        /// 控件创建完成后的事件
        /// </summary>
        protected virtual void OnControlsCreated()
        {
            var handler = this.ControlsCreated;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion
    }
}