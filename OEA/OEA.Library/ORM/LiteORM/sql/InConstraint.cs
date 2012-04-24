using System;
using System.Collections;
using System.Data;
using System.Text;
using hxy.Common.Data;
using System.Collections.Generic;

namespace OEA.ORM.sqlserver
{
    public class InConstraint : SqlConstraint
    {
        public InConstraint(string column, string op, IList values)
            : base(column, op, values)
        { }

        public override string GetSql(SqlTable table, ref int offset)
        {
            string name = table.Translate(Column);
            if (name == null || name.Length == 0)
                throw new LightException(string.Format("column {0} not found in table {1}",
                                                       Column, table.Name));
            index = offset;
            IList values = (IList)Value;
            StringBuilder buf = new StringBuilder();
            buf.Append("[").Append(name).Append("] ").Append(Operator).Append(" (");
            if (values.Count > 0)
            {
                offset = offset + values.Count;
                for (int i = index; i < offset; i++)
                {
                    if (i > index)
                        buf.Append(",");
                    string pname = string.Format("@{0}", i);
                    buf.Append(pname);
                }
            }
            else
            {
                buf.Append("null");
            }
            buf.Append(")");
            return buf.ToString();
        }

        public override void SetParameters(IParameterFactory pf, List<IDbDataParameter> paramaters)
        {
            IList values = (IList)Value;
            for (int i = 0; i < values.Count; i++)
            {
                int j = index + i;

                IDbDataParameter p = pf.CreateParameter();
                p.ParameterName = string.Format("@{0}", j);
                SqlUtils.PrepParam(p, values[i]);
                paramaters.Add(p);
            }
        }
    }
}
