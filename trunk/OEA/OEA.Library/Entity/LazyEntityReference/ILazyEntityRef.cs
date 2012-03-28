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
    internal interface ILazyEntityRefInternal
    {
        void LoadId(int value);
    }

    /// <summary>
    /// 延迟加载的引用实体
    /// （外键等）
    /// </summary>
    public interface ILazyEntityRef
    {
        /// <summary>
        /// 引用对象的键
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// 此属性综合了Id和IsEmpty属性。
        /// </summary>
        int? NullableId { get; set; }

        /// <summary>
        /// 被引用的对象
        /// </summary>
        Entity Entity { get; set; }

        /// <summary>
        /// 是否已经加载/设置了实体引用对象
        /// </summary>
        bool LoadedOrAssigned { get; }

        /// <summary>
        /// 表示是否已经清空这个引用，不再引用任何实体。
        /// </summary>
        bool IsEmpty { get; }

        void Clone(ILazyEntityRef target, bool cloneEntity);

        void Clone(ILazyEntityRef target);

        ///// <summary>
        ///// 直接加载外键实体。
        ///// 
        ///// 如果list中有指定的实体，则直接加载。
        ///// 否则，从存储层加载
        ///// </summary>
        ///// <param name="list"></param>
        //void Load(IList list);
    }

    /// <summary>
    /// 延迟加载的引用实体
    /// （外键等）
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface ILazyEntityRef<TEntity> : ILazyEntityRef
        where TEntity : Entity
    {
        /// <summary>
        /// 被引用的对象
        /// </summary>
        new TEntity Entity { get; set; }
    }
}