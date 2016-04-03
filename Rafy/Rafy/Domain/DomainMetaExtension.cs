/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：Rafy实体缓存的定义
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
using Rafy;
using Rafy.MetaModel;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy.Library 对元数据的扩展
    /// </summary>
    public static class DomainMetaExtension
    {
        /// <summary>
        /// 启用服务端内存缓存。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityMeta EnableServerCache(this EntityMeta meta)
        {
            meta.ServerCacheEnabled = true;
            return meta;
        }

        /// <summary>
        /// 启用缓存，并估计缓存的数量。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="estimatedDataCount">
        /// 预估的数据行数。
        /// 系统会根据此数量来选择使用的缓存策略。例如：超过 1000 条的组合子对象使用组合缓存。
        /// </param>
        /// <returns></returns>
        public static EntityMeta EnableClientCache(this EntityMeta meta, int estimatedDataCount)
        {
            var type = ClientCacheScopeType.Table;

            //超过 1000 条的组合子对象使用组合缓存。
            if (estimatedDataCount >= 1000 && meta.EntityCategory == EntityCategory.Child)
            {
                type = ClientCacheScopeType.ScopedByRoot;
            }

            meta.EnableClientCache(type);
            return meta;
        }

        /// <summary>
        /// 根据简单策略来启用缓存。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static EntityMeta EnableClientCache(this EntityMeta meta, ClientCacheScopeType type = ClientCacheScopeType.ScopedByRoot)
        {
            var cs = new ClientCacheScope();
            cs.Class = meta.EntityType;
            cs.SimpleScopeType = type;

            if (type == ClientCacheScopeType.ScopedByRoot && meta.EntityCategory != EntityCategory.Root)
            {
                cs.ScopeClass = meta.AggtRoot.EntityType;
                cs.ScopeIdGetter = e => GetRootId(e as Entity).ToString();
            }

            meta.ClientCacheDefinition = cs;

            return meta;
        }

        #region EntityMeta.SaveListServiceType

        /// <summary>
        /// '列表保存服务类型'属性在扩展属性中的键。
        /// </summary>
        private const string SaveListServiceTypeKey = "DomainMetaExtension.SaveListServiceType";

        /// <summary>
        /// 设置对象的'列表保存服务类型'属性。
        /// </summary>
        /// <param name="ext">扩展属性的对象。</param>
        /// <param name="value">设置的属性值。</param>
        /// <returns>扩展属性的对象。</returns>
        public static EntityMeta SetSaveListServiceType(this EntityMeta ext, Type value)
        {
            ext.SetExtendedProperty(SaveListServiceTypeKey, value);

            return ext;
        }

        /// <summary>
        /// 获取对象的'列表保存服务类型'属性。
        /// </summary>
        /// <param name="ext">扩展属性的对象。</param>
        /// <returns>被扩展的属性值，或者该属性的默认值。</returns>
        public static Type GetSaveListServiceType(this EntityMeta ext)
        {
            return ext.GetPropertyOrDefault(SaveListServiceTypeKey, default(Type));
        }

        #endregion

        private static object GetRootId(Entity entity)
        {
            var parentProeprtyMeta = entity.GetRepository().FindParentPropertyInfo(false);
            if (parentProeprtyMeta == null) { return entity.Id; }

            var refMP = parentProeprtyMeta.ManagedProperty.CastTo<IRefEntityProperty>();
            var parent = entity.GetRefEntity(refMP);

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