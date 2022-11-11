/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211027
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211027 09:23
 * 
*******************************************************/

using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rafy.ComponentModel
{
    /// <summary>
    ///  UI 层应用程序的基类。
    ///  在 DomainApp 的基础上，添加了界面元数据的初始化。
    /// </summary>
    public abstract class UIApp : DomainApp
    {
        protected override void PrepareToStartup()
        {
            RafyEnvironment.Provider = new UIEnvironmentProvider();

            base.PrepareToStartup();

            UIModel.Reset();
            WPFCommandNames.Clear();

            UIEnvironment.IsWPFUI = false;
            UIEnvironment.IsWebUI = false;
        }

        protected override void InitEnvironment()
        {
            base.InitEnvironment();

            //设置多国语言
            this.SetupLanguage();
        }

        protected override void OnMetaCreated()
        {
            //冻结模块的元数据
            UIModel.Modules.Freeze();

            base.OnMetaCreated();
        }

        /// <summary>
        /// 设置当前语言
        /// 
        /// 需要在所有 Translator 依赖注入完成后调用。
        /// </summary>
        private void SetupLanguage()
        {
            if (RafyEnvironment.Provider.Translator == null)
            {
                RafyEnvironment.Provider.Translator = new EmptyTranslator();
            }

            //当前线程的文化名，就是 Rafy 多国语言的标识。
            var culture = Thread.CurrentThread.CurrentUICulture.Name;
            if (!Translator.IsDevCulture(culture))
            {
                var translator = RafyEnvironment.Provider.Translator as Translator;
                if (translator != null)
                {
                    //目前，在服务端进行翻译时，只支持一种语言。所以尽量在客户端进行翻译。
                    translator.CurrentCulture = culture;
                    translator.Enabled = true;
                }
            }
        }
    }
}
