/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.Attributes
{
    /// <summary>
    /// 查询页签
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class NavigateQueryItemAttribute : Attribute
    {
        /// <summary>
        /// 如果本导航属性是一个集合时，IdPropertyName 表示集合的主键应该赋值给我这个导航对象的哪个属性。
        /// </summary>
        public string IdPropertyAfterSelection { get; set; }
    }
}