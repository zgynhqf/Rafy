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

namespace OEA.Library.Caching
{
    /// <summary>
    /// OEA实体缓存的定义，主要包括启用实体的缓存和实体缓存更新策略的范围定义。
    /// </summary>
    public class CacheDefinition
    {
        /// <summary>
        /// SingleTon
        /// </summary>
        public static readonly CacheDefinition Instance = new CacheDefinition();

        private CacheScope _lastCacheScope;

        /// <summary>
        /// 所有的缓存定义
        /// </summary>
        private List<CacheScope> _items = new List<CacheScope>();

        private CacheDefinition() { }

        /// <summary>
        /// Define With Scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TScope"></typeparam>
        /// <param name="scopeId"></param>
        public void Enable<T, TScope>(Func<Entity, int> getScopeIdByParent)
            where T : Entity
            where TScope : Entity
        {
            this.Enable<T, TScope>(e => getScopeIdByParent(e).ToString());
        }

        public void Enable<T, TScope>()
            where T : Entity
            where TScope : Entity
        {
            this.Enable<T, TScope>(e => e.Id.ToString());
        }

        public void Enable<T>()
            where T : Entity
        {
            this.Enable<T, T>(e => e.Id.ToString());
        }

        /// <summary>
        /// Define With Scope
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TScope"></typeparam>
        /// <param name="scopeId"></param>
        public void Enable<T, TScope>(Func<Entity, string> getScopeIdByParent)
            where T : Entity
            where TScope : Entity
        {
            var s = new CacheScope()
            {
                Class = typeof(T),
                ScopeClass = typeof(TScope),
                ScopeIdGetter = getScopeIdByParent
            };
            this._items.Add(s);
        }

        public bool TryGetScope(Type entityType, out CacheScope scopeDef)
        {
            var lcs = _lastCacheScope;
            if (lcs != null && lcs.Class == entityType)
            {
                scopeDef = lcs;
                return true;
            }

            scopeDef = null;

            for (int i = 0, c = this._items.Count; i < c; i++)
            {
                var item = this._items[i];
                if (item.Class == entityType)
                {
                    scopeDef = item;
                    _lastCacheScope = item;
                    return true;
                }
            }

            return false;
        }

        public bool IsEnabled(Type entityType)
        {
            CacheScope scopeDef = null;
            return TryGetScope(entityType, out scopeDef);
        }
    }

    /// <summary>
    /// 某个类型所使用的缓存更新范围。
    /// </summary>
    public class CacheScope
    {
        /// <summary>
        /// 为这个类型定义的范围。
        /// </summary>
        public Type Class { get; set; }

        /// <summary>
        /// 此属性表示为Class作为范围的类型。
        /// （注意：应该是在聚合对象树中，Class的上层类型。）
        /// 如果此属性为null，表示Class以本身作为缓存范围。
        /// </summary>
        public Type ScopeClass { get; set; }

        /// <summary>
        /// 此属性表示为Class作为范围的类型的对象ID。
        /// 如果此属性为null，表示Class不以某一特定的范围对象作为范围，而是全体对象。
        /// </summary>
        public Func<Entity, string> ScopeIdGetter { get; set; }

        /// <summary>
        /// 表示Class以本身作为缓存范围。
        /// </summary>
        public bool ScopeBySelf
        {
            get
            {
                return this.ScopeClass == null;
            }
        }

        /// <summary>
        /// 表示Class是否以某一特定的范围对象作为范围。
        /// </summary>
        public bool ScopeById
        {
            get
            {
                return this.ScopeIdGetter != null;
            }
        }
    }
}