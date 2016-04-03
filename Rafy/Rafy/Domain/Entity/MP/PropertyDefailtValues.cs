/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130526
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130526 16:11
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 中属性的默认值。
    /// </summary>
    public static class PropertyDefailtValues
    {
        /// <summary>
        /// 应用层可以修改此属性来变更管理属性在注册时的默认值。
        /// 
        /// 默认是 2000-01-01，这个默认值可以插入到各个数据库中。
        /// </summary>
        public static DateTime DefaultDateTime = new DateTime(2000, 1, 1);

        /// <summary>
        /// 默认的 LOB 二进制流
        /// </summary>
        public static readonly byte[] DefaultLOBBinary = new byte[0];

        /// <summary>
        /// 默认的 LOB 字符串
        /// </summary>
        public static readonly string DefaultLOBString = string.Empty;
    }
}
