/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140515
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140515 23:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 所有支持的主键的算法容器。
    /// </summary>
    public class KeyProviders
    {
        internal static List<IKeyProvider> Items= new List<IKeyProvider>
        {
            new IntKeyProvider(),
            new StringKeyProvider(),
            new LongKeyProvider(),
            new ObjectKeyProvider(),
            new GuidKeyProvider()
        };

        /// <summary>
        /// 获取指定类型的主键算法。
        /// </summary>
        /// <param name="keyType"></param>
        /// <returns></returns>
        public static IKeyProvider Get(Type keyType)
        {
            //由于量比较少，所以直接避免的性能是最好的。
            for (int i = 0, c = Items.Count; i < c; i++)
            {
                var item = Items[i];
                if (item.KeyType == keyType) return item;
            }
            throw new NotSupportedException("不支持这个类型的主键：" + keyType);
        }
    }
}
