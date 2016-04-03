/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120913 16:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120913 16:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Rafy
{
    /// <summary>
    /// 服务器端上下文提供器。
    /// 默认实现：一个标记了 ThreadStatic 的字段。
    /// </summary>
    public class ThreadStaticAppContextProvider : IAppContextProvider
    {
        [ThreadStatic]
        private static IDictionary<string, object> _items;

        /// <summary>
        /// 当前线程所使用的项的集合。
        /// </summary>
        public static IDictionary<string, object> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new Dictionary<string, object>();
                }
                return _items;
            }
        }

        public IPrincipal CurrentPrincipal
        {
            get { return Thread.CurrentPrincipal; }
            set { Thread.CurrentPrincipal = value; }
        }

        public IDictionary<string, object> DataContainer
        {
            get { return _items; }
            set { _items = value; }
        }

        //protected virtual IDictionary GetLocalContext()
        //{
        //    var slot = Thread.GetNamedDataSlot(LocalContextName);
        //    return (IDictionary)Thread.GetData(slot);
        //}

        //protected virtual void SetLocalContext(IDictionary localContext)
        //{
        //    var slot = Thread.GetNamedDataSlot(LocalContextName);
        //    Thread.SetData(slot, localContext);
        //}
    }
}
