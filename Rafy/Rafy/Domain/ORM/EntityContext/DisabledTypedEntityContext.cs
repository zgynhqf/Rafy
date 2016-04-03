/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121221 17:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121221 17:04
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 一个没有任何功能的上下文。
    /// </summary>
    internal class DisabledTypedEntityContext : TypedEntityContext
    {
        internal override void Add(object id, IEntity entity) { }

        internal override void Set(object id, IEntity entity) { }

        internal override IEntity TryGetById(object id)
        {
            return null;
        }
    }
}