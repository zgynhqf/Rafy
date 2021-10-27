/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100908
 * 说明：一些静态方法的存放类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100908
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 一些静态方法的存放类
    /// </summary>
    internal static class Helper
    {
        /// <summary>
        /// 对指定的列表按照给定的名字顺序。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="nameGetter"></param>
        /// <param name="labels">
        /// 指定的名字顺序。
        /// 可以是list的子集。
        /// </param>
        public static void Sort<T>(IList<T> list, Func<T, string> nameGetter, params string[] labels)
        {
            if (list == null) throw new ArgumentNullException("list");

            var orginalIndeces = list
                .Select((t, i) => new SortItem<T>()
                {
                    Item = t,
                    Index = i
                })
                .Where(t => labels.Contains(nameGetter(t.Item))).ToList();

            for (int i = 0, c = labels.Length; i < c; i++)
            {
                var name = labels[i];
                list[orginalIndeces[i].Index] = orginalIndeces.First(t => nameGetter(t.Item) == name).Item;
            }
        }

        private struct SortItem<T>
        {
            public T Item;

            public int Index;
        }
    }
}
