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

namespace OEA.MetaModel
{
    /// <summary>
    /// 可附加参数的行为
    /// 
    /// 可以给元数据附加一些额外的参数
    /// </summary>
    public interface ICustomParamsHolder
    {
        /// <summary>
        /// 获取指定参数的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <returns></returns>
        T TryGetCustomParams<T>(string paramName);

        /// <summary>
        /// 设置自定义参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        void SetCustomParams(string paramName, object value);

        /// <summary>
        /// 获取已经设置的所有的自定义参数
        /// </summary>
        /// <returns></returns>
        IEnumerable<KeyValuePair<string, object>> GetAllCustomParams();
    }

    public static class CustomParamsHolderExtension
    {
        /// <summary>
        /// 从特定的参数存储器中拷贝所有自定义参数
        /// </summary>
        /// <param name="a"></param>
        /// <param name="target"></param>
        public static void CopyParams(this ICustomParamsHolder a, ICustomParamsHolder target)
        {
            foreach (var kv in target.GetAllCustomParams())
            {
                a.SetCustomParams(kv.Key, kv.Value);
            }
        }
    }
}
