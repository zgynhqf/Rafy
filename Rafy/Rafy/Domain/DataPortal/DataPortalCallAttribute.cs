/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211115
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211115 10:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// 标记类型的某个方法为公开的数据门户调用方法。
    /// <para>框架会判断是需要在本地、还是服务端来执行此方法。如果需要在服务端执行，则框架会转而调用 WCF 数据门户。（如果需要分布式调用，所有参数需要支持可序列化。）</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DataPortalCallAttribute : Attribute { }
}