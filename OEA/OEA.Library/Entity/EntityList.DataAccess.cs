/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections;
using System.Linq;
using OEA.ORM;
using OEA.MetaModel;
using System.Linq.Expressions;
using OEA.MetaModel.Attributes;
using System.Collections.Generic;

namespace OEA.Library
{
    public abstract partial class EntityList
    {
        /// <summary>
        /// 子类可以重写这个方法，用于在获取时及时加载一些额外的属性。
        /// 本过程与根对象子对象无关。
        /// </summary>
        /// <param name="id"></param>
        protected virtual void QueryById(int id)
        {
            this.QueryDb(q => q.Constrain(Entity.IdProperty).Equal(id));
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的GetByParent方法的数据层代码。
        /// </summary>
        /// <param name="parentId"></param>
        protected virtual void QueryByParentId(int parentId)
        {
            var parentProperty = this.GetRepository().FindParentPropertyInfo(true);

            this.QueryDb(q => q.Constrain(parentProperty.ManagedProperty).Equal(parentId));
        }

        protected virtual void QueryByTreeParentCode(string treeCode)
        {
            //递归查找所有树型子
            var childCode = "%" + treeCode + "%" + this.GetRepository().TreeCodeOption.Seperator + "%";

            this.QueryDb(q => q.Constrain(Entity.TreeCodeProperty).Like(childCode));
        }

        /// <summary>
        /// 子类重写此方法，来实现自己的GetAll方法的数据层代码。
        /// </summary>
        protected virtual void QueryAll()
        {
            this.QueryDb(null as Action<IQuery>);
        }

        protected virtual void OnSaved() { }

        /// <summary>
        /// 使用 sql 语句加载对象
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="filter">对查询出来的对象进行内存级别的过滤器，默认为 null</param>
        protected void QueryDb(SqlWriter sql, Predicate<Entity> filter = null)
        {
            this.RaiseListChangedEvents = false;
            try
            {
                string formatSql = sql.ToString();

                using (var db = this.CreateDb())
                {
                    var list = filter == null ? this as ICollection<Entity> : new FilteredList(this, filter);
                    db.Select(this.EntityType, list, formatSql, sql.Parameters);
                }
            }
            finally
            {
                this.RaiseListChangedEvents = true;
            }
        }

        /// <summary>
        /// 使用 sql 语句加载对象
        /// </summary>
        /// <param name="formatSql">
        /// 格式化参数的 T-SQL。
        /// </param>
        protected void QueryDb(string formatSql, params object[] parameters)
        {
            Predicate<Entity> filter = null;
            this.QueryDb(formatSql, filter, parameters);
        }

        /// <summary>
        /// 使用 sql 语句加载对象
        /// </summary>
        /// <param name="formatSql">
        /// 格式化参数的 T-SQL。
        /// </param>
        /// <param name="filter">对查询出来的对象进行内存级别的过滤器，默认为 null</param>
        protected void QueryDb(string formatSql, Predicate<Entity> filter, params object[] parameters)
        {
            this.RaiseListChangedEvents = false;
            try
            {
                using (var db = this.CreateDb())
                {
                    var list = filter == null ? this as ICollection<Entity> : new FilteredList(this, filter);
                    db.Select(this.EntityType, list, formatSql, parameters);
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
        /// <param name="queryBuider">查询构造函数</param>
        /// <param name="filter">对查询出来的对象进行内存级别的过滤器，默认为 null</param>
        protected void QueryDb(Action<IQuery> queryBuider, Predicate<Entity> filter = null)
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
                    var list = filter == null ? this as ICollection<Entity> : new FilteredList(this, filter);
                    db.Select(query, list);
                }
            }
            finally
            {
                this.RaiseListChangedEvents = true;
            }
        }

        #region private class FilteredList

        private class FilteredList : ICollection<Entity>
        {
            private ICollection<Entity> _innerList;

            private Predicate<Entity> _filter;

            public FilteredList(ICollection<Entity> innerList, Predicate<Entity> filter)
            {
                this._innerList = innerList;
                this._filter = filter;
            }

            public void Add(Entity item)
            {
                if (this._filter == null || this._filter(item))
                {
                    this._innerList.Add(item);
                }
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(Entity item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(Entity[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(Entity item)
            {
                throw new NotImplementedException();
            }

            public IEnumerator<Entity> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        protected virtual void OnQueryDbOrder(IQuery query)
        {
            if (!query.HasOrdered)
            {
                //如果有OrderNo字段，则需要排序。
                var em = this.GetRepository().EntityMeta;
                if (em.DefaultOrderBy != null)
                {
                    query = query.Order(em.DefaultOrderBy.ManagedProperty, em.DefaultOrderByAscending);
                }
            }
        }

        protected void QueryBy(CommonPropertiesCriteria criteria)
        {
            this.QueryDb(q =>
            {
                var allProperties = this.GetRepository().PropertiesContainer.GetNonReadOnlyCompiledProperties();

                var first = true;
                foreach (var kv in criteria.Values)
                {
                    var property = allProperties.FirstOrDefault(mp => mp.Name == kv.Key);
                    var value = kv.Value;

                    if (!first)
                    {
                        if (criteria.Operator == CommonPropertiesOperator.And)
                        {
                            q.And();
                        }
                        else
                        {
                            q.Or();
                        }
                    }

                    q.Constrain(property).Equal(value);

                    first = false;
                }
            });
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
            this.QueryById(criteria.Id);
        }

        protected void QueryBy(GetByParentIdCriteria parentIdCriteria)
        {
            this.QueryByParentId(parentIdCriteria.Id);
        }

        protected void QueryBy(GetAllCriteria criteria)
        {
            this.QueryAll();
        }

        protected void QueryBy(GetByTreeParentCodeCriteria criteria)
        {
            this.QueryByTreeParentCode(criteria.TreeCode);
        }

        void IEntityOrListInternal.NotifySaved()
        {
            this.OnSaved();
        }


        #endregion
    }

    /// <summary>
    /// 用于一般性属性查询的条件
    /// </summary>
    [Serializable]
    public class CommonPropertiesCriteria
    {
        public CommonPropertiesCriteria()
        {
            this.Values = new Dictionary<string, object>();
            this.Operator = CommonPropertiesOperator.And;
        }

        /// <summary>
        /// 键值对
        /// </summary>
        public Dictionary<string, object> Values { get; private set; }

        /// <summary>
        /// 所有属性值使用的连接符
        /// </summary>
        public CommonPropertiesOperator Operator { get; set; }
    }

    /// <summary>
    /// 所有属性使用的连接符
    /// </summary>
    public enum CommonPropertiesOperator
    {
        And, Or
    }
}