/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130420
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130420 17:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 实体集合
    /// </summary>
    public class EntityTypeCollection : Collection<EntityType>
    {
        /// <summary>
        /// 通过全名称查找
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public EntityType Find(string fullName)
        {
            return this.FirstOrDefault(e => e.FullName == fullName);
        }
    }
}
