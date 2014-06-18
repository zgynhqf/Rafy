/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110525
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110525
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Linq.Expressions;
using Rafy.Domain;
using Rafy.Utils;
using System.Windows;
using System.Windows.Automation;
using Rafy.ManagedProperty;

namespace Rafy.WPF.Automation
{
    /// <summary>
    /// 自动化帮助类
    /// </summary>
    public static class AutomationHelper
    {
        #region 表格行 - 自动化属性绑定逻辑

        /// <summary>
        /// 查找逻辑：
        /// 先找元数据中的 AutomationPropertyKey
        /// 如果没有，再尝试使用 Title 属性。
        /// 如果没有，再尝试使用 "Name" 属性。
        /// </summary>
        /// <param name="evm"></param>
        /// <returns></returns>
        public static string TryGetPrimayDisplayProperty(this EntityViewMeta evm)
        {
            var autoProperty = TryGetAutomationNameBinding(evm);
            if (autoProperty != null) { return autoProperty; }

            var title = evm.TitleProperty;
            if (title != null) { return title.Name; }

            var nameProperty = evm.Property("Name");
            if (nameProperty != null) { return nameProperty.Name; }

            return null;
        }

        #endregion

        #region EntityViewMeta扩展属性：自动化测试的查找属性

        /// <summary>
        /// EntityViewMeta 的扩展属性：自动化的查找属性。
        /// 
        /// 自动化框架会使用这个属性值来进行控件查找。
        /// </summary>
        public const string AutomationPropertyKey = "AutomationHelper_AutomationPropertyKey";

        /// <summary>
        /// 设置自动化测试中的需要绑定的“查找属性”名。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="meta"></param>
        /// <param name="propertyExp"></param>
        public static void SetAutomationNameBinding(
            this EntityViewMeta meta, IManagedProperty property
            )
        {
            meta.SetExtendedProperty(AutomationPropertyKey, property.Name);
        }

        /// <summary>
        /// 表格中某一行在自动化测试中应该以哪个属性来作为“查找属性”。
        /// </summary>
        /// <param name="evm"></param>
        /// <returns></returns>
        public static string TryGetAutomationNameBinding(this EntityViewMeta evm)
        {
            var autoProperty = evm.GetPropertyOrDefault<string>(AutomationPropertyKey);
            if (!string.IsNullOrWhiteSpace(autoProperty)) { return autoProperty; }

            return null;
        }

        #endregion

        /// <summary>
        /// 最核心的编辑控件，需要调用此方法以支持自动化查找。
        /// </summary>
        /// <param name="editingElement"></param>
        public static void SetEditingElement(FrameworkElement editingElement)
        {
            AutomationProperties.SetName(editingElement, "编辑控件");
        }

        /// <summary>
        /// 最核心的编辑控件，需要调用此方法以支持自动化查找。
        /// </summary>
        /// <param name="editingElement"></param>
        public static void SetEditingElement(FrameworkElementFactory editingElement)
        {
            editingElement.SetValue(AutomationProperties.NameProperty, "编辑控件");
        }
    }
}
