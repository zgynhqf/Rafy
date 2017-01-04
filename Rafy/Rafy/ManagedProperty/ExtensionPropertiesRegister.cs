/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151026
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151026 10:19
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 一个动态注册扩展实体属性的类型。
    /// 该类的子类会在系统初始化时、注册扩展属性的时候被调用。
    /// </summary>
    public abstract class ExtensionPropertiesRegister
    {
        internal void Register()
        {
            this.RegisterCore();
        }

        /// <summary>
        /// 子类实现此方法来实现动态注册扩展实体属性的逻辑。
        /// 注意，此方法中不能使用任何实体，否则触发该实体的静态构造函数，而在加载扩展属性前就加载非扩展属性，从而造成框架抛出异常。
        /// </summary>
        protected abstract void RegisterCore();
    }
}