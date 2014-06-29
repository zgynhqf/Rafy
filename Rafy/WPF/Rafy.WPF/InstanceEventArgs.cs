/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2011
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF
{
    /// <summary>
    /// 实例被创建事件的参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class InstanceEventArgs<T> : EventArgs
        where T : class
    {
        public InstanceEventArgs(T instance)
        {
            this.Instance = instance;
        }

        /// <summary>
        /// 被创建的实例
        /// </summary>
        public T Instance { get; private set; }
    }
}
