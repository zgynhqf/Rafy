/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120327
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120327
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 本地实体的仓库基类（不用存储到数据库，只存在于客户端）。
    /// 
    /// 提供了为本地实体生成 Id 的功能。
    /// 本仓库会把所有的生成的实体都存储进来。
    /// </summary>
    public abstract class MemoryEntityRepository : EntityRepository
    {
        /// <summary>
        /// 当前已经生成 Id
        /// </summary>
        private Dictionary<string, Entity> _memory;

        /// <summary>
        /// 获取给定实体的真实键。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected abstract string GetRealKey(Entity entity);

        /// <summary>
        /// 清除本地内存数据库
        /// </summary>
        protected void ClearLocalMemory()
        {
            this._memory = null;
        }

        internal override void NotifyLoadedIfMemory(Entity entity)
        {
            base.NotifyLoadedIfMemory(entity);

            if (entity.Id < 0) { this.GenerateIntId(entity); }
        }

        /// <summary>
        /// 通过本地 Id 查找实体。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected override Entity GetByIdCore(int id)
        {
            if (this._memory == null) { return null; }

            var entity = this._memory.Values.FirstOrDefault(e => e.Id == id);

            if (entity == null)
            {
                //如果还没有加载，则主动加载所有实体，然后再找到其中的那个。
                entity = this.GetAll().FirstOrDefault(e => e.Id == id);
                if (entity == null) { throw new InvalidOperationException("没有找到对应 id 的实体，可能是还没有通过仓库获取该实体。"); }
            }

            return entity;
        }

        /// <summary>
        /// 为某个模型生成临时使用的本地 Id
        /// 
        /// 该方法应该在数据层中使用。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="realKey"></param>
        private void GenerateIntId(Entity entity)
        {
            if (this._memory == null) this._memory = new Dictionary<string, Entity>();

            string realKey = this.GetRealKey(entity);

            var found = this.FindByRealKey(realKey);
            if (found != null)
            {
                if (entity != found) { entity.Id = found.Id; }
            }
            else
            {
                entity.Id = OEAEnvironment.NewLocalId();
                this._memory[realKey] = entity;
            }
        }

        /// <summary>
        /// 通过真实的键查找目标实体。
        /// </summary>
        /// <param name="realKey"></param>
        /// <returns></returns>
        protected Entity FindByRealKey(string realKey)
        {
            if (this._memory == null) { return null; }

            Entity res = null;
            this._memory.TryGetValue(realKey, out res);
            return res;
        }
    }
}