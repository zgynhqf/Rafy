/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 环绕块
    /// 
    /// 支持 XML 序列化
    /// </summary>
    public class SurrounderBlock : Block
    {
        public SurrounderBlock(Type entityType) : base(entityType) { }

        public SurrounderBlock() { }

        /// <summary>
        /// 环绕类型
        /// </summary>
        public SurrounderType SurrounderType { get; set; }
    }

    /// <summary>
    /// 默认支持的一些 环绕类型
    /// </summary>
    public enum SurrounderType
    {
        Navigation, Condition, Result,
        List, Detail
    }

    public static class SurrounderTypeExtension
    {
        public static string GetDescription(this SurrounderType type)
        {
            return type.ToString().ToLower();
        }
    }
}
