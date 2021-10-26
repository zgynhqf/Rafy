/*******************************************************
 * 
 * 作者：LiteORM
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Domain.ORM.Linq;
using System.Linq.Expressions;
using System.IO;

namespace Rafy.Domain.ORM
{
    internal class PropertyQuery : IPropertyQuery, IPropertyQueryValue, IConstraintFactory
    {
        #region 字段

        private List<DbOrder> _orders;
        private IRepositoryInternal _repo;
        private IConstraintGroup _where;

        #endregion

        internal PropertyQuery(IRepositoryInternal repo)
        {
            this._orders = new List<DbOrder>(1);
            _repo = repo;
            _where = this.New();
        }

        #region 属性

        internal IRepositoryInternal Repo
        {
            get { return _repo; }
        }

        /// <summary>
        /// 可用的排序
        /// </summary>
        internal List<DbOrder> Orders
        {
            get { return _orders; }
        }

        /// <summary>
        /// 是否还没有任何语句
        /// </summary>
        public bool IsEmpty
        {
            get { return !this.HasWhere && !this.HasInnerJoin && !this.HasOrdered && PagingInfo.IsNullOrEmpty(this._pagingInfo); }
        }

        /// <summary>
        /// 是否已经有 Where 条件语句
        /// </summary>
        public bool HasWhere
        {
            get
            {
                return !_where.IsEmpty;
            }
        }

        /// <summary>
        /// 是否已经有了 OrderBy 语句
        /// </summary>
        public bool HasOrdered
        {
            get { return this._orders.Count > 0; }
        }

        /// <summary>
        /// 条件是否已经可以使用。
        /// 
        /// 当使用了 And、Or 并且还没有添加其它条件时，此属性会返回 false。
        /// </summary>
        public bool IsCompleted
        {
            get { return _pending == null && !_and.HasValue; }
        }

        /// <summary>
        /// 最终查询的条件。
        /// </summary>
        public IConstraintGroup Where
        {
            get { return _where; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _where = value;
            }
        }

        public IConstraintFactory ConstraintFactory
        {
            get { return this; }
        }

        #endregion

        #region 添加条件

        /// <summary>
        /// 是否使用 And 连接
        /// </summary>
        private bool? _and;
        /// <summary>
        /// 当前正在被定义查询的属性
        /// </summary>
        private IManagedProperty _pending;
        private Type _pendingOwner;

        public IPropertyQuery And()
        {
            _and = true;
            return this;
        }

        public IPropertyQuery Or()
        {
            _and = false;
            return this;
        }

        public IPropertyQueryValue Constrain(IManagedProperty property, Type propertyOwner = null)
        {
            //if (this.EntityType == null) throw new InvalidOperationException("要使用 IManagedProperty 作为查询参数，需要使用 IDb.Query(Type entityType) 接口。");
            if (_pending != null) { throw new InvalidOperationException("查询构造出错：你必须在 Constrain 方法之后调用值比较方法。"); }

            _pending = property;
            _pendingOwner = propertyOwner;

            return this;
        }

        public IPropertyQuery ConstrainSql(string formatSql, params object[] parameters)
        {
            var constraint = this.New(formatSql, parameters);
            AddConstraint(constraint);
            return this;
        }

        public IPropertyQuery In(IEnumerable values)
        {
            AddConstraint(PropertyCompareOperator.In, values);
            return this;
        }

        public IPropertyQuery NotIn(IEnumerable values)
        {
            AddConstraint(PropertyCompareOperator.NotIn, values);
            return this;
        }

        public IPropertyQuery Equal(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.Equal, val);
            return this;
        }

        public IPropertyQuery NotEqual(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.NotEqual, val);
            return this;
        }

        public IPropertyQuery Greater(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.Greater, val);
            return this;
        }

        public IPropertyQuery GreaterEqual(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.GreaterEqual, val);
            return this;
        }

        public IPropertyQuery Less(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.Less, val);
            return this;
        }

        public IPropertyQuery LessEqual(object val)
        {
            EnsureNotProperty(val);
            AddConstraint(PropertyCompareOperator.LessEqual, val);
            return this;
        }

        public IPropertyQuery Like(string val)
        {
            AddConstraint(PropertyCompareOperator.Like, val);
            return this;
        }

        public IPropertyQuery Contains(string val)
        {
            AddConstraint(PropertyCompareOperator.Contains, val);
            return this;
        }

        public IPropertyQuery StartWith(string val)
        {
            AddConstraint(PropertyCompareOperator.StartWith, val);
            return this;
        }

        public IPropertyQuery EndWith(string val)
        {
            AddConstraint(PropertyCompareOperator.EndWith, val);
            return this;
        }

        public IPropertyQuery Equal(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.Equal, property, propertyOwner);
            return this;
        }

        public IPropertyQuery NotEqual(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.NotEqual, property, propertyOwner);
            return this;
        }

        public IPropertyQuery Greater(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.Greater, property, propertyOwner);
            return this;
        }

        public IPropertyQuery GreaterEqual(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.GreaterEqual, property, propertyOwner);
            return this;
        }

        public IPropertyQuery Less(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.Less, property, propertyOwner);
            return this;
        }

        public IPropertyQuery LessEqual(IManagedProperty property, Type propertyOwner = null)
        {
            AddConstraint(PropertyCompareOperator.LessEqual, property, propertyOwner);
            return this;
        }

        private void AddConstraint(PropertyCompareOperator op, object val)
        {
            if (_pending == null)
            {
                throw new InvalidOperationException("查询构造出错：你必须先调用方法 Constrain。");
            }

            //如果没有指定属性对应的实体类型，则使用当前仓库对应的实体来作为属性的拥有者。
            var propertyOwner = _pendingOwner;
            if (propertyOwner == null)
            {
                propertyOwner = _pending.OwnerType;
                if (_repo.EntityType.IsSubclassOf(propertyOwner))
                {
                    propertyOwner = _repo.EntityType;
                }
            }

            var constraint = new PropertyConstraint
            {
                Context = this,
                Property = _pending,
                ConcreteType = propertyOwner,
                Operator = op,
                Value = val
            };
            this.AddConstraint(constraint);

            _pending = null;
            _pendingOwner = null;
        }

        private void AddConstraint(PropertyCompareOperator op, IManagedProperty property, Type propertyOwner = null)
        {
            if (_pending == null)
            {
                throw new InvalidOperationException("查询构造出错：你必须先调用方法 Constrain。");
            }

            //如果没有指定属性对应的实体类型，则使用当前仓库对应的实体来作为属性的拥有者。
            var leftPropertyOwner = _pendingOwner;
            if (leftPropertyOwner == null)
            {
                leftPropertyOwner = _pending.OwnerType;
                if (_repo.EntityType.IsSubclassOf(leftPropertyOwner))
                {
                    leftPropertyOwner = _repo.EntityType;
                }
            }

            var constraint = new TwoPropertiesConstraint
            {
                Context = this,
                LeftProperty = _pending,
                LeftPropertyOwner = leftPropertyOwner,
                Operator = op,
                RightProperty = property,
                RightPropertyOwner = propertyOwner
            };
            this.AddConstraint(constraint);

            _pending = null;
            _pendingOwner = null;
        }

        private void AddConstraint(IConstraintGroup constraint)
        {
            if (_and.GetValueOrDefault())
            {
                _where = _where.And(constraint);
            }
            else
            {
                _where = _where.Or(constraint);
            }

            _and = null;
        }

        private static void EnsureNotProperty(object val)
        {
            if (val is IProperty) throw new NotSupportedException("不支持使用");
        }

        #endregion

        #region 关联查询（引用属性查询）

        private IList<RefTableProperty> _refItems;

        /// <summary>
        /// 是否已经有 Inner Join 条件语句
        /// </summary>
        public bool HasInnerJoin
        {
            get { return _refItems != null && _refItems.Count > 0; }
        }

        internal IList<RefTableProperty> RefItems
        {
            get { return this._refItems; }
        }

        public IPropertyQuery JoinRef(IRefProperty property, Type propertyOwner = null)
        {
            if (propertyOwner == null) propertyOwner = property.OwnerType;

            if (_refItems == null) { _refItems = new List<RefTableProperty>(2); }

            if (_refItems.All(i => i.RefProperty != property.RefIdProperty || i.PropertyOwner != propertyOwner))
            {
                var item = new RefTableProperty(property.RefIdProperty, propertyOwner)
                {
                    JoinRefType = JoinRefType.JoinOnly
                };
                _refItems.Add(item);
            }

            return this;
        }

        #endregion

        #region 排序

        public IPropertyQuery OrderBy(IManagedProperty property, OrderDirection direction)
        {
            DbOrder o = new DbOrder(property, direction == OrderDirection.Ascending);
            _orders.Add(o);
            return this;
        }

        #endregion

        //#region SQL 生成

        ///// <summary>
        ///// 如果有关联查询，则此方法返回生成的 INNER JOIN 语句。
        ///// </summary>
        ///// <param name="sql">The SQL.</param>
        ///// <param name="table">正在被查询的表</param>
        //internal void AppendSqlJoin(TextWriter sql, DbTable table)
        //{
        //    if (this.HasInnerJoin)
        //    {
        //        foreach (var refItem in _refItems)
        //        {
        //            DbTable mainTable = refItem.OwnerTable;
        //            DbTable refTable = refItem.RefTable;
        //            string fkName = refItem.FKName;

        //            if (refItem.RefProperty.Nullable)
        //            {
        //                sql.Write("    LEFT OUTER JOIN ");
        //            }
        //            else
        //            {
        //                sql.Write("    INNER JOIN ");
        //            }
        //            sql.AppendQuoteName(refTable).Write(" ON ");
        //            sql.AppendQuoteName(mainTable).Write(".");
        //            sql.AppendQuote(mainTable, fkName).Write(" = ");
        //            sql.AppendQuoteName(refTable).Write(".");
        //            sql.AppendQuote(refTable, refTable.IdColumn.Name);
        //            sql.WriteLine();
        //        }
        //    }
        //}

        ///// <summary>
        ///// 此方法在指定字符串中生成 WHERE 语句。
        ///// </summary>
        ///// <param name="sql">正在被拼接的 sql</param>
        ///// <param name="mainTable">正在被查询的表</param>
        ///// <param name="parameters">The parameters.</param>
        ///// <param name="appendWhereClause">if set to <c>true</c> [append where clause].</param>
        //internal void AppendSqlWhere(TextWriter sql, DbTable mainTable, FormattedSqlParameters parameters, bool appendWhereClause = true)
        //{
        //    if (!_where.IsEmpty)
        //    {
        //        if (appendWhereClause) { sql.Write("WHERE "); }

        //        (_where as Constraint).GetSql(sql, parameters);
        //    }
        //}

        ///// <summary>
        ///// 此方法在指定字符串中生成 OrderBy 语句。
        ///// </summary>
        ///// <param name="sql"></param>
        ///// <param name="mainTable"></param>
        ///// <param name="appendOrderByClause">是否添加 " ORDER BY "</param>
        //internal void AppendSqlOrder(TextWriter sql, DbTable mainTable, bool appendOrderByClause = true)
        //{
        //    //生成 ORDER BY
        //    if (this._orders.Count > 0)
        //    {
        //        if (appendOrderByClause)
        //        {
        //            sql.WriteLine();
        //            sql.Write("ORDER BY ");
        //        }

        //        for (int i = 0, oc = this._orders.Count; i < oc; i++)
        //        {
        //            if (i > 0) { sql.Write(','); }

        //            var order = _orders[i];
        //            order.GetSql(sql, mainTable);
        //        }
        //    }
        //}

        //#endregion

        #region 分页

        private PagingInfo _pagingInfo = PagingInfo.Empty;

        /// <summary>
        /// 如果这个属性为空，或者它是 <see cref="Rafy.PagingInfo.Empty"/>，表示不需要进行分页。
        /// </summary>
        public PagingInfo PagingInfo
        {
            get { return this._pagingInfo; }
        }

        /// <summary>
        /// 使用指定的条件来进行分页。
        /// </summary>
        /// <param name="pagingInfo"></param>
        public void Paging(PagingInfo pagingInfo)
        {
            this._pagingInfo = pagingInfo;
        }

        #endregion

        #region 贪婪加载

        internal List<ConcreteProperty> EagerLoadProperties;

        void IPropertyQuery.EagerLoad(IProperty property, Type propertyOwner)
        {
            propertyOwner = propertyOwner ?? property.OwnerType;

            if (this.EagerLoadProperties == null)
            {
                this.EagerLoadProperties = new List<ConcreteProperty>();
            }

            this.EagerLoadProperties.Add(new ConcreteProperty(property, propertyOwner));
        }

        #endregion

        #region IConstraintGroup

        public IConstraintGroup New(PropertyComparisonExpression exp)
        {
            return new PropertyConstraint(exp)
            {
                Context = this
            };
        }

        public IConstraintGroup New(string formatSql, params object[] parameters)
        {
            return new SqlWhereConstraint
            {
                Context = this,
                FormatSql = formatSql,
                Parameters = parameters
            };
        }

        public IConstraintGroup New()
        {
            return new EmptyConstraint { Context = this };
        }

        #endregion

        #region Linq

        public IPropertyQuery CombineLinq(IQueryable queryable)
        {
            return CombineLinq(queryable.Expression);
        }

        private IPropertyQuery CombineLinq(Expression expression)
        {
            expression = Evaluator.PartialEval(expression);

            var builder = new QueryBuilder { _query = this };
            builder.BuildQuery(expression);

            return this;
        }

        #endregion

        #region 帮助方法

        /// <summary>
        /// 在查询时，对所有用到的表进行缓存。
        /// </summary>
        private Dictionary<Type, RdbTable> _tablesCache = new Dictionary<Type, RdbTable>();

        internal void ErrorIfNotCompleted()
        {
            if (!this.IsCompleted) { throw new InvalidOperationException("查询条件构造未完成！"); }
        }

        /// <summary>
        /// 在当前查询所有可使用的表中检索指定属性拥有者对应的表。
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyOwner"></param>
        /// <returns></returns>
        internal RdbTable GetPropertyTable(IManagedProperty property, Type propertyOwner)
        {
            return ConditionalSql.GetPropertyTable(property, propertyOwner, this._tablesCache);
        }

        #endregion
    }
}