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

namespace OEA.Library
{
    /// <summary>
    /// OEA 实体的元数据提供器
    /// </summary>
    public interface IOEAPropertyMetaProvider : IPropertyMetaProvider { }

    internal class EntityPropertyMetaProvider : IOEAPropertyMetaProvider
    {
        //public object GetDefaultValue()
        //{
        //    return "DefaultValue";
        //}
    }
}
