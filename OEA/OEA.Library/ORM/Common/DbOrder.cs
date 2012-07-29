using OEA.ManagedProperty;
namespace OEA.ORM
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

        public string GetSql(DbTable table)
        {
            string name = table.Translate(this._property);
            string dir = _asc ? "ASC" : "DESC";
            return string.Format("{0}.{1} {2}", table.QuoteName, table.Quote(name), dir);
        }
    }
}
