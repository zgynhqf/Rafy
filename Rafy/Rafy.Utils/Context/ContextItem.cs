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
    /// 表示一个从上下文数据环境中的一个项。
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class ContextItem<TValue>
    {
        private string _key;
        private TValue _defaultValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextItem{TValue}"/> class.
        /// </summary>
        /// <param name="key">此项在 <see cref="ContextDataContainer" /> 中的 Key。</param>
        /// <param name="defaultValue">如果 <see cref="ContextDataContainer" /> 中没有值时，本项对应的默认值。</param>
        /// <exception cref="System.ArgumentNullException">key</exception>
        public ContextItem(string key, TValue defaultValue = default(TValue))
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
            _key = key;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// 子类实现此属性来提供使用的上下文数据窗口。
        /// </summary>
        protected abstract IDictionary<string, object> ContextDataContainer { get; }

        /// <summary>
        /// 获取或设置当前代码上下文中的本项对应的值。
        /// </summary>
        public TValue Value
        {
            get
            {
                object value = null;
                this.ContextDataContainer.TryGetValue(_key, out value);
                if (value == null) return _defaultValue;
                return (TValue)value;
            }
            set
            {
                if (object.Equals(value, _defaultValue))
                {
                    this.ContextDataContainer.Remove(_key);
                }
                else
                {
                    this.ContextDataContainer[_key] = value;
                }
            }
        }

        /// <summary>
        /// 可以使用 using 语法调用本方法来声明一个代码段，在代码段中时 AppContext 中本项的值将使用指定的值；
        /// 当跳出代码段时，本项的值又回复原来的值。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IDisposable UseScopeValue(TValue value)
        {
            var oldValue = this.Value;
            if (object.Equals(value, oldValue)) return EmptyAgent.Instance;

            var scope = new UseScopeValueAgent
            {
                Ext = this,
                OldValue = oldValue
            };

            this.Value = value;

            return scope;
        }

        private class UseScopeValueAgent : IDisposable
        {
            internal ContextItem<TValue> Ext;
            internal TValue OldValue;

            public void Dispose()
            {
                Ext.Value = OldValue;
            }
        }
    }

    internal class EmptyAgent : IDisposable
    {
        public static readonly EmptyAgent Instance = new EmptyAgent();

        private EmptyAgent() { }

        public void Dispose() { }
    }
}
