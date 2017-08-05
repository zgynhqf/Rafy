/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101122
 * 说明：路径帮助程序
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101122
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Rafy.WPF
{
    internal static class PathHelper
    {
        /// <summary>
        /// 修改当前程序的私有路径
        /// </summary>
        /// <param name="path"></param>
        public static void ModifyPrivateBinPath(string path)
        {
            AppDomain.CurrentDomain.SetData("PRIVATE_BINPATH", path);
            AppDomain.CurrentDomain.SetData("BINPATH_PROBE_ONLY", path);
            //var m = typeof(AppDomainSetup).GetMethod("UpdateContextProperty", BindingFlags.NonPublic | BindingFlags.Static);
            //var funsion = typeof(AppDomain).GetMethod("GetFusionContext", BindingFlags.NonPublic | BindingFlags.Instance);
            //m.Invoke(null, new object[] { funsion.Invoke(AppDomain.CurrentDomain, null), "PRIVATE_BINPATH", path });
        }
    }
}