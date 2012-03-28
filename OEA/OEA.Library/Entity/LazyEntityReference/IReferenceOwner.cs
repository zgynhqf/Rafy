/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110422
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 外键的拥有者
    /// </summary>
    internal interface IReferenceOwner
    {
        bool NotifyIdChanging(LazyEntityRefPropertyInfo refInfo, int newId);

        bool NotifyEntityChanging(LazyEntityRefPropertyInfo refInfo, Entity newEntity);

        /// <summary>
        /// 当外键改变时，可以回调Owner，通知它Id属性已经改变。
        /// </summary>
        /// <param name="refIdProperty"></param>
        void NotifyIdChanged(LazyEntityRefPropertyInfo refInfo, int oldId, int newId);

        /// <summary>
        /// 当外键改变时，可以回调Owner，通知它外键引用的实体属性已经改变。
        /// </summary>
        /// <param name="refEntityProperty"></param>
        void NotifyEntityChanged(LazyEntityRefPropertyInfo refInfo, Entity oldEntity, Entity newEntity);
    }
}