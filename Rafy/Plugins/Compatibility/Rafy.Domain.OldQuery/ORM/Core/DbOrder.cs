using System.IO;
using System.Text;
using Rafy.ManagedProperty;
namespace Rafy.Domain.ORM
{
    internal class DbOrder
    {
        private IManagedProperty _property;

        private bool _asc = true;

        public DbOrder(IManagedProperty property, bool asc)
        {
            this._property = property;
            this._asc = asc;
        }

        public IManagedProperty Property
        {
            get { return this._property; }
        }

        public bool Ascending
        {
            get { return _asc; }
        }

        public void GetSql(TextWriter sql, RdbTable table)
        {
            string name = table.Translate(this._property);
            string dir = _asc ? "ASC" : "DESC";
            sql.AppendQuoteName(table).Write('.');
            sql.AppendQuote(table, name);
            sql.Write(' ');
            sql.Write(dir);
        }
    }
}
