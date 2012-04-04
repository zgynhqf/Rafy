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
using System.Linq;
using System.Text;
using OEA.ORM;
using OEA.Utils;
using OEA.Library.Caching;
using OEA.MetaModel;
using System.Data;


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
                // update child objects
                this.UpdateChildren();
            }
        }

        private void SyncChildrenPId()
        {
            foreach (var value in this.GetCompiledPropertyValues())
            {
                var children = value.Value as EntityList;
                if (children != null)
                {
                    children.TrySetParentEntity(null);
                    children.TrySetParentEntity(this);
                }
            }
        }

        protected virtual void OnUpdate()
        {
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

            // update child objects
            this.UpdateChildren();
        }

        protected virtual void OnDelete()
        {
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

        /// <summary>
        /// 重写这个方法，用于在从数据库获取出来时，及时地加载一些额外的属性。
        /// </summary>
        /// <param name="data"></param>
        internal protected virtual void OnDbLoaded() { }

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
