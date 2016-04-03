/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101027
 * 说明：类型的比较器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101027
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Utils
{
    /// <summary>
    /// 类型的比较器
    /// </summary>
    public class TypeNameComparer : IComparer<Type>
    {
        public static readonly TypeNameComparer Instance = new TypeNameComparer();

        private TypeNameComparer() { }

        /// <summary>
        /// TypeNameComparer 先尝试使用Name来比较，如果一样，再使用NameSpace进行比较。
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int IComparer<Type>.Compare(Type x, Type y)
        {
            var result = x.Name.CompareTo(y.Name);
            if (result == 0)
            {
                result = x.Namespace.CompareTo(y.Namespace);
            }
            return result;
        }
    }
}
