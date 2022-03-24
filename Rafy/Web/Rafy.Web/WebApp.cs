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
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Web;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.UI;
using Rafy.Web.ClientMetaModel;

namespace Rafy.Web
{
    /// <summary>
    /// WebApp 启动环境
    /// </summary>
    public class WebApp : UIApp
    {
        protected override void PrepareToStartup()
        {
            base.PrepareToStartup();

            JsonServiceRepository.Clear();
        }

        protected override void InitEnvironment()
        {
            //如果在初始化时发生异常，则会引发再次启动。这时应该保证之前的所有的初始化工作归零。
            UIEnvironment.IsWebUI = true;

#if NET45
            //如果是网站，则一个 HttpContext 使用一个身份（上下文）；否则，每个线程使用一个单独的身份（上下文）。
            AppContext.SetProvider(new WebOrThreadAppContextProvider());

#endif
            base.InitEnvironment();
        }

        protected override void CreateMeta()
        {
            JsonServiceRepository.LoadAllServices();

            //虽然是 WebApp，但是可能只是作为 WPFClient 的服务端。
            if (UIEnvironment.IsWebUI)
            {
                //webcommand 需要在实体前初始化好
                this.InitCommandMetas();
            }

            base.CreateMeta();
        }

        private void InitCommandMetas()
        {
            UIModel.WebCommands.AddByAssembly(typeof(WebApp).Assembly);

            UIModel.InitCommandMetas();

            //UIModel.Commands.SortByName(CommandNames.CommonCommands);
            //UIModel.Commands.SortByLabel("添加", "编辑", "删除", "保存", "刷新");

            //this.OnCommandMetasIntialized();
        }

        protected override void OnMetaCreated()
        {
            UIModel.Freeze();

            base.OnMetaCreated();
        }
    }
}