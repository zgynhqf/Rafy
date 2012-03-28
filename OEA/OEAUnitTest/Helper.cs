using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OEA.Utils;
using OEA;
using Common;

namespace OEAUnitTest
{
    public static class Helper
    {
        private static readonly string SQL_TRACE_SWITCH_FILE = @"C:\SQL_TRACE_ENABLED";

        private static readonly string SQL_TRACE_LOG_FILE = @"C:\SQLTraceLog.txt";

        public static bool IsSqlTraceEnabled
        {
            get
            {
                return File.Exists(SQL_TRACE_SWITCH_FILE);
            }
            set
            {
                if (value)
                {
                    File.Delete(SQL_TRACE_SWITCH_FILE + "1");
                    File.Create(SQL_TRACE_SWITCH_FILE).Close();
                }
                else
                {
                    File.Delete(SQL_TRACE_SWITCH_FILE);
                    File.Create(SQL_TRACE_SWITCH_FILE + "1").Close();
                }
            }
        }

        public static long GetSqlTraceLength()
        {
            var fileInfo = new FileInfo(SQL_TRACE_LOG_FILE);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }

            return 0;
        }

        public static void LoginAsAdmin()
        {
            //var cur = OEAIdentity.Current;
            //if (cur == null || cur.Name != "admin")
            //{
            //    if (!OEAPrincipal.Login("admin", StringHelper.MD5(string.Empty)))
            //    {
            //        throw new InvalidOperationException();
            //    }
            //}
        }

        public static string MapDataFilePath(string relativePath)
        {
            return MapFilePath(Path.Combine(@"GIX4\Trunk\OEAUnitTest\Data\", relativePath));
        }

        public static string MapFilePath(string relativePath)
        {
            var root = ConfigurationHelper.GetAppSettingOrDefault("单元测试-整个代码的根目录");
            return Path.Combine(root, relativePath);
        }

        public static void CopyFileAndDir(string sourceDir, string desDir)
        {
            string[] filesSDir = Directory.GetFiles(sourceDir);
            string[] filesDDir = Directory.GetFiles(desDir);

            foreach (string sFile in filesSDir)
            {
                string sFileName = Path.GetFileName(sFile);
                bool bFileExists = false;
                foreach (string dFile in filesDDir)
                {
                    string dFileName = Path.GetFileName(dFile);
                    if (dFileName.ToUpper() == sFileName.ToUpper())
                    {
                        bFileExists = true;
                        break;
                    }
                }

                if (!bFileExists)
                {
                    File.Copy(sFile, desDir + "\\" + sFileName, true);
                }
            }

            //检验目录
            string[] subDirSou = Directory.GetDirectories(sourceDir);
            string[] subDirDes = Directory.GetDirectories(desDir);

            foreach (string sdir in subDirSou)
            {
                //Path.GetDirectoryName
                if (sdir.ToUpper().IndexOf(".SVN") > 0)
                {
                    continue;
                }

                string sOnedir = Path.GetFileName(sdir);

                bool dirExists = false;
                foreach (string ddir in subDirDes)
                {

                    string dOneDir = Path.GetFileName(ddir);
                    if (sOnedir.ToUpper() == dOneDir.ToUpper())
                    {
                        dirExists = true;
                        break;
                    }
                }


                if (!dirExists)
                {
                    Directory.CreateDirectory(desDir + "\\" + sOnedir);
                }
                CopyFileAndDir(sdir, desDir + "\\" + sOnedir);
            }
        }
    }
}
