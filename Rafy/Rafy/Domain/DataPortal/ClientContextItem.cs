/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151113
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151113 18:40
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 表示 <see cref="DistributionContext.ClientContext"/> 中的一个项。
    /// 客户端提供的范围数据。
    /// 这些数据只会从客户端向服务端传输。
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class ClientContextItem<TValue> : ContextItem<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClientContextItem{TValue}"/> class.
        /// </summary>
        /// <param name="key">此项在 <see cref="DistributionContext.ClientContext" /> 中的 Key。</param>
        /// <param name="defaultValue">如果 <see cref="DistributionContext.ClientContext" /> 中没有值时，本项对应的默认值。</param>
        public ClientContextItem(string key, TValue defaultValue = default(TValue)) : base(key, defaultValue) { }

        protected override IDictionary<string, object> ContextDataContainer
        {
            get { return DistributionContext.ClientContext; }
        }
    }
}
