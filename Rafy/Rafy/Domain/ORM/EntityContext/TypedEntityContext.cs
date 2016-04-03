/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120831 15:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120831 15:35
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
    /// 某一特定实体类型对应的实体上下文
    /// </summary>
    internal class TypedEntityContext : IEnumerable<IEntity>
    {
        /// <summary>
        /// 从 id 到 实体 的键值对。
        /// Key：id
        /// Value：Entity
        /// </summary>
        private Dictionary<object, IEntity> _entities = new Dictionary<object, IEntity>();

        /// <summary>
        /// 特定的实体类型。
        /// </summary>
        public Type EntityType;

        /// <summary>
        /// 通过 id 在上下文中查找对应的实体。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal virtual IEntity TryGetById(object id)
        {
            IEntity res = null;
            this._entities.TryGetValue(id, out res);
            return res;
        }

        /// <summary>
        /// 直接添加一个实体到上下文中
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        internal virtual void Add(object id, IEntity entity)
        {
            this._entities.Add(id, entity);
        }

        /// <summary>
        /// 设置或者刷新上下文中对应 id 的实体
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        internal virtual void Set(object id, IEntity entity)
        {
            this._entities[id] = entity;
        }

        public IEnumerator<IEntity> GetEnumerator()
        {
            return this._entities.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._entities.Values.GetEnumerator();
        }
    }
}