/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120913 15:08
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120913 15:08
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace Rafy
{
    /// <summary>
    /// <para>本类型表示执行的上下文环境。</para>
    /// <para>其中包含：自定义数据集合、当前身份。</para>
    /// 
    /// <para>一般可用的上下文有：</para>
    /// <para>单线程共用数据的执行环境、进程共用数据的执行环境、一次请求（如 Web）共用数据的执行环境。</para>
    /// <para>默认使用单线程上下文，如果要使用其它上下文，请使用 <see cref="SetProvider"/> 方法替换提供算法。</para>
    /// </summary>
    public class AppContext
    {
        private static IAppContextProvider _provider = new ThreadStaticAppContextProvider();

        /// <summary>
        /// 设置上下文提供程序。
        /// 默认使用 <see cref="ThreadStaticAppContextProvider"/>。
        /// </summary>
        /// <param name="context"></param>
        public static void SetProvider(IAppContextProvider context)
        {
            _provider = context;
        }

        internal static IAppContextProvider GetProvider()
        {
            return _provider;
        }

        /// <summary>
        /// 可获取或设置当前的身份。
        /// </summary>
        public static IPrincipal CurrentPrincipal
        {
            get { return _provider.CurrentPrincipal; }
            set { _provider.CurrentPrincipal = value; }
        }

        /// <summary>
        /// 当前上下文中的所有数据
        /// </summary>
        public static IDictionary<string, object> Items
        {
            get
            {
                var ctx = _provider.DataContainer;
                if (ctx == null)
                {
                    ctx = new Dictionary<string, object>();
                    _provider.DataContainer = ctx;
                }

                return ctx;
            }
        }
    }
}