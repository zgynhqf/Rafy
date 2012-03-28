using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace OEA.Module.WPF.Shell
{
    public static class ShellHelper
    {
        public static void ForeachItemContainer<T>(ItemsControl container, Func<T, bool> actionAndStop)
            where T : HeaderedItemsControl
        {
            var generator = container.ItemContainerGenerator;

            foreach (var item in container.Items)
            {
                var subContainer = generator.ContainerFromItem(item) as T;
                if (subContainer != null)
                {
                    if (actionAndStop(subContainer)) return;

                    ForeachItemContainer<T>(subContainer, actionAndStop);
                }
            }
        }

        public static T FindItemContainer<T>(ItemsControl container, object itemToFind)
            where T : HeaderedItemsControl
        {
            var generator = container.ItemContainerGenerator;

            var result = generator.ContainerFromItem(itemToFind) as T;
            if (result != null) return result;

            foreach (var item in container.Items)
            {
                var subContainer = generator.ContainerFromItem(item) as ItemsControl;
                if (subContainer != null)
                {
                    result = FindItemContainer<T>(subContainer, itemToFind);
                    if (result != null) return result;
                }
            }

            return null;
        }
    }
}
