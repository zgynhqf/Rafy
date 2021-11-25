using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rafy.Utils;
using Rafy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.UnitTest;

namespace RafyUnitTest
{
    public static class ServerTestHelper
    {
        private static bool _runned = false;

        public static void ClassInitialize(TestContext context)
        {
            CommonTestBase.Assert = new AssertAdapter();

            if (!_runned)
            {
                new TestServerApp().Start();
                _runned = true;
            }
        }

        public static long GetSqlTraceLength()
        {
            var file = ConfigurationHelper.GetAppSettingOrDefault("Rafy.FileLogger.SqlTraceFileName");
            var fileInfo = new FileInfo(file);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }

            return 0;
        }
    }
}
