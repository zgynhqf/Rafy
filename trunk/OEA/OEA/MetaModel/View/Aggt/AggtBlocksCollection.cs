/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using System.Xml.Serialization;
using hxy;
using System.IO;
using System.Collections.ObjectModel;

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 聚合块集合
    /// </summary>
    public class AggtBlocksCollection : Collection<AggtBlocks>
    {
        /// <summary>
        /// 在集合中查找第一个某指定实体类型的块。
        /// </summary>
        /// <returns></returns>
        public AggtBlocks Find(Type entityType)
        {
            return this.FirstOrDefault(b => b.MainBlock.EntityType == entityType);
        }

        /// <summary>
        /// 在集合中获取第一个某指定实体类型的块。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public AggtBlocks this[Type entityType]
        {
            get
            {
                var item = this.Find(entityType);
                if (item == null) throw new ArgumentOutOfRangeException("childType");
                return item;
            }
        }
    }

    /// <summary>
    /// 环绕块集合
    /// 
    /// 之中定义了方便使用的环绕块查询方法
    /// </summary>
    public class SurrounderCollection : AggtBlocksCollection
    {
        /// <summary>
        /// 查找某个名称的第一个环绕块
        /// </summary>
        /// <returns></returns>
        public AggtBlocks Find(string surrounderType)
        {
            foreach (var item in this)
            {
                var surrounderBlock = item.MainBlock as SurrounderBlock;
                if (surrounderBlock.SurrounderType == surrounderType) return item;
            }

            return null;
        }

        /// <summary>
        /// 获取某个名称的第一个环绕块
        /// </summary>
        /// <param name="childType"></param>
        /// <returns></returns>
        public AggtBlocks this[string surrounderType]
        {
            get
            {
                var item = this.Find(surrounderType);
                if (item == null) throw new ArgumentOutOfRangeException("childType");
                return item;
            }
        }
    }

    /// <summary>
    /// 聚合子块集合。
    /// </summary>
    public class AggtChildrenCollection : AggtBlocksCollection
    {
        private AggtBlocks _owner;

        public AggtChildrenCollection(AggtBlocks owner)
        {
            this._owner = owner;
        }

        protected override void InsertItem(int index, AggtBlocks item)
        {
            base.InsertItem(index, item);

            (item.MainBlock as ChildBlock).Owner = this._owner;
        }

        protected override void SetItem(int index, AggtBlocks item)
        {
            base.SetItem(index, item);

            (item.MainBlock as ChildBlock).Owner = this._owner;
        }
    }
}