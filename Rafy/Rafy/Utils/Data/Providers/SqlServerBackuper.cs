using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using System.Data.SqlClient;
using Rafy.Data;

namespace Rafy.Data.Providers
{
    public class SqlServerBackuper : IDbBackuper
    {
        private IDbAccesser _db;

        public SqlServerBackuper(IDbAccesser masterDBAccesser)
        {
            if (masterDBAccesser == null) throw new ArgumentNullException("masterDBAccesser");

            this._db = masterDBAccesser;
        }

        protected virtual string DatabaseIdColumnName
        {
            get
            {
                return "_dbid";
            }
        }

        #region IDbBackuper Members

        /// <summary>
        /// Backup a special database
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="filename">database file path to save.</param>
        /// 
        /// <returns></returns>
        public Result BackupDatabase(string databaseName, string filename)
        {
            string strCmd = "BACKUP DATABASE " + databaseName + " TO DISK = @devicename";
            try
            {
                _db.RawAccesser.ExecuteText(
                    strCmd,
                    _db.RawAccesser.ParameterFactory.CreateParameter("@devicename", filename)
                    );
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return true;
        }

        /// <summary>
        /// restore a special database from a file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="filename">the file path which is a database backup</param>
        /// <returns></returns>
        public Result RestoreDatabase(string databaseName, string filename)
        {
            //在master数据库中还原!!
            string strRestore = "RESTORE DATABASE " + databaseName + " FROM DISK=@deviceName";
            try
            {
                this._db.Connection.Open();
                this._db.Connection.ChangeDatabase("master");
                //find all active processes
                string query = string.Format(@"
SELECT spid 
FROM sysprocesses , sysdatabases 
WHERE sysprocesses.{1} = sysdatabases.{1} AND sysdatabases.name = '{0}'", databaseName, this.DatabaseIdColumnName);
                var dt = this._db.QueryDataTable(query, CommandType.Text);
                //kill all active processes
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var id = Convert.ToInt32(dt.Rows[i][0]);
                    this._db.RawAccesser.ExecuteText("KILL " + id);
                }
                //restore
                this._db.RawAccesser.ExecuteText(
                    strRestore,
                    _db.RawAccesser.ParameterFactory.CreateParameter("@devicename", filename)
                    );
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                this._db.Connection.Close();
            }
            return true;
        }

        #endregion
    }
}