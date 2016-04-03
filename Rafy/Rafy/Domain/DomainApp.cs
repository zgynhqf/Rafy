/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130405 23:41
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130405 23:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.ComponentModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 领域应用程序（用于启动领域实体框架）
    /// </summary>
    public class DomainApp : AppImplementationBase
    {
        protected override void PrepareToStartup()
        {
            base.PrepareToStartup();

            DataSaver.SubmitInterceptors = new List<Type>();
        }

        protected override void OnAllPluginsIntialized()
        {
            base.OnAllPluginsIntialized();

            //锁定该集合。
            DataSaver.SubmitInterceptors = new ReadOnlyCollection<Type>(DataSaver.SubmitInterceptors);
        }

        public void Startup()
        {
            try
            {
                this.StartupApplication();
            }
            catch (Exception ex)
            {
                Logger.LogError("领域应用程序在启动时发生异常", ex);
                throw;
            }
        }

        /// <summary>
        /// 当外部程序在完全退出时，通过领域应用程序也同时退出。
        /// </summary>
        public void NotifyExit()
        {
            try
            {
                this.OnExit();
            }
            catch (Exception ex)
            {
                Logger.LogError("领域应用程序退出时发生异常", ex);
                throw;
            }
        }
    }
}
