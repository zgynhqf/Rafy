/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151112
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151112 20:33
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy
{
    /// <summary>
    /// 表示 <see cref="AppContext.Items"/> 中的一个项。
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class AppContextItem<TValue> : ContextItem<TValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppContextItem{TValue}"/> class.
        /// </summary>
        /// <param name="key">此项在 <see cref="AppContext.Items" /> 中的 Key。</param>
        /// <param name="defaultValue">如果 <see cref="AppContext.Items" /> 中没有值时，本项对应的默认值。</param>
        public AppContextItem(string key, TValue defaultValue = default(TValue)) : base(key, defaultValue) { }

        protected override IDictionary<string, object> ContextDataContainer
        {
            get { return AppContext.Items; }
        }
    }
}
