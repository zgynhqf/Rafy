using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 属性编辑器
    /// </summary>
    public interface IPropertyEditor
    {
        /// <summary>
        /// 当前的LabelControl
        /// </summary>
        FrameworkElement LabelControl { get; }

        /// <summary>
        /// 当前的编辑控件
        /// </summary>
        FrameworkElement Control { get; }

        /// <summary>
        /// 所处的视图
        /// </summary>
        IPropertyEditorContext Context { get; }

        /// <summary>
        /// 被编辑的属性（可以动态更改）
        /// </summary>
        WPFEntityPropertyViewMeta Meta { get; }

        /// <summary>
        /// 属性编辑器是否可见
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 这个属性是否是只读的
        /// </summary>
        ReadOnlyStatus IsReadOnly { get; set; }

        /// <summary>
        /// 被编辑的属性的当前值
        /// "View.CurrentObject.(PropertyInfo.Name)"
        /// </summary>
        object PropertyValue { get; set; }

        /// <summary>
        /// 属性变化前发生此事件。
        /// </summary>
        event EventHandler PropertyValueChanging;

        /// <summary>
        /// 属性变化时发生此事件。
        /// </summary>
        event EventHandler PropertyValueChanged;
    }
}
