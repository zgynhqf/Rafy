/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140629
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140629 11:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    /// <summary>
    /// 在某个类型上指定的标记，说明该类型将会注册到 IOC 默认容器中。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ContainerItemAttribute : Attribute
    {
        /// <summary>
        /// 构造器。
        /// </summary>
        /// <param name="provideFor">为这个类型提供实例。</param>
        public ContainerItemAttribute(Type provideFor)
        {
            if (provideFor == null) throw new ArgumentNullException("provideFor");
            this.ProvideFor = provideFor;

            this.RegisterWay = RegisterWay.Type;
        }

        /// <summary>
        /// 为这个类型提供实例。
        /// </summary>
        public Type ProvideFor { get; private set; }

        /// <summary>
        /// 注册到 IOC 容器中的方式。默认值为 <see cref="Rafy.ComponentModel.RegisterWay.Type"/>。
        /// </summary>
        public RegisterWay RegisterWay { get; set; }

        /// <summary>
        /// 注册时使用的键。
        /// </summary>
        public string Key { get; set; }
    }

    /// <summary>
    /// 注册到 IOC 容器中的方式。
    /// </summary>
    public enum RegisterWay
    {
        /// <summary>
        /// 以单一实例的方式注册。
        /// </summary>
        Instance,
        /// <summary>
        /// 以类型的方式注册。
        /// </summary>
        Type
    }
}
