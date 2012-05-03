using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using OEA.ManagedProperty;
using OEA.MetaModel;
using hxy.Common.Data;

namespace OEA.ORM
{
    public class DbQuery : IQuery
    {
        private List<DbConstraint> _constraints;

        protected List<string> _operators;

        protected List<DbOrder> _orders;

        private string _pending;

        public DbQuery() : this(null) { }

        public DbQuery(Type entityType)
        {
            _constraints = new List<DbConstraint>(3);
            _operators = new List<string>(2);
            _orders = new List<DbOrder>(1);

            this.EntityType = entityType;
        }

        protected List<DbConstraint> Constraints
        {
            get { return _constraints; }
        }

        protected List<string> Operators
        {
            get { return _operators; }
        }

        protected List<DbOrder> Orders
        {
            get { return _orders; }
        }

        protected string Pending
        {
            get { return _pending; }
        }

        public IConstraint Get(int index)
        {
            if (index > -1 && index < _constraints.Count)
                return _constraints[index];
            return null;
        }

        protected void ErrorIfPending()
        {
            if (_pending != null)
                throw new InvalidOperationException("you must finish operation started by Constrain(string)");
        }

        protected void ErrorIfNotPending()
        {
            if (_pending == null)
                throw new InvalidOperationException("you must call Constrain(string) first");
        }

        protected void ErrorIfNull(object val)
        {
            if (val == null)
                throw new ArgumentNullException("parameter: val");
        }

        public Type EntityType { get; private set; }

        public bool IsEmpty
        {
            get { return this._constraints.Count == 0 && !this.HasOrdered; }
        }

        public bool HasOrdered
        {
            get { return this._orders.Count > 0; }
        }

        private string ToColumnName(IManagedProperty property)
        {
            var type = this.EntityType;
            if (type == null) throw new InvalidOperationException("要使用 IManagedProperty 作为查询参数，需要使用 IDb.Query(Type entityType) 接口。");
            string column = property.GetMetaPropertyName(type);
            return column;
        }

        public IQuery Constrain(IManagedProperty property)
        {
            string column = ToColumnName(property);
            return this.Constrain(column);
        }

        public IQuery Constrain(string column)
        {
            ErrorIfPending();
            _pending = column;
            return this;
        }

        public IQuery Constrain(IQuery query)
        {
            ErrorIfPending();
            this.Constraints.Add(new DbConstraint(query as DbQuery));
            return this;
        }

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

        public IQuery Equal(object val)
        {
            if (val == null || val == DBNull.Value)
                AddConstraint(new NullConstraint(this.Pending, true));
            else
                AddConstraint("=", val);
            return this;
        }

        public IQuery NotEqual(object val)
        {
            if (val == null || val == DBNull.Value)
                AddConstraint(new NullConstraint(this.Pending, false));
            else
                AddConstraint("!=", val);
            return this;
        }

        public IQuery Greater(object val)
        {
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

            //edit by zhoujg
            //AddConstraint(" like ", val);
            AddConstraint(" like ", "%" + val + "%");

            return this;
        }

        public IQuery Order(string column, bool asc)
        {
            DbOrder o = new DbOrder(column, asc);
            _orders.Add(o);
            return this;
        }

        public IQuery Order(IManagedProperty property, bool asc)
        {
            string column = ToColumnName(property);
            return this.Order(column, asc);
        }

        protected bool IsComplete
        {
            get
            {
                int c = _constraints.Count;
                int o = _operators.Count;
                return (_pending == null) && (((c == 0 || c == 1) && o == 0) || ((c - 1) == o));
            }
        }

        public string GetSql(DbTable table)
        {
            int offset = 0;
            return GetSql(table, ref offset);
        }

        public string GetSql(DbTable table, ref int offset)
        {
            if (!this.IsComplete)
            {
                throw new InvalidOperationException("invalid query");
            }

            var buf = new StringBuilder();
            int cc = this._constraints.Count;
            if (cc > 0)
            {
                buf.Append("WHERE");
                for (int i = 0; i < cc; i++)
                {
                    if (i > 0)
                    {
                        string op = _operators[i - 1];
                        buf.Append(' ').Append(op);
                    }
                    var constraint = _constraints[i];
                    string sql = constraint.GetSql(table, ref offset);
                    buf.Append(' ').Append(sql);
                }
            }
            var oc = _orders.Count;
            if (oc > 0)
            {
                buf.Append(" ORDER BY ");
                for (int i = 0; i < oc; i++)
                {
                    if (i > 0)
                        buf.Append(',');
                    var order = _orders[i];
                    string sql = order.GetSql(table);
                    buf.Append(sql);
                }
            }
            return buf.ToString();
        }

        public void ReadParameters(List<object> paramaters)
        {
            foreach (var constraint in this._constraints)
            {
                constraint.ReadParameters(paramaters);
            }
        }
    }
}
