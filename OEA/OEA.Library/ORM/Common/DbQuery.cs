using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using hxy.Common.Data;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA.ORM
{
    internal class DbQuery : IQuery, IQueryValue
    {
        #region 字段

        private List<IWhereConstraint> _constraints;

        private IList<RefTableProperty> _refItems;

        protected List<string> _operators;

        protected List<DbOrder> _orders;

        private IManagedProperty _pending;

        #endregion

        public DbQuery(Type entityType)
        {
            this._constraints = new List<IWhereConstraint>(3);
            this._refItems = new List<RefTableProperty>(2);
            this._operators = new List<string>(2);
            this._orders = new List<DbOrder>(1);

            this.EntityType = entityType;
        }

        #region 属性

        /// <summary>
        /// 一般条件
        /// </summary>
        protected List<IWhereConstraint> Constraints
        {
            get { return this._constraints; }
        }

        /// <summary>
        /// 连接 Constraints 的连接符列表。
        /// 也就是说：Operators.Count + 1 = Constraints.Count。
        /// </summary>
        protected List<string> Operators
        {
            get { return _operators; }
        }

        /// <summary>
        /// 可用的排序
        /// </summary>
        protected List<DbOrder> Orders
        {
            get { return _orders; }
        }

        /// <summary>
        /// 当前正在被定义查询的属性
        /// </summary>
        protected IManagedProperty Pending
        {
            get { return _pending; }
        }

        /// <summary>
        /// 当前正在查询的实体类型
        /// </summary>
        public Type EntityType { get; private set; }

        /// <summary>
        /// 是否还没有任何语句
        /// </summary>
        public bool IsEmpty
        {
            get { return !this.HasWhere && !this.HasInnerJoin && !this.HasOrdered; }
        }

        /// <summary>
        /// 是否已经有 Where 条件语句
        /// </summary>
        public bool HasWhere
        {
            get { return this._constraints.Count > 0; }
        }

        /// <summary>
        /// 是否已经有 Inner Join 条件语句
        /// </summary>
        public bool HasInnerJoin
        {
            get { return this._refItems.Count > 0; }
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
            get
            {
                int c = this._constraints.Count;
                int o = this._operators.Count;
                return this._pending == null &&
                    (((c == 0 || c == 1) && o == 0) || ((c - 1) == o));
            }
        }

        #endregion

        #region 添加条件

        public IQueryValue Constrain(IManagedProperty property)
        {
            //if (this.EntityType == null) throw new InvalidOperationException("要使用 IManagedProperty 作为查询参数，需要使用 IDb.Query(Type entityType) 接口。");

            ErrorIfPending();

            _pending = property;

            return this;
        }

        public IQuery ConstrainSql(string formatSql, params object[] parameters)
        {
            this._constraints.Add(new SqlWhereConstraint
            {
                FormatSql = formatSql,
                Parameters = parameters
            });

            return this;
        }

        public IQuery In(IList values)
        {
            var c = new InConstraint(this.Pending, "in", values);
            AddConstraint(c);
            return this;
        }

        public IQuery NotIn(IList values)
        {
            var c = new InConstraint(this.Pending, "not in", values);
            AddConstraint(c);
            return this;
        }

        public IQuery Equal(object val)
        {
            if (val == null || val == DBNull.Value)
            {
                AddConstraint(new NullConstraint(this.Pending, true));
            }
            else
            {
                AddConstraint("=", val);
            }

            return this;
        }

        public IQuery NotEqual(object val)
        {
            if (val == null || val == DBNull.Value)
            {
                AddConstraint(new NullConstraint(this.Pending, false));
            }
            else
            {
                AddConstraint("!=", val);
            }

            return this;
        }

        public IQuery Greater(object val)
        {
            ErrorIfNull(val);
            AddConstraint(">", val);
            return this;
        }

        public IQuery GreaterEqual(object val)
        {
            ErrorIfNull(val);
            AddConstraint(">=", val);

            return this;
        }

        public IQuery Less(object val)
        {
            ErrorIfNull(val);
            AddConstraint("<", val);
            return this;
        }

        public IQuery LessEqual(object val)
        {
            ErrorIfNull(val);
            AddConstraint("<=", val);
            return this;
        }

        public IQuery Like(string val)
        {
            ErrorIfNull(val);
            AddConstraint(" like ", val);

            return this;
        }

        public IQuery Contains(string val)
        {
            return this.Like("%" + val + "%");
        }

        public IQuery StartWith(string val)
        {
            return this.Like(val + "%");
        }

        protected void AddConstraint(string op, object val)
        {
            var c = new DbConstraint(this.Pending, op, val);
            AddConstraint(c);
        }

        protected void AddConstraint(DbConstraint c)
        {
            ErrorIfNotPending();
            _constraints.Add(c);
            _pending = null;
        }

        protected void AddConstraint(IWhereConstraint c)
        {
            ErrorIfNotPending();
            _constraints.Add(c);
            _pending = null;
        }

        #endregion

        #region 关联查询（引用属性查询）

        internal IList<RefTableProperty> RefItems
        {
            get { return this._refItems; }
        }

        public IQuery JoinRef(IRefProperty property)
        {
            this._refItems.Add(new RefTableProperty(property));

            return this;
        }

        public IQuery JoinRef(IRefProperty property, Type propertyOwnerType)
        {
            this._refItems.Add(new RefTableProperty(property, propertyOwnerType));

            return this;
        }

        #endregion

        #region 添加连接符

        public IQuery And()
        {
            ErrorIfPending();
            _operators.Add("and");
            return this;
        }

        public IQuery Or()
        {
            ErrorIfPending();
            _operators.Add("or");
            return this;
        }

        #endregion

        #region 排序

        public IQuery Order(IManagedProperty property, bool asc)
        {
            DbOrder o = new DbOrder(property, asc);
            _orders.Add(o);
            return this;
        }

        #endregion

        #region SQL 生成

        /// <summary>
        /// 如果有关联查询，则此方法返回生成的 INNER JOIN 语句。
        /// </summary>
        /// <param name="table">正在被查询的表</param>
        /// <returns></returns>
        internal string GetSqlInnerJoin(DbTable table)
        {
            if (this._refItems.Count > 0)
            {
                var sql = new StringBuilder();

                foreach (var refItem in this._refItems)
                {
                    DbTable mainTable = refItem.OwnerTable;
                    DbTable refTable = refItem.RefTable;
                    string fkName = refItem.FKName;

                    sql.AppendLine();
                    if (refItem.RefProperty.GetMeta(refItem.RefProperty.OwnerType).Nullable)
                    {
                        sql.Append("    LEFT OUTER JOIN ");
                    }
                    else
                    {
                        sql.Append("    INNER JOIN ");
                    }
                    sql.Append(refTable.QuoteName).Append(" ON ")
                        .Append(mainTable.QuoteName).Append(".").Append(mainTable.Prepare(fkName)).Append(" = ")
                        .Append(refTable.QuoteName).Append(".").Append(mainTable.Prepare(mainTable.PKID.Name));
                }

                return sql.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// 此方法返回生成的 WHERE 及 ORDER BY 语句。
        /// </summary>
        /// <param name="mainTable">正在被查询的表</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        internal string GetSqlWhereOrder(DbTable mainTable, FormatSqlParameter parameters)
        {
            if (!this.IsCompleted)
            {
                throw new InvalidOperationException("invalid query");
            }

            var sqlBuf = new StringBuilder();

            if (this._constraints.Count > 0)
            {
                sqlBuf.AppendLine().Append("WHERE");

                //为 constraint 生成 SQL
                for (int index = 0, cc = this._constraints.Count; index < cc; index++)
                {
                    if (index > 0)
                    {
                        string op = this._operators[index - 1];
                        sqlBuf.Append(' ').Append(op);
                    }
                    var constraint = this._constraints[index];
                    var nmt = constraint as DbConstraint;
                    if (nmt != null) nmt.SetMainTable(mainTable);

                    string sql = constraint.GetSql(parameters);
                    sqlBuf.Append(' ').Append(sql);
                }
            }

            //生成 ORDER BY
            var oc = this._orders.Count;
            if (oc > 0)
            {
                sqlBuf.Append(" ORDER BY ");
                for (int i = 0; i < oc; i++)
                {
                    if (i > 0) { sqlBuf.Append(','); }

                    var order = this._orders[i];
                    string sql = order.GetSql(mainTable);
                    sqlBuf.Append(sql);
                }
            }

            return sqlBuf.ToString();
        }

        #endregion

        #region 帮助方法

        private void ErrorIfPending()
        {
            if (_pending != null)
                throw new InvalidOperationException("you must finish operation started by Constrain(string)");
        }

        private void ErrorIfNotPending()
        {
            if (_pending == null)
                throw new InvalidOperationException("you must call Constrain(string) first");
        }

        private static void ErrorIfNull(object val)
        {
            if (val == null)
                throw new ArgumentNullException("parameter: val");
        }

        #endregion
    }
}