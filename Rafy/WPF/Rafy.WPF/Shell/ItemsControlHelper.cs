/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace Rafy.WPF
{
    public static class ItemsControlHelper
    {
        /// <summary>
        /// 对于指定 ItemsControl 中所有项进行递归遍历，返回找到的 T 类型的容器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="iterator">返回值表示是否停止整个递归活动。</param>
        /// <returns></returns>
        public static T ForeachItemContainer<T>(ItemsControl container, Func<T, bool> iterator)
            where T : HeaderedItemsControl
        {
            var generator = container.ItemContainerGenerator;

            foreach (var item in container.Items)
            {
                var subContainer = generator.ContainerFromItem(item) as T;
                if (subContainer != null)
                {
                    if (iterator(subContainer)) return subContainer;

                    var res = ForeachItemContainer<T>(subContainer, iterator);
                    if (res != null) return res;
                }
            }

            return null;
        }

        /// <summary>
        /// 对于指定 ItemsControl 中所有项进行递归遍历，返回对应 item 的 T 类型的容器。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="itemToFind"></param>
        /// <returns></returns>
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
