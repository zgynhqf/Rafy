/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120330
 * 
*******************************************************/

using System;
using Rafy.ManagedProperty;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 定义所有验证规则方法签名的委托。
    /// </summary>
    /// <param name="target">需要被验证的属性对象。</param>
    /// <param name="e">用于接收验证后的信息。</param>
    public delegate void RuleHandler(ManagedPropertyObject target, RuleArgs e);
}