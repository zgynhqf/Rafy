using System;
using System.Data;
using hxy.Common.Data;
using System.Collections.Generic;

namespace OEA.ORM
{
    public class NullConstraint : DbConstraint
    {
        public NullConstraint(string column, bool isNull) : base(column, isNull ? "is" : "is not", null) { }

        public override string GetSql(DbTable table, ref int offset)
        {
            string name = table.Translate(this.Property);

            return string.Format("{0} {1} null", table.Quote(name), this.Operator);
        }

        public override void ReadParameters(List<object> paramaters)
        {
            // nothing to do
        }
    }
}
