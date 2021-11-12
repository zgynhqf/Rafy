/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2012
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2012
 * 编辑文件 崔化栋 20180502 14:00
 * 
*******************************************************/

using Rafy.ManagedProperty;
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
#if NET45
        public EnvironmentProvider()
        {
            this.RootDirectory = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var httpContext = System.Web.HttpContext.Current;
            if (httpContext != null)
            {
                this.DllRootDirectory = httpContext.Server.MapPath("Bin");
                this.IsDebuggingEnabled = httpContext.IsDebuggingEnabled;
            }
            else
            {
                this.DllRootDirectory = this.RootDirectory;
            }
        }
#endif
#if NS2
        public EnvironmentProvider()
        {
            this.RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
            this.DllRootDirectory = this.RootDirectory;
            this.IsDebuggingEnabled = ConfigurationHelper.GetAppSettingOrDefault("Rafy:IsDebuggingEnabled", false);

            //var httpContext = HttpContext.Current;
            //if (httpContext != null)
            //{
            //    //this.DllRootDirectory = httpContext.Server.MapPath("Bin"); 许保同修改
            //    this.IsDebuggingEnabled = false;//httpContext.IsDebuggingEnabled;许保同修改
            //}
        }
#endif

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

        /// <summary>
        /// 当前使用的翻译器
        /// </summary>
        public ITranslator Translator { get; set; }

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取属性对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[属性名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public virtual string GetLabelForDisplay(IManagedProperty property, Type entityType)
        {
            return $"[{entityType.FullName}.{property.Name}]";
        }

        /// <summary>
        /// 如果当前 Rafy 运行时环境中，已经拥有 UI 层界面的元数据，则获取实体对应的的显示名称，并进行翻译后返回。
        /// 否则，直接返回以下格式的字符串，方便替换：[实体类型名称]。（服务端一般都没有 UI 层元数据。）
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public virtual string GetLabelForDisplay(Type entityType)
        {
            return $"[{entityType.FullName}]";
        }
    }
}