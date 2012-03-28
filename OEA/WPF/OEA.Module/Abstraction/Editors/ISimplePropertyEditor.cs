using System;
using System.Collections.Generic;
using System.ComponentModel;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA
{
    /// <summary>
    /// 属性编辑器
    /// </summary>
    public interface ISimplePropertyEditor
    {
        /// <summary>
        /// 所处的视图
        /// </summary>
        IPropertyEditorContext Context { get; }

        /// <summary>
        /// 被编辑的属性（可以动态更改）
        /// </summary>
        EntityPropertyViewMeta PropertyViewInfo { get; }

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