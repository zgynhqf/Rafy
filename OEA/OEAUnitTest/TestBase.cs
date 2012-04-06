using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OEA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OEA.Library;
using OEA.Library.Caching;
using OEA.Utils;
using Microsoft.Practices.Unity;
using System.Configuration;
using OEA.Server;
using System.Diagnostics;

namespace OEAUnitTest
{
    [TestClass]
    public class TestBase
    {
        private static bool _appStarted = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mockAsClientSide">
        /// 当前的测试环境是否需要
        /// </param>
        public static void ClassInitialize(TestContext context, bool mockAsClientSide = false)
        {
            if (!_appStarted)
            {
                _appStarted = true;

                Helper.CopyFileAndDir(Helper.MapFilePath(@"OEA\OEAUnitTest\bin\Debug"), AppDomain.CurrentDomain.SetupInformation.ApplicationBase);

                CacheInstance.SetCacheFile(Helper.MapFilePath(@"OEA\OEA.Host.WPF\bin\Debug\Cache.sdf"));

                new TestApp().Start();
            }

            if (mockAsClientSide)
            {
                //根据客户化设置私有程序集
                ModifyPrivateBinPath();
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            //由于每个单元测试是由单独线程运行的，所以登录的代码放在 TestInitialize 中。
            Helper.LoginAsAdmin();
        }

        /// <summary>
        /// 根据客户化的信息，动态设置私有程序集路径
        /// 1、不设置的话会出现找不到加载的dll的情况
        /// 2、直接SetData不起作用，需要手动刷新下
        /// </summary>
        private static void ModifyPrivateBinPath()
        {
            var pathes = new List<string> { 
                "Library", "Module", "Files", "Report"
            };

            var dlls = OEAEnvironment.GetEntityDlls(false);
            if (dlls.Length > 0)
            {
                var dir = Path.GetDirectoryName(dlls[0]) + @"\Library";
                pathes.Add(dir);

                var path = string.Join(";", pathes);

                PathHelper.ModifyPrivateBinPath(path);
            }
        }

        [DebuggerStepThrough]
        protected static void AssertIsTrue(bool value)
        {
            Assert.IsTrue(value);
        }

        //[ClassCleanup]
        //public static void ClassCleanup() { }
    }
}
