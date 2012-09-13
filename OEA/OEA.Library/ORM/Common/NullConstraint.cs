using System;
using System.Data;
using hxy.Common.Data;
using System.Collections.Generic;
using OEA.ManagedProperty;

namespace OEA.ORM
{
    internal class NullConstraint : DbConstraint
    {
        public NullConstraint(IManagedProperty column, bool isNull) : base(column, isNull ? "is" : "is not", null) { }

        public override string GetSql(FormatSqlParameters paramaters)
        {
            string name = this.PropertyTable.Translate(this.Property);

            return string.Format("{0}.{1} {2} null", this.PropertyTable.QuoteName, this.PropertyTable.Quote(name), this.Operator);
        }
    }
}
