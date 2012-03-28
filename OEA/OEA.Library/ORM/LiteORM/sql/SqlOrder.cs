namespace OEA.ORM.sqlserver
{
	public class SqlOrder
	{
		private string column;
		private bool asc = true;
		
		public SqlOrder(string column, bool asc)
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
		
		public string GetSql(SqlTable table)
		{
			string name = table.Translate(column);
			string dir = asc ? "asc" : "desc";
			return string.Format("[{0}] {1}", name, dir);
		}
	}
}
