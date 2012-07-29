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
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA
{
    /// <summary>
    /// OEA 中的属性元数据
    /// </summary>
    public interface IPropertyMetadata : IManagedPropertyMetadata
    {
        /// <summary>
        /// 如果当前是一个时间类型，则这个属性表示该时间类型正在被使用的部分。
        /// </summary>
        DateTimePart DateTimePart { get; }
    }
}