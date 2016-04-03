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
    /// 托管属性生命周期
    /// </summary>
    public enum ManagedPropertyLifeCycle
    {
        /// <summary>
        /// 编译期、启动期
        /// </summary>
        Compile,

        /// <summary>
        /// 运行期（动态属性）
        /// </summary>
        Runtime
    }
}