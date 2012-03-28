/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110328
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel
{
    /// <summary>
    /// 属性的约定
    /// </summary>
    public static class PropertyConvention
    {
        /// <summary>
        /// 选择列表操作方式下约定选择属性
        /// </summary>
        public const string IsSelected = "IsSelected";

        /// <summary>
        /// 获取约定的外键引用实体的属性名。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetRefEntityPropertyName(Type entityType)
        {
            return entityType.Name;
        }
    }
}
