using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using OEA.ManagedProperty;
using OEA.MetaModel;
using hxy.Common.Data;

namespace OEA.ORM.sqlserver
{
    public class SqlQuery : IQuery
    {
        protected List<SqlConstraint> constraints;
        protected List<string> operators;
        protected List<SqlOrder> orders;
        private string pending;

        public SqlQuery() : this(null) { }

        public SqlQuery(Type entityType)
        {
            constraints = new List<SqlConstraint>(3);
            operators = new List<string>(2);
            orders = new List<SqlOrder>(1);

            this.EntityType = entityType;
        }

        public IConstraint Get(int index)
        {
            if (index > -1 && index < constraints.Count)
                return constraints[index];
            return null;
        }

        private void ErrorIfPending()
        {
            if (pending != null)
                throw new InvalidOperationException("you must finish operation started by Constrain(string)");
        }

        private void ErrorIfNotPending()
        {
            if (pending == null)
                throw new InvalidOperationException("you must call Constrain(string) first");
        }

        private void ErrorIfNull(object val)
        {
            if (val == null)
                throw new ArgumentNullException("parameter: val");
        }

        public IQuery Constrain(IQuery query)
        {
            ErrorIfPending();
            constraints.Add(new SqlConstraint(query));
            return this;
        }

        public Type EntityType { get; private set; }

        public bool HasOrdered
        {
            get { return this.orders.Count > 0; }
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
            pending = column;
            return this;
        }

        public IQuery And()
        {
            ErrorIfPending();
            operators.Add("and");
            return this;
        }

        public IQuery Or()
        {
            ErrorIfPending();
            operators.Add("or");
            return this;
        }

        public IQuery In(IList values)
        {
            SqlConstraint c = new InConstraint(pending, "in", values);
            AddConstraint(c);
            return this;
        }

        public IQuery NotIn(IList values)
        {
            SqlConstraint c = new InConstraint(pending, "not in", values);
            AddConstraint(c);
            return this;
        }

        protected void AddConstraint(string op, object val)
        {
            SqlConstraint c = new SqlConstraint(pending, op, val);
            AddConstraint(c);
        }

        protected void AddConstraint(SqlConstraint c)
        {
            ErrorIfNotPending();
            constraints.Add(c);
            pending = null;
        }

        public IQuery Equal(object val)
        {
            if (val == null || val == DBNull.Value)
                AddConstraint(new NullConstraint(pending, true));
            else
                AddConstraint("=", val);
            return this;
        }

        public IQuery NotEqual(object val)
        {
            if (val == null || val == DBNull.Value)
                AddConstraint(new NullConstraint(pending, false));
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
            SqlOrder o = new SqlOrder(column, asc);
            orders.Add(o);
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
                int c = constraints.Count;
                int o = operators.Count;
                return (pending == null) && (((c == 0 || c == 1) && o == 0) || ((c - 1) == o));
            }
        }

        public virtual string GetSql(SqlTable table, ref int offset)
        {
            if (!IsComplete)
                throw new InvalidOperationException("invalid query");

            StringBuilder buf = new StringBuilder();
            int sz = constraints.Count;
            if (sz > 0)
                buf.Append("where");
            for (int i = 0; i < sz; i++)
            {
                if (i > 0)
                {
                    string op = operators[i - 1];
                    buf.Append(" ").Append(op);
                }
                SqlConstraint constraint = constraints[i];
                string sql = constraint.GetSql(table, ref offset);
                buf.Append(" ").Append(sql);
            }
            sz = orders.Count;
            if (sz > 0)
            {
                buf.Append(" order by ");
                for (int i = 0; i < sz; i++)
                {
                    if (i > 0)
                        buf.Append(",");
                    SqlOrder order = orders[i];
                    string sql = order.GetSql(table);
                    buf.Append(sql);
                }
            }
            return buf.ToString();
        }

        public virtual void SetParameters(IParameterFactory pf, List<IDbDataParameter> paramaters)
        {
            foreach (SqlConstraint constraint in constraints)
            {
                constraint.SetParameters(pf, paramaters);
            }
        }

        public virtual string GetSql(SqlTable table)
        {
            int offset = 1;
            return GetSql(table, ref offset);
        }

        #region IQuery Members

        //add by zhoujg
        private System.Collections.Generic.IList<string> columns;
        public System.Collections.Generic.IList<string> Columns
        {
            get
            {
                return columns;
            }
            set
            {
                columns = value;
                for (int i = 0; i <= columns.Count - 1; i++)
                {
                    columns[i] = columns[i].ToLower();
                }
            }
        }

        #endregion
    }
}
