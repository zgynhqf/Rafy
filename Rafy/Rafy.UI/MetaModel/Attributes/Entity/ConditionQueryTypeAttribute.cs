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

namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 对该类进行条件查询的条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ConditionQueryTypeAttribute : Attribute
    {
        public ConditionQueryTypeAttribute(Type queryType)
        {
            QueryType = queryType;
        }

        /// <summary>
        /// 条件查询的条件对象的类型
        /// </summary>
        public Type QueryType { get; private set; }
    }
}