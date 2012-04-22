/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110616
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110616
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF.Editors;
using System.Windows;
using System.Windows.Controls;
using OEA.Library;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 一个可以根据当前实体的状态，动态生成控件的属性编辑器。
    /// </summary>
    public abstract class EntityDynamicPropertyEditor : WPFPropertyEditor
    {
        protected override sealed FrameworkElement CreateEditingElement()
        {
            //生成一个临时的代理控件，真正的生成在 PrepareElementForEditCore 中实现。
            return new ContentControl();
        }

        protected override sealed void ResetBinding(FrameworkElement editingControl) { }

        protected override DependencyProperty BindingProperty()
        {
            return null;
        }

        protected override sealed void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            var curEntity = this.Context.CurrentObject;
            if (curEntity == null) return;

            var dynamicEditingControl = this.CreateDynamicEditingElement(curEntity);

            var contentControl = editingElement as ContentControl;
            contentControl.Content = dynamicEditingControl;
            contentControl.UpdateLayout();

            this.PrepareDynamicElementForEdit(dynamicEditingControl, editingEventArgs);
        }

        /// <summary>
        /// 根据当前的实体对象创建一个动态控件。
        /// </summary>
        /// <param name="curEntity"></param>
        /// <returns></returns>
        protected abstract FrameworkElement CreateDynamicEditingElement(Entity curEntity);

        /// <summary>
        /// 准备动态控件进入编辑状态。
        /// </summary>
        /// <param name="dynamicElement"></param>
        /// <param name="editingEventArgs"></param>
        protected virtual void PrepareDynamicElementForEdit(FrameworkElement dynamicElement, RoutedEventArgs editingEventArgs)
        {
            this.ResetDynamicElementBinding(dynamicElement);
        }

        protected virtual void ResetDynamicElementBinding(FrameworkElement dynamicElement) { }
    }
}