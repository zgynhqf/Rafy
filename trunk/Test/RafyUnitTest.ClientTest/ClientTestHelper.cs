using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rafy.Utils;
using Rafy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RafyUnitTest.ClientTest
{
    public static class ClientTestHelper
    {
        public static void ClassInitialize(TestContext context)
        {
            new TestLocalApp().Start();
        }

        public static string MapFilePath(string relativePath)
        {
            var root = ConfigurationHelper.GetAppSettingOrDefault("单元测试-整个代码的根目录");
            return Path.Combine(root, relativePath);
        }

        /// <summary>
        /// 根据客户化的信息，动态设置私有程序集路径
        /// 1、不设置的话会出现找不到加载的dll的情况
        /// 2、直接SetData不起作用，需要手动刷新下
        /// </summary>
        public static void ModifyPrivateBinPath()
        {
            var dlls = RafyEnvironment.GetCustomerEntityDlls(false);
            if (dlls.Length > 0)
            {
                var pathes = new List<string> { 
                    "Library", "Module", "Files", "Report"
                };

                var dir = Path.GetDirectoryName(dlls[0]) + @"\Library";
                pathes.Add(dir);

                var path = string.Join(";", pathes);

                PathHelper.ModifyPrivateBinPath(path);
            }
        }

        //public static void CopyFileAndDir(string sourceDir, string desDir)
        //{
        //    string[] filesSDir = Directory.GetFiles(sourceDir);
        //    string[] filesDDir = Directory.GetFiles(desDir);

        //    foreach (string sFile in filesSDir)
        //    {
        //        string sFileName = Path.GetFileName(sFile);
        //        bool bFileExists = false;
        //        foreach (string dFile in filesDDir)
        //        {
        //            string dFileName = Path.GetFileName(dFile);
        //            if (dFileName.ToUpper() == sFileName.ToUpper())
        //            {
        //                bFileExists = true;
        //                break;
        //            }
        //        }

        //        if (!bFileExists)
        //        {
        //            File.Copy(sFile, desDir + "\\" + sFileName, true);
        //        }
        //    }

        //    //检验目录
        //    string[] subDirSou = Directory.GetDirectories(sourceDir);
        //    string[] subDirDes = Directory.GetDirectories(desDir);

        //    foreach (string sdir in subDirSou)
        //    {
        //        //Path.GetDirectoryName
        //        if (sdir.ToUpper().IndexOf(".SVN") > 0)
        //        {
        //            continue;
        //        }

        //        string sOnedir = Path.GetFileName(sdir);

        //        bool dirExists = false;
        //        foreach (string ddir in subDirDes)
        //        {

        //            string dOneDir = Path.GetFileName(ddir);
        //            if (sOnedir.ToUpper() == dOneDir.ToUpper())
        //            {
        //                dirExists = true;
        //                break;
        //            }
        //        }

        //        if (!dirExists)
        //        {
        //            Directory.CreateDirectory(desDir + "\\" + sOnedir);
        //        }
        //        CopyFileAndDir(sdir, desDir + "\\" + sOnedir);
        //    }
        //}
    }
}
