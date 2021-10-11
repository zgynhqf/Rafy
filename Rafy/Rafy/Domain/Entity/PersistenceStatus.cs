/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体的持久化状态。
    /// 保存实体时，会根据这个状态来进行对应的增、删、改的操作。
    /// </summary>
    public enum PersistenceStatus
    {
        /// <summary>
        /// 已保存。
        /// <para>表示该实体当前的所有状态已经持久化到数据库中。</para>
        /// <para>注意，它只表示当前实体状态被持久化。而不包含聚合子实体。</para>
        /// <para>注意，如果要设置实体的持久化状态为 Saved，请使用 <see cref="Entity.MarkSaved"/> 方法。该方法会同时重置所有属性的变更状态。</para>
        /// <para>
        /// The object has not been modified since it was loaded into the memory or since
        /// the last time that the Repository.Save method was
        /// called.
        /// </para>
        /// </summary>
        Saved = 0,
        [Obsolete("此枚举将不再可用，请使用 PersistenceStatus.Saved。")]
        Unchanged = 0,
        /// <summary>
        /// 数据变更。
        /// <para>仓库保存时，需要执行更新操作。</para>
        /// <para>
        /// The object is changed but the Repository.Save method was not called.
        /// </para>
        /// </summary>
        Modified = 1,
        /// <summary>
        /// 新对象。
        /// <para>仓库保存时，需要执行添加操作。</para>
        /// <para>注意：给实体设置状态为 New 时，为清空实体的 Id。</para>
        /// <para>
        /// The object is new to the database and the Repository.Save
        /// method has not been called.
        /// </para>
        /// </summary>
        New = 2,
        /// <summary>
        /// 已删除。
        /// <para>仓库保存时，需要执行删除操作。</para>
        /// <para>
        /// The object is deleted in memory, and not saved into database.
        /// </para>
        /// </summary>
        Deleted = 3
    }
}