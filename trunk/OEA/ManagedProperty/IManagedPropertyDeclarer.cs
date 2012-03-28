/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.ManagedProperty
{
    /// <summary>
    /// 所有声明了托管属性（包括附加属性）的类都需要实现这个接口，框架会自动查找这些类，并进行初始化。
    /// </summary>
    public interface IManagedPropertyDeclarer { }
}
