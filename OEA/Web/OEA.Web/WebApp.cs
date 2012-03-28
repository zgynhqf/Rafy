/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Server;
using System.Web;
using System.Configuration;
using OEA.Web.ClientMetaModel;
using System.IO;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Web.Services;

namespace OEA.Web
{
    /// <summary>
    /// WebApp 启动环境
    /// </summary>
    public class WebApp : AppImplementationBase
    {
        public void Startup()
        {
            this.OnAppStartup();
        }

        protected override void InitEnvironment()
        {
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath += @";D:\OEAView\GIX5\bin\Library";
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe = @"Library";
            //AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            OEAEnvironment.Location = OEALocation.WebServer;

            OEAEnvironment.Provider.IsDebuggingEnabled = HttpContext.Current.IsDebuggingEnabled;
            OEAEnvironment.Provider.DllRootDirectory = Path.Combine(OEAEnvironment.Provider.RootDirectory, "Bin");

            base.InitEnvironment();

            JsonServiceRepository.LoadAllServices();
        }

        protected override void InitEntityMeta()
        {
            //webcommand 需要在实体前初始化好
            this.InitCommandMetas();

            base.InitEntityMeta();
        }

        private void InitCommandMetas()
        {
            UIModel.WebCommands.AddByAssembly(typeof(WebApp).Assembly);

            UIModel.InitCommandMetas();

            //UIModel.Commands.SortByName(CommandNames.CommonCommands);
            //UIModel.Commands.SortByLabel("添加", "编辑", "删除", "保存", "刷新");

            if (OEAEnvironment.IsDebuggingEnabled)
            {
                WebCommandNames.TreeCommands.Insert(0, WebCommandNames.CustomizeUI);
                WebCommandNames.CommonCommands.Insert(0, WebCommandNames.CustomizeUI);
            }

            this.OnCommandMetasIntialized();
        }

        protected override void OnAllPluginsMetaIntialized()
        {
            UIModel.NotifyPluginsMetaIntialized();

            base.OnAllPluginsMetaIntialized();
        }

        protected override void OnAppModelCompleted()
        {
            UIModel.Freeze();

            base.OnAppModelCompleted();
        }

        protected override void StartMainProcess() { }
    }
}