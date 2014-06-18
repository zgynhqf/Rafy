/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111206
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 使用到的 GridView 内部私有的类成员
    /// </summary>
    internal static class MSInternal
    {
        #region LogicalTreeHelper.RemoveLogicalChild(DependencyObject parent, object child)

        private static readonly MethodInfo RemoveLogicalChildMethod = typeof(LogicalTreeHelper)
            .GetMethod("RemoveLogicalChild", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(DependencyObject), typeof(object) }, null);

        internal static void RemoveLogicalChild(DependencyObject parent, object child)
        {
            RemoveLogicalChildMethod.Invoke(null, new object[] { parent, child });
        }

        #endregion

        #region UIElement.RemoveNoVerify(UIElement element)

        private static readonly MethodInfo RemoveNoVerifyMethod = typeof(UIElementCollection)
            .GetMethod("RemoveNoVerify", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void RemoveNoVerify(UIElementCollection @this, UIElement element)
        {
            RemoveNoVerifyMethod.Invoke(@this, new object[] { element });
        }

        #endregion

        #region DependencyObject.ProvideSelfAsInheritanceContext(DependencyObject doValue, DependencyProperty dp)

        private static readonly MethodInfo ProvideSelfAsInheritanceContextMethod = typeof(DependencyObject)
            .GetMethod("ProvideSelfAsInheritanceContext", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DependencyObject), typeof(DependencyProperty) }, null);

        internal static bool ProvideSelfAsInheritanceContext(DependencyObject @this, DependencyObject doValue, DependencyProperty dp)
        {
            return (bool)ProvideSelfAsInheritanceContextMethod.Invoke(@this, new object[] { doValue, dp });
        }

        #endregion

        #region DependencyObject.RemoveSelfAsInheritanceContext(DependencyObject doValue, DependencyProperty dp)

        private static readonly MethodInfo RemoveSelfAsInheritanceContextMethod = typeof(DependencyObject)
            .GetMethod("RemoveSelfAsInheritanceContext", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(DependencyObject), typeof(DependencyProperty) }, null);

        internal static bool RemoveSelfAsInheritanceContext(DependencyObject @this, DependencyObject doValue, DependencyProperty dp)
        {
            return (bool)RemoveSelfAsInheritanceContextMethod.Invoke(@this, new object[] { doValue, dp });
        }

        #endregion

        #region DependencyObject.InheritanceContext

        private static readonly PropertyInfo InheritanceContextProperty = typeof(DependencyObject)
            .GetProperty("InheritanceContext", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static DependencyObject GetInheritanceContext(DependencyObject @this)
        {
            return InheritanceContextProperty.GetValue(@this, null) as DependencyObject;
        }

        #endregion

        #region ItemsControl.ScrollHost

        private static readonly PropertyInfo ScrollHostProperty = typeof(ItemsControl)
            .GetProperty("ScrollHost", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static ScrollViewer GetScrollHost(ItemsControl @this)
        {
            return ScrollHostProperty.GetValue(@this, null) as ScrollViewer;
        }

        #endregion

        #region ItemsControl._itemsHost

        private static readonly FieldInfo ItemsHostField = typeof(ItemsControl).GetField("_itemsHost", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static Panel GetItemsHost(ItemsControl @this)
        {
            return ItemsHostField.GetValue(@this) as Panel;
        }

        #endregion

        #region VirtualizingStackPanel.IgnoreMaxDesiredSize

        private static readonly PropertyInfo IgnoreMaxDesiredSizeProperty = typeof(VirtualizingStackPanel)
            .GetProperty("IgnoreMaxDesiredSize", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static void SetIgnoreMaxDesiredSize(VirtualizingStackPanel @this, bool value)
        {
            IgnoreMaxDesiredSizeProperty.SetValue(@this, value, null);
        }

        #endregion
    }
}
