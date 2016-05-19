/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：可附加参数的行为
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 可附加参数的行为
    /// 
    /// 可以给元数据附加一些额外的参数
    /// </summary>
    public interface IExtendable
    {
        /// <summary>
        /// 返回当前已经扩展的属性个数。
        /// </summary>
        int ExtendedPropertiesCount { get; }

        /// <summary>
        /// 通过属性名称设置/获取某个值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        object this[string property] { get; set; }

        /// <summary>
        /// 获取指定名称的扩展属性值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        T GetPropertyOrDefault<T>(string property, T defaultValue = default(T));

        /// <summary>
        /// 设置某个扩展属性为指定的值。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        void SetExtendedProperty(string property, object value);

        /// <summary>
        /// 获取已经设置的所有的扩展属性
        /// </summary>
        /// <returns></returns>
        IReadOnlyDictionary<string, object> GetExtendedProperties();
    }

    public static class IExtendableExtension
    {
        /// <summary>
        /// 从特定的参数存储器中拷贝所有自定义参数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="target"></param>
        public static void CopyExtendedProperties(this IExtendable a, IExtendable target)
        {
            foreach (var kv in target.GetExtendedProperties())
            {
                a[kv.Key] = kv.Value;
            }
        }
    }
}
