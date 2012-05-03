using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Linq;
using hxy.Common.Data;
using System.Collections.Generic;

namespace OEA.ORM
{
    public class InConstraint : DbConstraint
    {
        public InConstraint(string column, string op, IList values) : base(column, op, values) { }

        public override string GetSql(DbTable table, ref int offset)
        {
            string column = table.Translate(this.Property);
            var sql = new StringBuilder()
                .Append(table.Quote(column)).Append(' ').Append(Operator).Append(" (");

            IList values = (IList)Value;
            if (values.Count > 0)
            {
                var indexFrom = offset;
                offset += values.Count;
                for (int i = indexFrom; i < offset; i++)
                {
                    if (i > indexFrom) sql.Append(',');
                    sql.Append('{' + i.ToString() + '}');
                }
            }
            else
            {
                sql.Append("NULL");
            }

            sql.Append(')');

            return sql.ToString();
        }

        public override void ReadParameters(List<object> paramaters)
        {
            IList values = (IList)this.Value;
            paramaters.AddRange(values.Cast<object>());
        }
    }
}
