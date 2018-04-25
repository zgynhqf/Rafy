/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140704
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140704 23:13
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
    class UnityContainerAdapterFactory : IObjectContainerFactory
    {
        public IObjectContainer CreateContainer()
        {
            var container = new UnityContainer();

            UnityAdapterHelper.OnUnityContainerCreated(new UnityContainerCreatedEventArgs(container));

            return new UnityContainerAdapter(container);
        }
    }
}