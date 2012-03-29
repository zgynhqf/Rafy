/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100330
 * 说明：所有实体集合类的基类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.3
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100330
 * 添加EntityTreeList和EntityList，并实现ReadFromTable方法 胡庆访 20100402
 * 添加两个非泛型的列表基类 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Linq;
using System.Diagnostics;
using SimpleCsla;
using OEA.Library.Caching;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// 所有实体集合类的基类
    /// 
    /// 注意，此类从GBusinessListBase(TList,TEntity)上继承过来，同时更改GBusinessListBase类的代码和它的某些约定！如下：
    /// 1. TList不再是最具体的子列表类，此参数已经作废。（正是这一点导致了列表类不能继承，这是一种反模式。）
    /// 2. TEntity不再是最具体的实体类，而是所有实体的基类Entity。
    ///     也就是说，它的使用方法类似IList(Entity)。
    ///     这一点的变更，使得所有用户对此列表进行遍历时，需要把每一个元素强制转换为具体的子类。这种遍历是弱类型的，较不安全，但是支持列表类的继承。
    /// </summary>
    [Serializable]
    public abstract partial class EntityList : IEntityOrListInternal
    {
        protected EntityList()
        {
            this.AllowNew = true;

            this.OnContructTree();
        }

        #region FindRepository

        [NonSerialized]
        private IRepository _repository;

        internal protected IRepository FindRepository()
        {
            if (this._repository == null)
            {
                var entityType = EntityConvention.EntityType(this.GetType());
                this._repository = RepositoryFactoryHost.Factory.Create(entityType);
            }

            return this._repository;
        }

        #endregion

        protected override object AddNewCore()
        {
            var item = this.FindRepository().New();
            this.Add(item);
            return item;
        }

        /// <summary>
        /// 是否需要关闭此行为：
        /// 在添加每一项时，设置父列表为当前列表，并设置它的父对象为本列表对象的父对象。
        /// </summary>
        public bool SupressSetItemParent { get; set; }

        protected override void InsertItem(int index, Entity item)
        {
            if (this.TreeRelationLoaded)
            {
                if (this.Contains(item)) throw new InvalidOperationException("当前列表中已经存在这个实体，添加操作不可用。");
            }

            var needParent = item != null && !this.SupressSetItemParent;
            if (needParent) { item.CastTo<IEntityOrList>().SetParent(this); }

            base.InsertItem(index, item);
            if (item.IsDeleted)
            {
                item.RevertDeleted();
                this._deletedList.Remove(item);
            }

            if (needParent)
            {
                var parentEntity = this.Parent;
                if (parentEntity != null)
                {
                    //有 ParentList.Parent 属性，则必然有 ParentProperty，本来可以直接调用以下方法进行设置，
                    //但是，由于一个类的集合，并不一定只是作为其父类的子集合（也就是说，可能作为其它类的引用集合），
                    //这时，父对象的类型并不匹配，所以需要进行尝试设置：
                    item.TrySetParentEntity(parentEntity);
                }
            }

            if (this.SupportTree) { this.OnTreeItemInserted(index, item); }
        }

        public Entity Parent
        {
            get { return (this as IEntityOrList).Parent as Entity; }
        }

        public Type EntityType
        {
            get { return this.FindRepository().EntityType; }
        }

        public void SetParentEntity(Entity entity)
        {
            //★Entity 有相同逻辑的代码，修改时请注意！！！
            var property = this.FindRepository().ParentPropertyIndicator;
            if (property == null) throw new NotSupportedException("请为父外键引用属性标记，传入 ReferenceType.Parent 的参数。");

            for (int i = 0, c = this.Count; i < c; i++)
            {
                var child = this[i];
                child.GetLazyRef(property).Entity = entity;
            }
        }

        public void TrySetParentEntity(Entity entity)
        {
            //★Entity 有相同逻辑的代码，修改时请注意！！！
            //如果父外键是懒加载外键，并且其对应的实体类型兼容 parent 的类型，才进行属性值设置。

            var property = this.FindRepository().ParentPropertyIndicator;
            if (property != null)
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ILazyEntityRef<>))
                {
                    if (entity == null || propertyType.GetGenericArguments()[0].IsAssignableFrom(entity.GetType()))
                    {
                        for (int i = 0, c = this.Count; i < c; i++)
                        {
                            var child = this[i];
                            child.GetLazyRef(property).Entity = entity;
                        }
                    }
                }
            }
        }

        #region Caching

        /// <summary>
        /// 些方法重写父类的实现，实现以下功能：
        /// 1.在父类实现中，是先删除，再更新其它结点。
        ///     这种实现中，如果先把子结点B向上级移动，然后再把父结点A删除；
        ///     在保存的时候如果成功删除A，则B由于级联删除的原因也会被删除。
        ///     所以这里子类实现时，是先更新，再删除。
        /// 2.删除结点时，考虑树结点的问题，先删除子结点，再删除父结点。
        /// </summary>
        /// <param name="parameters"></param>
        protected virtual void Child_Update(params object[] parameters)
        {
            this.NotifyCacheVersion();

            var oldRLCE = this.RaiseListChangedEvents;
            this.RaiseListChangedEvents = false;
            try
            {
                var deletedNodes = this.DeletedList.ToArray();
                //父结点肯定要在子结点的前面
                //暂时不起作用，等等GBusinessListBase.RemoveItem方法重构后生效
                //for (int i = 0, c = deletedNodes.Length; i < c; i++)
                //{
                //    var item = deletedNodes[i];
                //    var pid = item.Pid;
                //    if (pid != null)
                //    {
                //        for (int j = i + 1; j < c; j++)
                //        {
                //            var parent = deletedNodes[j];
                //            if (parent.Id == pid)
                //            {
                //                deletedNodes[i] = parent;
                //                deletedNodes[j] = item;
                //                break;
                //            }
                //        }
                //    }
                //}

                ////先从后面的子结点删除起。
                //for (int i = deletedNodes.Length - 1; i >= 0; i--)
                //{
                //    var child = deletedNodes[i];
                //    DataPortal.UpdateChild(child, parameters);
                //}

                for (int i = 0, c = deletedNodes.Length; i < c; i++)
                {
                    var child = deletedNodes[i];
                    DataPortal.UpdateChild(child, parameters);
                }

                this.DeletedList.Clear();

                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var child = this[i];
                    DataPortal.UpdateChild(child, parameters);
                }
            }
            finally
            {
                this.RaiseListChangedEvents = oldRLCE;
            }
        }

        /// <summary>
        /// 在本实体列表更新时，通知服务器更新对象的版本号。
        /// </summary>
        public void NotifyCacheVersion()
        {
            var entityType = this.EntityType;
            CacheScope scope = null;
            if (CacheDefinition.Instance.TryGetScope(entityType, out scope))
            {
                string scopeId = null;
                if (scope.ScopeIdGetter != null)
                {
                    var parent = this.Parent;
                    if (parent == null) throw new InvalidOperationException("此列表没有父对象，调用 NotifyVersion 方法失败。");
                    scopeId = scope.ScopeIdGetter(parent);
                }

                if (EntityListVersion.Repository != null)
                {
                    EntityListVersion.Repository.UpdateVersion(entityType, scope.ScopeClass, scopeId);
                }
            }
        }

        #endregion

        #region ReadRowDirectly

        /// <summary>
        /// 直接从table中读取列表对象的所有数据。
        /// 
        /// 注意：
        /// 如果需要把子对象的数据也加入进来，请使用本方法的另一重载。
        /// </summary>
        /// <param name="table"></param>
        internal protected void ReadFromTable(IDbTable table)
        {
            this.ReadFromTable(table, null);
        }

        /// <summary>
        /// 直接从table中读取列表对象的所有数据。
        /// </summary>
        /// <param name="table"></param>
        /// <param name="relationLoader">
        /// 为聚合对象item从table中加载其所有子对象的数据。
        /// 
        /// 注意：
        /// 默认不加载。
        /// 子类重写这个方法，手工调用TEntity的方法为其加载。
        /// 
        /// 参数：
        /// TEntity：需要加载孩子属性的实体对象
        /// IGTable：在这些行中加载所有子对象的数据
        /// 返回TEntity：加载完毕孩子后的实体对象。
        /// </param>
        internal protected void ReadFromTable(IDbTable table, Action<Entity, IDbTable> relationLoader)
        {
            EntityListHelper.ReadFromTable(this, table, relationLoader);
        }

        #endregion

        #region 值的复制

        /// <summary>
        /// 复制目标集合中的所有对象。
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="options"></param>
        public void Clone(EntityList targetList, CloneOptions options)
        {
            this.Clear();

            for (int i = 0, c = targetList.Count; i < c; i++)
            {
                var target = targetList[i];
                var src = this.AddNew();
                src.Clone(target, options);

                this.OnItemCloned(targetList, i);
            }

            this.NotifyLoaded(targetList._repository);
        }

        #endregion
    }

    /// <summary>
    /// 由于GEntityList和GEntityTreeList这两个类不能同时从一个类继承下来，
    /// 所以这个Helper类主要用于为它们共享一些代码。
    /// </summary>
    internal static class EntityListHelper
    {
        /// <summary>
        /// 这个方法把table中的数据全部读取并转换为对象存入对象列表中。
        /// 
        /// 算法简介：
        /// 由于子对象的数据都是存储在这个IGTable中，所以每一个TEntity可能对应多个行，
        /// 每一行数据其实就是一个子对象的数据，而TEntity的属性值是重复的。
        /// 所以这里找到每个TEntity对应的第一行和最后一行，把它封装为一个子表格，传给子对象集合进行加载。
        /// 这样的设计是为了实现重用这个方法：集合加载IGTable中的数据。
        /// </summary>
        /// <typeparam name="TCollection"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="list">转换的对象存入这个列表中</param>
        /// <param name="table">
        /// 表格数据，数据类型于以下形式：
        /// TableA  TableB  TableC  TableD...
        /// a1      b1      c1
        /// a1      b1      c2
        /// a2      b2      NULL
        /// a3      NULL    NULL
        /// ...
        /// </param>
        /// <param name="relationLoader">
        /// 为每个TEntity调用此方法，从IGTable中加载它对应的孩子对象。
        /// 加载完成后的对象会被加入到list中，所以此方法有可能返回一个全新的TEntity。
        /// </param>
        public static void ReadFromTable(this EntityList list, IDbTable table, Action<Entity, IDbTable> relationLoader)
        {
            list.RaiseListChangedEvents = false;
            var entityType = list.EntityType;
            string idName = RF.GetReadableColumnSql(entityType, DBConvention.FieldName_Id);

            int? lastId = null;
            //每个TEntity对象对应的第一行数据
            int startRow = 0;
            for (int i = 0, c = table.Count; i < c; i++)
            {
                var row = table[i];

                var objId = row[idName];
                int? id = objId != DBNull.Value ? (int)objId : (int?)null;

                //如果 id 改变，表示已经进入到下一个 TEntity 对象的开始行了。
                if (id != lastId)
                {
                    //不是第一次 或者 全是NULL值
                    if (lastId.HasValue)
                    {
                        //前一行就是最后一行。
                        int endRow = i - 1;

                        Entity item = CreateEntity(entityType, table, startRow, endRow, relationLoader);

                        list.Add(item);
                    }

                    //重置 startRow 为下一个 TEntity
                    startRow = i;
                }

                lastId = id;
            }

            //加入最后一个 Entity
            if (lastId.HasValue)
            {
                Entity lastEntity = CreateEntity(entityType, table, startRow, table.Count - 1, relationLoader);
                list.Add(lastEntity);
            }

            //完毕，退出
            list.RaiseListChangedEvents = true;
        }

        /// <summary>
        /// 把 table 从 startRow 到 endRow 之间的数据，都转换为一个 TEntity 并返回。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="table"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="relationLoader"></param>
        /// <returns></returns>
        private static Entity CreateEntity(Type entityType, IDbTable table, int startRow, int endRow, Action<Entity, IDbTable> relationLoader)
        {
            //新的TEntity
            var entity = RF.Create(entityType).GetFromRow(table[startRow]);
            Debug.Assert(entity != null, "id不为空，对象也不应该为空。");

            var childTable = new SubTable(table, startRow, endRow);
            if (relationLoader != null) { relationLoader(entity, childTable); }
            return entity;
        }
    }
}