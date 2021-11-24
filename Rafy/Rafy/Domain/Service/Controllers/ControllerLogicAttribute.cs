/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211125
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211125 01:41
 * 
*******************************************************/

using Rafy.DataPortal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 标记 <see cref="DomainController"/> 中的某个虚方法是一个控制器主逻辑方法。
    /// 标记后，获得以下功能：
    /// * 支持远程调用。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ControllerLogicAttribute : DataPortalCallAttribute { }
}