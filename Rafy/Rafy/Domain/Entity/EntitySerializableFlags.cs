/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121122 16:44
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121122 16:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 前三位一起表示 PersistenceStatus，其它位表示一些 boolean 值。
    /// </summary>
    [Flags]
    internal enum EntitySerializableFlags
    {
        #region Persistence

        /// <summary>
        /// 数据变更
        /// </summary>
        IsModified = 1,
        /// <summary>
        /// 新对象
        /// </summary>
        IsNew = 2,
        /// <summary>
        /// 标记删除
        /// </summary>
        IsDeleted = 4,

        DeletedOrNew = IsNew | IsDeleted,
        PersistenceAll = IsNew | IsDeleted | IsModified,

        #endregion

        /// <summary>
        /// 是否需要在数据层更新对应的冗余值。
        /// </summary>
        UpdateRedundancies = 8,

        /// <summary>
        /// 是否为树的叶子节点。
        /// </summary>
        isTreeLeaf = 16,

        AllMask = 0xFFFF
    }
}