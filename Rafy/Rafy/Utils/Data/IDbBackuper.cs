using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Data
{
    public interface IDbBackuper
    {
        /// <summary>
        /// Backup a special database
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="filename">database file path to save.</param>
        /// <param name="isErase">if exists, whether to delete current file.</param>
        /// <returns></returns>
        Result BackupDatabase(string databaseName, string filename, bool isErase);

        /// <summary>
        /// restore a special database from a file
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="filename">the file path which is a database backup</param>
        /// <returns></returns>
        Result RestoreDatabase(string databaseName, string filename);
    }
}
