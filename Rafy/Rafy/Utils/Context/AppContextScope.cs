/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150510
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150510 22:20
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
    /// ServerContextScope 的泛型版本。
    /// 
    /// 它封装了 ContextKey 的构造，提升易用性。如果要定制 ContextKey，请继承非泛型版本。
    /// </summary>
    /// <typeparam name="TSub"></typeparam>
    public abstract class AppContextScope<TSub> : ContextScope where TSub : ContextScope
    {
        /// <summary>
        /// 构造器。
        /// </summary>
        public AppContextScope()
            : base(AppContext.Items)
        {
            this.EnterScope(typeof(TSub).FullName);
        }

        /// <summary>
        /// 获取最外层的范围对象。
        /// </summary>
        /// <returns></returns>
        protected new TSub WholeScope
        {
            get { return base.WholeScope as TSub; }
        }

        /// <summary>
        /// 获取当前最外层的范围对象。
        /// </summary>
        /// <returns></returns>
        public static TSub GetWholeScope()
        {
            return ContextScope.GetWholeScope(typeof(TSub).FullName, AppContext.Items) as TSub;
        }
    }
}
