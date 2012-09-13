/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using OEA.Library.Caching;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.ORM;
using OEA.Utils;

namespace OEA.Library
{
    public partial class Entity : IEntityOrListInternal
    {
        #region IsDataAccessing

        /// <summary>
        /// 此属性表示是否这个实体正在执行数据访问操作。
        /// 
        /// 一般使用场景：
        /// 一些数据属性的获取或设置根据此状态来实现不同的行为。
        /// </summary>
        public bool IsDataAccessing
        {
            get { return this._isDataAccessing; }
        }

        /// <summary>
        /// 此字段表明当前实体是否正在被数据层操作中（Load/Save）。
        /// </summary>
        [NonSerialized]
        private bool _isDataAccessing;

        /// <summary>
        /// 从 EntityRepository 中加载完成，并从中返回时，都会执行此方法。
        /// </summary>
        internal void NotifyLoaded(IRepository repository)
        {
            this._repository = repository;

            if (this.Status != PersistenceStatus.New)
            {
                this.Status = PersistenceStatus.Unchanged;
            }
        }

        #endregion

        #region 数据层实现

        /// <summary>
        /// 是否启用批量插入。默认为 false
        /// </summary>
        protected virtual bool EnableBatchInsert
        {
            get
            {
                return false;
                //return this.FindRepository().EntityInfo.EntityCategory == EntityCategory.Root;
            }
        }

        protected virtual void OnInsert()
        {
            try
            {
                this._isDataAccessing = true;

                this.InsertCore();

                this.SyncChildrenPId();
            }
            finally
            {
                this._isDataAccessing = false;
            }

            if (this.EnableBatchInsert)
            {
                //根对象使用批插入
                var reader = new EntityChldrenBatchReader(this);
                var dic = reader.Read();

                foreach (var kv in dic)
                {
                    var repository = RepositoryFactoryHost.Factory.Create(kv.Key);
                    repository.AddBatch(kv.Value);

                    if (EntityListVersion.Repository != null)
                    {
                        EntityListVersion.Repository.UpdateVersion(kv.Key);
                    }
                }
            }
            else
            {
                this.UpdateChildren();
            }
        }

        private void SyncChildrenPId()
        {
            foreach (var field in this.GetLoadedChildren())
            {
                var children = field.Value as EntityList;
                children.SyncParentEntityId(this);
            }
        }

        protected virtual void OnUpdate()
        {
            this.UpdateRedundanciesIf();

            //如果是聚合子对象发生改变，而当前对象没有改变时，则不需要更新当前对象。
            if (this.IsSelfDirty)
            {
                try
                {
                    this._isDataAccessing = true;

                    this.UpdateCore();
                }
                finally
                {
                    this._isDataAccessing = false;
                }
            }

            this.UpdateChildren();
        }

        protected virtual void OnDelete()
        {
            //根对象默认使用级联删除，所以不需要更新聚合子，直接删除本对象即可。
            try
            {
                this._isDataAccessing = true;

                this.DeleteCore();
            }
            finally
            {
                this._isDataAccessing = false;
            }
        }

        private void InsertCore()
        {
            this.NotifyCacheVersion();

            using (var db = this.CreateDb())
            {
                db.Insert(this);
            }
        }

        private void UpdateCore()
        {
            this.NotifyCacheVersion();

            using (var db = this.CreateDb())
            {
                db.Update(this);
            }
        }

        private void DeleteCore()
        {
            this.NotifyCacheVersion();

            using (var db = this.CreateDb())
            {
                db.Delete(this);
            }
        }

        protected virtual void OnSaved() { }

        #endregion

        #region 冗余属性处理

        /// <summary>
        /// 是否在更新本行数据时，同时更新所有依赖它的冗余属性。
        /// </summary>
        private bool _updateRedundancies = false;

        /// <summary>
        /// 在属性变更时，如果该属性在某个冗余路径中，则应该使用冗余更新策略。
        /// </summary>
        /// <param name="e"></param>
        private void NotifyIfInRedundancyPath(IManagedPropertyChangedEventArgs e)
        {
            var property = e.Property as IProperty;
            if (property.IsInRedundantPath)
            {
                var refProperty = property as IRefProperty;
                if (refProperty != null)
                {
                    foreach (var path in refProperty.InRedundantPathes)
                    {
                        if (path.RefPathes[0] == refProperty)
                        {
                            //如果是第一个，说明冗余属性和这个引用属性是在当前类型中声明的，
                            //此时，直接更新冗余属性的值。
                            object value = this.GetRedundancyValue(path);
                            this.SetProperty(path.Redundancy, value);
                        }
                        else
                        {
                            //延迟到更新数据库行时，才更新其它表的冗余属性
                            this._updateRedundancies = true;
                        }
                    }
                }
                else
                {
                    this._updateRedundancies = true;
                }
            }
        }

        /// <summary>
        /// 尝试更新冗余属性值。
        /// </summary>
        private void UpdateRedundanciesIf()
        {
            if (!this._updateRedundancies) return;

            //如果有一些在冗余属性路径中的属性的值改变了，则开始更新数据库的中的所有冗余字段的值。
            var repository = this.GetRepository();
            Entity dbEntity = null;
            var propertiesInPath = repository.GetPropertiesInRedundancyPath();
            for (int i = 0, c = propertiesInPath.Count; i < c; i++)
            {
                var property = propertiesInPath[i];

                //如果只有一个属性，那么就是它变更引起的更新
                //否则，需要从数据库获取原始值来对比检测具体哪些属性值变更，然后再发起冗余更新。
                bool isChanged = c == 1;

                var refProperty = property as IRefProperty;
                if (refProperty != null)
                {
                    if (!isChanged)
                    {
                        if (dbEntity == null) dbEntity = repository.GetById(this.Id);
                        var dbRef = dbEntity.GetLazyRef(refProperty);
                        var newRef = this.GetLazyRef(refProperty);
                        isChanged = dbRef.NullableId != newRef.NullableId;
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            //如果是引用变更了，并且只有一个 RefPath，则不需要处理。
                            //因为这个已经在属性变更时实时处理过了。
                            if (path.RefPathes.Count > 1)
                            {
                                this.UpdateRedundancyByIntermidateRef(path, refProperty);
                            }
                        }
                    }
                }
                else
                {
                    var newValue = this.GetProperty(property);

                    if (!isChanged)
                    {
                        if (dbEntity == null) dbEntity = repository.GetById(this.Id);
                        var dbValue = dbEntity.GetProperty(property);
                        isChanged = !object.Equals(dbValue, newValue);
                    }

                    if (isChanged)
                    {
                        foreach (var path in property.InRedundantPathes)
                        {
                            UpdateRedundancyByValue(path, newValue);
                        }
                    }
                }
            }

            this._updateRedundancies = false;
        }

        /// <summary>
        /// 值改变时引发的冗余值更新操作。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="newValue"></param>
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        private void UpdateRedundancyByValue(RedundantPath path, object newValue)
        {
            UpdateRedundancy(path.Redundancy, newValue, path.RefPathes, this.Id);
        }

        /// <summary>
        /// 冗余路径中非首位的引用属性变化时引发的冗余值更新操作。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="refChanged">该引用属性值变化了</param>
        private void UpdateRedundancyByIntermidateRef(RedundantPath path, IRefProperty refChanged)
        {
            var newValue = this.GetRedundancyValue(path, refChanged);

            //只要从开始到 refChanged 前一个
            var refPathes = new List<IRefProperty>(5);
            foreach (var refProperty in path.RefPathes)
            {
                if (refProperty == refChanged) break;
                refPathes.Add(refProperty);
            }

            this.UpdateRedundancy(path.Redundancy, newValue, refPathes, this.Id);
        }

        /// <summary>
        /// 更新某个冗余属性
        /// </summary>
        /// <param name="redundancy">更新指定的冗余属性</param>
        /// <param name="newValue">冗余属性的新值</param>
        /// <param name="refPathes">
        /// 从冗余属性声明类型开始的一个引用属性集合，
        /// 将会为这个集合路径生成更新的 Where 条件。
        /// </param>
        /// <param name="lastRefId">引用路径中最后一个引用属性对应的值。这个值将会作为 Where 条件的值。</param>
        private void UpdateRedundancy(IProperty redundancy, object newValue, IList<IRefProperty> refPathes, int lastRefId)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 假定场景是：
             * refPathes: D(CRef,AName) -> C(BRef) -> B(ARef)，
             * lastRefId: AId。（B.ARef 是最后一个引用属性）
             * 则对应生成的 SQL 是：
             * update D set AName = @AName where CId in (
             *     select id from C where BId in (
             *          select id from B where AId = @AId
             *     )
             * )
             * 如果 B、C 表也有冗余属性，对应的 SQL 则是：
             * update B Set AName = @AName where AId = @BId
             * update C set AName = @AName where BId in (
             *     select id from B where AId = @AId
             * )
             * 
            **********************************************************************/

            //准备所有用到的 DbTable
            var table = DbTableHost.TableFor(redundancy.OwnerType);
            var refTables = new RefPropertyTable[refPathes.Count];
            for (int i = 0, c = refPathes.Count; i < c; i++)
            {
                var refProperty = refPathes[i];
                refTables[i] = new RefPropertyTable
                {
                    RefProperty = refProperty,
                    OwnerTable = DbTableHost.TableFor(refProperty.OwnerType)
                };
            }

            var sql = new SqlWriter();
            //SQL: UPDATE D SET AName = {0} WHERE
            sql.Append("UPDATE ").Append(table.QuoteName)
                .Append(" SET ").Append(table.Quote(table.Translate(redundancy)))
                .Append(" = ").AppendParameter(newValue).Append(" WHERE ");

            int quoteNeeded = 0;
            if (refTables.Length > 1)
            {
                //中间的都生成 Where XX in
                var inWherePathes = refTables.Take(refTables.Length - 1).ToArray();
                for (int i = 0; i < inWherePathes.Length; i++)
                {
                    var inRef = inWherePathes[i];

                    //SQL: CId In (
                    var columnName = table.Quote(inRef.OwnerTable.Translate(inRef.RefProperty));
                    sql.Append(columnName).Append(" IN (").WriteLine();
                    quoteNeeded++;

                    var nextRef = refTables[i + 1];

                    //SQL: SELECT Id FROM C WHERE 
                    sql.Append(" SELECT ").Append(nextRef.OwnerTable.Quote(DBConvention.FieldName_Id))
                        .Append(" FROM ").Append(nextRef.OwnerTable.QuoteName)
                        .Append(" WHERE ");
                }
            }

            //最后一个，生成SQL: BId = {1}
            var lastRef = refTables[refTables.Length - 1];
            var lastRefColumnName = table.Quote(lastRef.OwnerTable.Translate(lastRef.RefProperty));
            sql.Append(lastRefColumnName).Append(" = ").AppendParameter(lastRefId);

            while (quoteNeeded > 0)
            {
                sql.WriteLine(")");
                quoteNeeded--;
            }

            //执行最终的 SQL 语句
            using (var db = this.CreateDb())
            {
                db.DBA.ExecuteText(sql, sql.Parameters);
            }
        }

        /// <summary>
        /// 根据冗余路径从当前对象开始搜索，获取真实的属性值。
        /// </summary>
        /// <param name="path"></param>
        /// <param name="from">
        /// 本对象在路径中拥有的引用属性。
        /// 在 D->C->B->A.Name 场景中，当前对象（this）可能是 C，那么 from 就是 C.BRefProperty.
        /// 如果没有指定此属性，则表示从第一个开始。
        /// </param>
        /// <returns></returns>
        private object GetRedundancyValue(RedundantPath path, IRefProperty from = null)
        {
            Entity refEntity = this;
            foreach (var refP in path.RefPathes)
            {
                if (from != null && refP != from) continue;

                refEntity = refEntity.GetLazyRef(refP).Entity;
                if (refEntity == null) break;
            }

            object value = null;
            if (refEntity != this && refEntity != null) value = refEntity.GetProperty(path.ValueProperty);

            return value;
        }

        /// <summary>
        /// 某个引用属性与其所在类对应的表元数据
        /// </summary>
        private struct RefPropertyTable
        {
            public IRefProperty RefProperty;
            public DbTable OwnerTable;
        }

        #endregion

        /// <summary>
        /// 重写这个方法，用于在从数据库获取出来时，及时地加载一些额外的属性。
        /// 
        /// 注意：这个方法中只应该为一般属性计算值，不能有其它的数据访问。
        /// </summary>
        /// <param name="data"></param>
        internal protected virtual void OnDbLoaded() { }

        protected void LoadRefProperty(IRefProperty refProperty)
        {
            //暂时只是获取 Entity，以后可以使用其它方法优化此实现。
            var load = this.GetLazyRef(refProperty).Entity;
        }

        /// <summary>
        /// 数据库配置名称（每个库有一个唯一的配置名）
        /// </summary>
        internal protected virtual string ConnectionStringSettingName
        {
            get { return ConnectionStringNames.OEAPlugins; }
        }

        void IEntityOrListInternal.NotifySaved()
        {
            this.OnSaved();
        }
    }
}
