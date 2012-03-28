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

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 使用到的 GridView 内部私有的类成员
    /// </summary>
    internal static class GridViewInternal
    {
        internal static readonly FieldInfo _paddingHeaderField = typeof(GridViewHeaderRowPresenter).GetField("_paddingHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly FieldInfo _isHeaderDraggingField = typeof(GridViewHeaderRowPresenter).GetField("_isHeaderDragging", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly FieldInfo _indicatorField = typeof(GridViewHeaderRowPresenter).GetField("_indicator", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly FieldInfo _floatingHeaderField = typeof(GridViewHeaderRowPresenter).GetField("_floatingHeader", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly MethodInfo EnsureDesiredWidthListMethod = typeof(GridViewRowPresenterBase).GetMethod("EnsureDesiredWidthList", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly PropertyInfo DesiredWidthListProperty = typeof(GridViewRowPresenterBase).GetProperty("DesiredWidthList", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly PropertyInfo StateProperty = typeof(GridViewColumn).GetProperty("State", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static readonly PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty("ActualIndex", BindingFlags.NonPublic | BindingFlags.Instance);

        internal static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
            {
                return true;
            }
            double num = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * 2.2204460492503131E-16;
            double num2 = value1 - value2;
            return -num < num2 && num > num2;
        }
    }
}
