/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 13:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ComponentModel;
using Rafy.MetaModel.View;

namespace Rafy.MultiLanguages.WPF
{
    internal class MultiLanguagesWPFPlugin : UIPlugin
    {
        protected override int SetupLevel
        {
            get { return PluginSetupLevel.System; }
        }

        public override void Initialize(IApp app)
        {
            (app as IClientApp).LoginSuccessed += OnLoginSuccessed;
            app.Exit += OnExit;
        }

        private bool _loginSuccessed = false;

        private void OnLoginSuccessed(object sender, EventArgs e)
        {
            this._loginSuccessed = true;
        }

        private void OnExit(object sender, EventArgs e)
        {
            if (this._loginSuccessed)
            {
                //在程序退出时，自动添加开发语言项。
                DbTranslator.Instance.AutoSave();
            }
        }
    }
}