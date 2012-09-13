using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Linq;
using hxy.Common.Data;
using System.Collections.Generic;
using OEA.ManagedProperty;

namespace OEA.ORM
{
    internal class InConstraint : DbConstraint
    {
        public InConstraint(IManagedProperty property, string op, IList values) : base(property, op, values) { }

        public override string GetSql(FormatSqlParameters parameters)
        {
            DbTable table = this.PropertyTable;

            string column = table.Translate(this.Property);
            var sql = new StringBuilder()
                .Append(table.QuoteName).Append('.').Append(table.Quote(column))
                .Append(' ').Append(Operator).Append(" (");

            IList values = (IList)this.Value;
            if (values.Count > 0)
            {
                var first = true;
                foreach (var value in values)
                {
                    if (!first) sql.Append(',');
                    parameters.AddParameter(sql, value);
                    first = false;
                }
            }
            else
            {
                sql.Append("NULL");
            }

            sql.Append(')');

            return sql.ToString();
        }
    }
}
