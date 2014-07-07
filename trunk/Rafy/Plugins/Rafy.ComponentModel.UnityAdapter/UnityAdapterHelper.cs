/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140704
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140704 23:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;

namespace Rafy.ComponentModel.UnityAdapter
{
    /// <summary>
    /// 常用公共方法。
    /// </summary>
    public static class UnityAdapterHelper
    {
        /// <summary>
        /// 如果已经在应用程序插件中添加了本插件，
        /// 则可以使用本方法来获取 IObjectContainer 中的 UnityContainer 容器。
        /// </summary>
        /// <param name="objectContainer"></param>
        /// <returns></returns>
        public static UnityContainer GetUnityContainer(IObjectContainer objectContainer)
        {
            var adapter = objectContainer as UnityContainerAdapter;
            if (adapter != null)
            {
                return adapter.Container;
            }
            return null;
        }

        /// <summary>
        /// 当容器工厂创建每一个容器时，都会发生此事件。
        /// </summary>
        public static event EventHandler<UnityContainerCreatedEventArgs> UnityContainerCreated;

        internal static void OnUnityContainerCreated(UnityContainerCreatedEventArgs e)
        {
            var handler = UnityContainerCreated;
            if (handler != null) handler(null, e);
        }
    }
}