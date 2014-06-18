/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:59
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:59
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 目前支持的值属性的类型
    /// </summary>
    public enum ValuePropertyType
    {
        /// <summary>
        /// 字符串
        /// </summary>
        String,
        /// <summary>
        /// 布尔
        /// </summary>
        Boolean,
        /// <summary>
        /// 整形
        /// </summary>
        Int,
        /// <summary>
        /// 浮点
        /// </summary>
        Double,
        /// <summary>
        /// 金钱
        /// </summary>
        Decimal,
        /// <summary>
        /// 时间
        /// </summary>
        DateTime,
        /// <summary>
        /// 枚举
        /// </summary>
        Enum,
        /// <summary>
        /// 二进制流
        /// </summary>
        Bytes,
        /// <summary>
        /// 未知类型。
        /// 例如一些临时的内存属性。
        /// </summary>
        Unknown
    }
}
