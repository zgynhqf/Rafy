/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140704
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140704 23:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;

namespace Rafy.ComponentModel.UnityAdapter
{
    /// <summary>
    /// UnityContainer 创建完成事件。
    /// </summary>
    public class UnityContainerCreatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnityContainerCreatedEventArgs"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public UnityContainerCreatedEventArgs(UnityContainer container)
        {
            this.Container = container;
        }

        /// <summary>
        /// 被创建的 UnityContainer。
        /// </summary>
        public UnityContainer Container { get; private set; }
    }
}