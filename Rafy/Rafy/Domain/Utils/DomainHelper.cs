/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150314
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150314 00:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 领域帮助类。
    /// </summary>
    public class DomainHelper
    {
        /// <summary>
        /// 判断某个值是否非空。
        /// 
        /// 如果是字符串，则检测它是否为非空字符。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static bool IsNotEmpty(object value)
        {
            bool notNull = false;

            if (value is string)
            {
                notNull = !string.IsNullOrEmpty(value as string);
            }
            else if (value != null)
            {
                notNull = true;
            }

            return notNull;
        }

        /// <summary>
        /// 枚举出属于该聚合的所有的实体类型（深度递归）。
        /// </summary>
        /// <param name="aggtRepository"></param>
        /// <returns></returns>
        public static IEnumerable<IRepository> EnumerateAllTypesInAggregation(IRepository aggtRepository)
        {
            yield return aggtRepository;

            foreach (var childProperty in aggtRepository.GetChildProperties())
            {
                Type childEntityType = null;
                var listProperty = childProperty as IListProperty;
                if (listProperty != null)
                {
                    childEntityType = listProperty.ListEntityType;
                }
                else
                {
                    var refProperty = childProperty as IRefProperty;
                    if (refProperty != null)
                    {
                        childEntityType = refProperty.RefEntityType;
                    }
                    else
                    {
                        throw new NotSupportedException("不支持其它的子属性。");
                    }
                }

                var childRepo = RF.Find(childEntityType);
                foreach (var item in EnumerateAllTypesInAggregation(childRepo))
                {
                    yield return item;
                }
            }
        }
    }
}
