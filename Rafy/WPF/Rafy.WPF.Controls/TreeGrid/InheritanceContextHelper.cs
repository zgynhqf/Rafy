using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Rafy.WPF.Controls
{
    internal static class InheritanceContextHelper
    {
        internal static void RemoveContextFromObject(DependencyObject context, TreeGridColumn oldValue)
        {
            if (context != null && MSInternal.GetInheritanceContext(oldValue) == context)
            {
                MSInternal.ProvideSelfAsInheritanceContext(context, oldValue, null);
            }
        }

        internal static void ProvideContextForObject(DependencyObject context, TreeGridColumn newValue)
        {
            if (context != null)
            {
                MSInternal.ProvideSelfAsInheritanceContext(context, newValue, null);
            }
        }
    }
}
