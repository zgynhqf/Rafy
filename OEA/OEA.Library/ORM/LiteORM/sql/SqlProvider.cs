using System;
using System.Data;
using System.Data.SqlClient;

namespace OEA.ORM.sqlserver
{
	public class SqlProvider : IProvider
	{
		protected string connString;
		
		public SqlProvider(string connectionString)
		{
			connString = connectionString;
		}
		
		public SqlProvider() : this(null)
		{}
		
		public virtual IDb OpenDb(IDbConnection cn)
		{
			SqlDb db = new SqlDb(this, cn);
			db.IsAdapter = true;
			return db;
		}
		
		public virtual IDb OpenDb(string connectString)
		{
			IDbConnection cn = new SqlConnection(connectString);
			cn.Open();
			IDb db = new SqlDb(this, cn);
			return db;
		}
		
		public virtual IDb OpenDb()
		{
			IDbConnection cn = new SqlConnection(connString);
			cn.Open();
			IDb db = new SqlDb(this, cn);
			return db;
		}
		
		public virtual void CloseDb(SqlDb db)
		{
			if (!db.IsClosed)
			{
				if (!db.IsAdapter && db.Connection != null)
				{
					SqlConnection cn = (SqlConnection) db.Connection;
					if (cn.State != ConnectionState.Closed)
						cn.Close();
					cn.Dispose();
					db.Connection = null;
				}
			}
		}
	}
}
