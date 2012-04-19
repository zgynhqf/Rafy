using System;
using System.Collections;
using System.Linq;
using OEA.ORM;
using OEA.MetaModel;
using System.Linq.Expressions;
using OEA.MetaModel.Attributes;


namespace OEA.Library
{
    public abstract partial class EntityList
    {
        /// <summary>
        /// 子类可以重写这个方法，用于在获取时及时加载一些额外的属性。
        /// 本过程与根对象子对象无关。
        /// </summary>
        /// <param name="id"></param>
        protected virtual void OnGetById(int id)
        {
            this.QueryDb(q => q.Constrain(DBConvention.FieldName_Id).Equal(id));
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的GetByParent方法的数据层代码。
        /// </summary>
        /// <param name="parentId"></param>
        protected virtual void OnGetByParentId(int parentId)
        {
            var parentProperty = this.GetRepository().FindParentPropertyInfo(true);

            this.QueryDb(q => q.Constrain(parentProperty.Name).Equal(parentId));
        }

        protected virtual void OnGetByTreeParentCode(string treeCode)
        {
            //递归查找所有树型子
            var childCode = treeCode + "%" + this.GetRepository().TreeCodeOption.Seperator;

            this.QueryDb(q => q.Constrain(DBConvention.FieldName_TreeCode).Like(childCode));
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的GetAll方法的数据层代码。
        /// </summary>
        protected virtual void OnGetAll()
        {
            this.QueryDb(null as Action<IQuery>);
        }

        protected virtual void OnSaved() { }

        /// <summary>
        /// 使用 sql 语句加载对象
        /// </summary>
        /// <param name="sql">
        /// 尽量使用 T-SQL。
        /// </param>
        protected void QueryDb(string sql)
        {
            this.RaiseListChangedEvents = false;
            try
            {
                using (var db = this.CreateDb())
                {
                    //访问数据库
                    var table = db.ExecSql(this.EntityType, sql);

                    //读取数据
                    this.AddTable(table);
                }
            }
            finally
            {
                this.RaiseListChangedEvents = true;
            }
        }

        /// <summary>
        /// 访问数据库，把指定的实体类 entityType 满足 queryBuider 条件的所有实体查询出来，并直接加到本列表中。
        /// </summary>
        /// <param name="entityType"></param>
        protected void QueryDb(Action<IQuery> queryBuider)
        {
            this.RaiseListChangedEvents = false;
            try
            {
                using (var db = this.CreateDb())
                {
                    var query = db.Query(this.EntityType);
                    if (queryBuider != null) queryBuider(query);

                    this.OnQueryDbOrder(query);

                    //访问数据库
                    var table = db.Select(query);

                    //读取数据
                    this.AddTable(table);
                }
            }
            finally
            {
                this.RaiseListChangedEvents = true;
            }
        }

        protected virtual void OnQueryDbOrder(IQuery query)
        {
            if (!query.HasOrdered)
            {
                //如果有OrderNo字段，则需要排序。
                var em = this.GetRepository().EntityMeta;
                if (em.DefaultOrderBy != null)
                {
                    query = query.Order(em.DefaultOrderBy.Name, em.DefaultOrderByAscending);
                }
            }
        }

        public void AddTable(IEnumerable entities)
        {
            foreach (Entity entity in entities)
            {
                entity.OnDbLoaded();
                this.Add(entity);
            }
        }

        #region QueryBy

        private void Child_Fetch() { }

        protected virtual void QueryBy(object criteria)
        {
            throw new NotImplementedException(string.Format(
                "实体列表类需要编写对应 {0} 类型的 QueryBy 数据层方法完成数据访问逻辑。",
                criteria.GetType().FullName
                ));
        }

        protected void QueryBy(GetByIdCriteria criteria)
        {
            this.OnGetById(criteria.Id);
        }

        protected void QueryBy(GetByParentIdCriteria parentIdCriteria)
        {
            this.OnGetByParentId(parentIdCriteria.Id);
        }

        protected void QueryBy(GetAllCriteria criteria)
        {
            this.OnGetAll();
        }

        protected void QueryBy(GetByTreeParentCodeCriteria criteria)
        {
            this.OnGetByTreeParentCode(criteria.TreeCode);
        }

        void IEntityOrListInternal.NotifySaved()
        {
            this.OnSaved();
        }

        #endregion
    }
}
