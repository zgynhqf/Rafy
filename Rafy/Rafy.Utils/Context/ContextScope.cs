/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130417
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130417 12:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 基于 IDictionary 服务上下文的一个范围类型。
    /// 继承此类的子类都拥有多层嵌套声明范围的功能，只有最外层的范围对象的 ExitWholeScope 方法会被调用。
    /// 
    /// 注意：必须使用 using 来构造此类型的子类，否则会出现未知的问题。
    /// </summary>
    public abstract class ContextScope : IDisposable
    {
        /// <summary>
        /// 存储项的位置
        /// </summary>
        private IDictionary<string, object> _context;
        /// <summary>
        /// 是否已经进入本对象声明的代码范围。
        /// </summary>
        private bool _scopeEntered;
        /// <summary>
        /// 本对象是否已经被析构。
        /// </summary>
        private bool _disposed;
        /// <summary>
        /// 在 <see cref="_context"/> 中的名称。
        /// </summary>
        private string _contextKey;
        /// <summary>
        /// 最外层的范围对象缓存。
        /// </summary>
        private ContextScope _wholeScope;
        /// <summary>
        /// 目前进入到了第几个代码范围。
        /// 这个字段只在最外层范围上有用。
        /// </summary>
        private int _scopeCount;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context"></param>
        protected ContextScope(IDictionary<string, object> context)
        {
            _context = context;
        }

        /// <summary>
        /// 获取最外层的范围对象。
        /// </summary>
        /// <returns></returns>
        protected ContextScope WholeScope
        {
            get
            {
                if (_wholeScope == null)
                {
                    //从上下文中中查找最外层的范围对象，如果还没有，则直接添加本对象为最外层的范围对象。
                    object res = null;
                    if (!_context.TryGetValue(_contextKey, out res))
                    {
                        res = this;
                        this.EnterWholeScope();
                        _context.Add(_contextKey, res);
                    }

                    _wholeScope = res as ContextScope;
                }

                return _wholeScope;
            }
        }

        /// <summary>
        /// 声明进入这个范围对象声明的范围代码。
        /// </summary>
        /// <param name="contextKey"></param>
        protected void EnterScope(string contextKey)
        {
            if (_scopeEntered) throw new InvalidOperationException("不可重复进入本对象声明的代码范围。");
            if (string.IsNullOrEmpty(contextKey)) throw new ArgumentNullException("contextKey");

            _contextKey = contextKey;

            var item = this.WholeScope;
            item._scopeCount++;

            _scopeEntered = true;
        }

        /// <summary>
        /// 进入最外层范围时，会调用此方法。
        /// </summary>
        protected abstract void EnterWholeScope();

        /// <summary>
        /// 如果本对象是最外层的范围对象，则这个对象的这个方法会在范围退出时执行。
        /// </summary>
        protected abstract void ExitWholeScope();

        /// <summary>
        /// 本对象的范围结束。
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                object res = null;
                _context.TryGetValue(_contextKey, out res);

                var item = res as ContextScope;
                if (item != null)//其实这里，必须不为空。
                {
                    //只是把引用次数减一
                    item._scopeCount--;

                    //当数据到达 0 时，说明所有提交完毕，这时才真正提交事务。
                    if (item._scopeCount <= 0)
                    {
                        //从上下文中移除
                        _context.Remove(_contextKey);

                        //通知项开始执行退出逻辑。
                        item.ExitWholeScope();
                    }
                }

                _disposed = true;
            }

            _scopeEntered = false;
        }

        #region Dispose Pattern

        ~ContextScope()
        {
            this.Dispose(false);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// 子类可以通过这个方法来获取当前最外层的范围对象。
        /// </summary>
        /// <param name="contextKey">The context key.</param>
        /// <param name="store">The store.</param>
        /// <returns></returns>
        protected static ContextScope GetWholeScope(string contextKey, IDictionary<string, object> store)
        {
            object res = null;
            store.TryGetValue(contextKey, out res);
            return res as ContextScope;
        }
    }
}
