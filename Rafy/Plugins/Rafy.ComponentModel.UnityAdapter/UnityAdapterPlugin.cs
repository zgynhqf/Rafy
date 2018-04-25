/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140704
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140704 23:06
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel.UnityAdapter
{
    /// <summary>
    /// 从 Unity 适配到 IObjectContainer 的插件。
    /// 使用此插件后，Rafy 平台的 IOC 框架将由 UnityContainer 来实现。
    /// </summary>
    public class UnityAdapterPlugin : DomainPlugin
    {
        /// <summary>
        /// 插件的初始化方法。
        /// </summary>
        /// <param name="app">应用程序对象。</param>
        public override void Initialize(IApp app)
        {
            ObjectContainerFactory.SetProvider(new UnityContainerAdapterFactory());
        }
    }
}