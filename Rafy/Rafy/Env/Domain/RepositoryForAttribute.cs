/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140506
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140506 00:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 标记某一个类型是指定的实体对应的仓库类型。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RepositoryForAttribute : Attribute
    {
        public RepositoryForAttribute(Type entityType)
        {
            this.EntityType = entityType;
        }

        /// <summary>
        /// 对应的实体类型。
        /// </summary>
        public Type EntityType { get; private set; }
    }
}
