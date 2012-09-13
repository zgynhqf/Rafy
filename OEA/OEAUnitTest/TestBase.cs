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
using OEA.ORM.DbMigration;
using System.Reflection;

namespace OEAUnitTest
{
    [TestClass]
    public class TestBase
    {
        private static bool _appStarted = false;

        protected static readonly FieldInfo LocationField = typeof(OEAEnvironment).GetField("_Location", BindingFlags.NonPublic | BindingFlags.Static);

        protected static void EnterLocation(OEALocation loc, Action action)
        {
            var oldLocation = OEAEnvironment.Location;

            try
            {
                LocationField.SetValue(null, loc);
                Assert.IsTrue(OEAEnvironment.Location == loc, "无法进行测试用例需要的目标位置：" + loc);
                action();
            }
            finally
            {
                LocationField.SetValue(null, oldLocation);
            }
        }

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

                CacheInstance.SetCacheFile(Helper.MapFilePath(@"OEA\WPFClient\bin\Debug\OEA_Entity_Cache.sdf"));

                new TestApp().Start();
            }

            if (mockAsClientSide)
            {
                //根据客户化设置私有程序集
                ModifyPrivateBinPath();
            }

            using (var c = new OEADbMigrationContext(ConnectionStringNames.DbMigrationHistory))
            {
                //对于迁移日志库的构造，无法记录它本身的迁移日志
                c.HistoryRepository = null;

                c.AutoMigrate();
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
