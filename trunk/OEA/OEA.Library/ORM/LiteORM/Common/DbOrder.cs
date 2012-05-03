namespace OEA.ORM
{
    public class DbOrder
    {
        private string column;
        private bool asc = true;

        public DbOrder(string column, bool asc)
        {
            this.column = column.ToLower();
            this.asc = asc;
        }

        public string Column
        {
            get { return column; }
        }

        public bool Ascending
        {
            get { return asc; }
        }

        public string GetSql(DbTable table)
        {
            string name = table.Translate(column);
            string dir = asc ? "ASC" : "DESC";
            return string.Format("{0} {1}", table.Quote(name), dir);
        }
    }
}
