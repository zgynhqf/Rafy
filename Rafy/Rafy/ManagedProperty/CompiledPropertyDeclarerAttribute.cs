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

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 编译/启动期属性声明器
    /// 
    /// 声明了托管属性（包括扩展属性）的类标记这个接口后，
    /// 框架会在启动期自动查找这些类，并把其中的托管属性都初始化为启动期属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class CompiledPropertyDeclarerAttribute : Attribute { }
}
