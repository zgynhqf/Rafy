using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;

namespace OEA
{
    /// <summary>
    /// 属性编辑器
    /// </summary>
    public interface IPropertyEditor : IControlWrapper, ISimplePropertyEditor
    {
        /// <summary>
        /// 属性编辑器是否可见
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 这个属性是否是只读的
        /// </summary>
        bool IsReadonly { get; }

        /// <summary>
        /// 当前的LabelControl
        /// </summary>
        object LabelControl { get; }

        /// <summary>
        /// 生成所有要使用的所有控件
        /// </summary>
        /// <returns></returns>
        void EnsureControlsCreated();
    }
}
