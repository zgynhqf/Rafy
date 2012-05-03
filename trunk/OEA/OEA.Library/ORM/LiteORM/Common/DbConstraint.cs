using System;
using System.Data;
using System.Collections.Generic;
using hxy.Common.Data;

namespace OEA.ORM
{
    public class DbConstraint : IConstraint
    {
        private string _property;

        private string _operator;

        private object _value;

        private DbQuery _query;

        public DbConstraint(string property, string op, object val)
        {
            this._property = property.ToLower();
            this._operator = op;
            this._value = val;
        }

        public DbConstraint(DbQuery query)
        {
            this._query = query;
        }

        public string Property
        {
            get { return _property; }
        }

        public string Operator
        {
            get { return _operator; }
        }

        public object Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public DbQuery Query
        {
            get { return _query; }
        }

        public virtual string GetSql(DbTable table, ref int offset)
        {
            if (this._query != null)
            {
                string sql = this._query.GetSql(table, ref offset);
                return string.Format("({0})", sql);
            }

            string name = table.Translate(this.Property);
            return table.Quote(name) + this.Operator + '{' + offset++ + '}';
        }

        public virtual void ReadParameters(List<object> paramaters)
        {
            if (this._query != null)
            {
                this._query.ReadParameters(paramaters);
            }
            else
            {
                paramaters.Add(this.Value);
            }
        }

        IQuery IConstraint.Query
        {
            get { return this._query; }
        }
    }
}
