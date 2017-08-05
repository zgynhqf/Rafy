/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 
*******************************************************/

using Rafy.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// 环境变量值提供器。
    /// </summary>
    public class EnvironmentProvider
    {
        public EnvironmentProvider()
        {
            this.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.DllRootDirectory = this.RootDirectory;
            this.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("Rafy.IsDebuggingEnabled", false);

            //var httpContext = HttpContext.Current;
            //if (httpContext != null)
            //{
            //    //this.DllRootDirectory = httpContext.Server.MapPath("Bin"); 许保同修改
            //    this.IsDebuggingEnabled = false;//httpContext.IsDebuggingEnabled;许保同修改
            //}
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

        private Translator _translator;

        /// <summary>
        /// 当前使用的翻译器
        /// </summary>
        public Translator Translator
        {
            get
            {
                if (this._translator == null)
                {
                    this._translator = new EmptyTranslator();
                }
                return _translator;
            }
            set
            {
                if (value == null) throw new ArgumentNullException("value");

                _translator = value;
            }
        }
    }
}
