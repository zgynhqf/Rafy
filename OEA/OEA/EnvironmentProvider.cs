using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    public class EnvironmentProvider
    {
        public EnvironmentProvider()
        {
            this.RootDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            this.DllRootDirectory = this.RootDirectory;
        }

        /// <summary>
        /// 整个应用程序的根目录
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Dll 存在的目录路径
        /// （Web 项目的路径是 RootDirectory+"/Bin"）
        /// </summary>
        public string DllRootDirectory { get; set; }

        /// <summary>
        /// 在程序启动时，设置本属性以指示当前程序是否处于调试状态。
        /// </summary>
        public bool IsDebuggingEnabled { get; set; }
    }
}
