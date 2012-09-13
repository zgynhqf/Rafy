/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：OEA实体缓存的定义
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using OEA.MetaModel;
using OEA.ManagedProperty;

namespace OEA.Library
{
    public static class CacheMetaExtensionConfig
    {
        /// <summary>
        /// 启用缓存，并估计缓存的数量。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="estimatedDataCount">系统会根据此数量来选择使用的缓存策略。</param>
        /// <returns></returns>
        public static EntityMeta EnableCache(this EntityMeta meta, int estimatedDataCount)
        {
            meta.EnableCache(SimpleCacheType.Table);
            return meta;
        }

        /// <summary>
        /// 根据简单策略来启用缓存。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityMeta EnableCache(this EntityMeta meta, SimpleCacheType type = SimpleCacheType.ScopedByRoot)
        {
            var cs = new CacheScope();
            cs.Class = meta.EntityType;
            cs.SimpleCacheType = type;

            if (type == SimpleCacheType.ScopedByRoot && meta.EntityCategory != EntityCategory.Root)
            {
                cs.ScopeClass = meta.AggtRoot.EntityType;
                cs.ScopeIdGetter = e => GetRootId(e as Entity).ToString();
            }

            meta.CacheDefinition = cs;

            return meta;
        }

        private static int GetRootId(Entity entity)
        {
            var parentProeprtyMeta = entity.GetRepository().FindParentPropertyInfo(false);
            if (parentProeprtyMeta == null) { return entity.Id; }

            var refMP = parentProeprtyMeta.ManagedProperty.CastTo<IRefProperty>();
            var parent = entity.GetLazyRef(refMP).Entity;

            return GetRootId(parent);
        }

        //private static int GetSpecificParentId(ManagedPropertyObject obj, Type scopeType)
        //{
        //    var entity = obj.CastTo<Entity>();

        //    var parentProeprtyMeta = entity.GetRepository().FindParentPropertyInfo(true);
        //    var refMP = parentProeprtyMeta.ManagedProperty.CastTo<IRefProperty>();
        //    var lazyRef = entity.GetLazyRef(refMP);

        //    //如果当前的引用实体类型就是指定的范围实体类型，则返回它的 Id
        //    if (refMP.RefEntityType == scopeType) { return lazyRef.Id; }

        //    //如果还没有找到对应范围实体类型的实体，则继续往聚合树根方向找。
        //    return GetSpecificParentId(lazyRef.Entity, scopeType);
        //}
    }
}