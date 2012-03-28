/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：实体列表的数据存储器。
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
using OEA.Utils.Caching;
using System.Runtime.Caching;
using OEA.MetaModel;

namespace OEA.Library.Caching
{
    /// <summary>
    /// 实体列表的数据存储器。
    /// 本类设计为整合Cache、CacheDefinition、VersionChecker的外观模式类，为Entity和EntityList提供了简单、统一的接口。
    /// 
    /// 实体行的缓存使用SQLCompact进行缓存，所以只在客户端使用。
    /// </summary>
    public class EntityRowCache
    {
        public static readonly EntityRowCache Instance = new EntityRowCache();

        private EntityRowCache() { }

        /// <summary>
        /// 从缓存中读取指定实体类型的所有数据。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用ifNotExsits方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="ifNotExsits"></param>
        /// <returns></returns>
        public IList<Entity> CacheAll(Type entityType, Func<IList<Entity>> ifNotExsits = null)
        {
            IList<Entity> result = null;

            //目前只是在客户端使用了缓存。
            if (OEAEnvironment.Location.IsOnClient())
            {
                CacheScope sd = TryGetScope(entityType);

                if (sd != null)
                {
                    var className = sd.Class.Name;
                    var key = "All";

                    result = CacheInstance.Memory.Get(key, className) as IList<Entity>;

                    //如果内存不存在此数据，则尝试使用硬盘缓存获取
                    if (result == null)
                    {
                        result = CacheInstance.SqlCe.Get(key, className) as IList<Entity>;

                        var policy = new Policy()
                        {
                            Checker = new VersionChecker(sd.Class)
                        };

                        //如果硬盘缓存不存在此数据，则使用指定方法获取数据
                        if (result == null && ifNotExsits != null)
                        {
                            result = ifNotExsits();
                            CacheInstance.SqlCe.Add(key, result, policy, className);
                        }

                        if (result != null)
                        {
                            CacheInstance.Memory.Add(key, result, policy, className);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 从缓存中读取指定实体类型的某个父对象下的所有子对象。
        /// 如果缓存中不存在，或者缓存数据已经过期，则调用ifNotExsits方法获取数据，并把最终数据加入到缓存中。
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="parent"></param>
        /// <param name="ifNotExsits"></param>
        /// <returns></returns>
        public IList<Entity> CacheByParent(Type entityType, Entity parent, Func<int, IList<Entity>> ifNotExsits = null)
        {
            IList<Entity> result = null;

            if (OEAEnvironment.Location.IsOnClient())
            {
                CacheScope sd = TryGetScope(entityType);

                if (sd != null)
                {
                    var className = sd.Class.Name;
                    var key = string.Format("ByParentId_{0}", parent.Id);
                    result = CacheInstance.SqlCe.Get(key, className) as IList<Entity>;

                    if (result == null && ifNotExsits != null)
                    {
                        result = ifNotExsits(parent.Id);
                        var scopeId = sd.ScopeIdGetter(parent);
                        var policy = new Policy()
                        {
                            Checker = new VersionChecker(sd.Class, sd.ScopeClass, scopeId)
                        };

                        CacheInstance.SqlCe.Add(key, result, policy, className);
                    }
                }
            }

            return result;
        }

        ///// <summary>
        ///// 判断是否已经为指定的类启用了缓存。
        ///// </summary>
        ///// <param name="entityType"></param>
        ///// <returns></returns>
        //public static bool IsCacheEnabled(Type entityType)
        //{
        //    return CacheDefinition.Instance.TryGetScope(entityType, out _lastEntityDefCache);
        //}

        //public static void SetCache(Type entityType, Entity parent, IList<Entity> entityList)
        //{
        //    throw new NotImplementedException();//huqf to implement
        //    //if (entityType == null) throw new ArgumentNullException("entityType");
        //    //if (parent == null) throw new ArgumentNullException("parent");
        //    //if (entityList == null) throw new ArgumentNullException("entityList");

        //    //if (OEAEnvironment.IsOnClient())
        //    //{
        //    //    CacheScope sd = TryGetScope(entityType);

        //    //    if (sd != null)
        //    //    {
        //    //        var className = sd.Class.Name;
        //    //        var scopeId = sd.ScopeIdGetter(parent);
        //    //        var key = string.Format("ByParentId_{0}_{1}", className, parent.Id);
        //    //        var policy = new Policy()
        //    //        {
        //    //            Checker = new VersionChecker(sd.Class, sd.ScopeClass, scopeId)
        //    //        };

        //    //        Cache.Set(new CacheItem(key, entityList, className), policy);
        //    //    }
        //    //}
        //}

        /// <summary>
        /// 尝试从CacheDefinition中获取指定实体类型的缓存定义。
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        private CacheScope TryGetScope(Type entityType)
        {
            CacheScope sd = null;
            CacheDefinition.Instance.TryGetScope(entityType, out sd);
            return sd;
        }
    }
}