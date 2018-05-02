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
        /// <param name="isErase">if exists, whether to delete current file.</param>
        /// <returns></returns>
        public Result BackupDatabase(string databaseName, string filename, bool isErase)
        {
            var result = DealFile(filename, isErase);
            if (result.Success)
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
                    return new Result(ex.Message);
                }
                return new Result(true);
            }
            return result;
        }

        /// <summary>
        /// restore a special database from a file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="filename">the file path which is a database backup</param>
        /// <returns></returns>
        public Result RestoreDatabase(string databaseName, string filename)
        {
            if (!File.Exists(filename))
            {
                return new Result("当前的文件不存在!");
            }

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
                return new Result(ex.Message);
            }
            finally
            {
                this._db.Connection.Close();
            }
            return new Result(true);
        }

        #endregion

        /// <summary>
        /// 处理文件
        /// </summary>
        /// <param name="filename">文件名</param>
        /// <param name="isErase">是否擦除</param>
        /// <returns></returns>
        private static Result DealFile(string filename, bool isErase)
        {
            try
            {
                if (File.Exists(filename))//文件存在
                {
                    if (!isErase)//不擦除
                    {
                        return new Result("文件已经存在!");
                    }
                    else//擦除
                    {
                        File.Delete(filename);
                    }
                }
                string directory = Path.GetDirectoryName(filename);
                string file = Path.GetFileName(filename);
                if (!Directory.Exists(directory))//路径不存在,创建
                {
                    Directory.CreateDirectory(directory);
                }
                return new Result(true);
            }
            catch (Exception ex)
            {
                return new Result(ex.Message);
            }
        }
    }
}