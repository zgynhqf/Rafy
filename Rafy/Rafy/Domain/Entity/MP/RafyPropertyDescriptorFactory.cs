/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120904 10:15
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120904 10:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 属性描述器工厂
    /// </summary>
    internal class RafyPropertyDescriptorFactory : PropertyDescriptorFactory
    {
        protected override PropertyDescriptor CreateDescriptor(IManagedProperty mp)
        {
            return new RafyPropertyDescriptor(mp);
        }
    }
}