using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Rafy.Utils;
using Rafy;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RafyUnitTest
{
    public static class ServerTestHelper
    {
        public static void ClassInitialize(TestContext context)
        {
            new TestServerApp().Start();
        }

        public static long GetSqlTraceLength()
        {
            var file = ConfigurationHelper.GetAppSettingOrDefault("Rafy:FileLogger:SqlTraceFileName");
            var fileInfo = new FileInfo(file);
            if (fileInfo.Exists)
            {
                return fileInfo.Length;
            }

            return 0;
        }
    }
}
