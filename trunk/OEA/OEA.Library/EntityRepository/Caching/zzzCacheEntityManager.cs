//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OEA.MetaModel;

//namespace OEA.Library.Caching
//{
//    internal interface IEntityManager
//    {
//        bool IsRootType(Type entityType);

//        Entity GetById(Type entityType, Guid id);

//        EntityList GetAll(Type listType);

//        EntityList Convert(IList<Entity> table);
//    }

//    public class CacheEntityManager
//    {
//        public static readonly CacheEntityManager Instance = new CacheEntityManager();

//        private IEntityManager _manager;

//        private CacheEntityManager() { }

//        private bool IsOnClient()
//        {
//            return OEAEnvironment.IsOnClient();
//        }

//        private bool IsCacheEnabled(Type entityType)
//        {
//            return this.IsOnClient() && CacheDefinition.Instance.IsEnabled(entityType);
//        }

//        public EntityList CacheAll(Type listType)
//        {
//            //如果是在客户端，则使用缓存。
//            if (this.IsOnClient())
//            {
//                var entityType = EntityConvention.EntityType(listType);

//                var table = EntityRowCache.Instance.CacheAll(entityType, () => this._manager.GetAll(listType));

//                if (table != null)
//                {
//                    return this._manager.Convert(table);
//                    //var isRoot = EM.IsRootType(entityType);

//                    //var result = isRoot ? GetRoots(listType) : GetChild(listType);
//                    //result.RaiseListChangedEvents = false;

//                    //for (int i = 0, c = table.Count; i < c; i++)
//                    //{
//                    //    var row = table[i];
//                    //    result.Add(EM.ConvertEntity(row, isRoot));
//                    //}

//                    //result.RaiseListChangedEvents = true;

//                    //return result;
//                }
//            }

//            return this._manager.GetAll(listType);
//        }

//        public Entity CacheById(Type entityType, Guid id)
//        {
//            if (this.IsCacheEnabled(entityType))
//            {
//                var isRoot = this._manager.IsRootType(entityType);
//                if (isRoot)
//                {
//                    return AggregateRootCache.Instance.CacheById(entityType, id, i => this._manager.GetById(entityType, id));
//                }
//                else
//                {
//                    return this.CacheChildById(entityType, id);
//                }
//            }

//            return this._manager.GetById(entityType, id);
//        }

//        public Entity CacheChildById(Type entityType, Guid id)
//        {
//            Entity result = null;

//            //如果是在客户端，则使用缓存。
//            if (OEAEnvironment.IsOnClient())
//            {
//                var listType = EntityConvention.ListType(entityType);

//                var table = EntityRowCache.Instance.CacheAll(entityType, () => this._manager.GetAll(listType));

//                var entity = table.FirstOrDefault(e => e.Id == id);

//                if (entity != null)
//                {
//                    result = EM.ConvertEntity(entity);
//                }
//            }

//            return result;
//        }
//    }
//}
