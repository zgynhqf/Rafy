/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110520
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110520
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Library;

namespace OEA.Module.WPF
{
    /// <summary>
    /// WPF 模块基类
    /// 
    /// 主要用于规范子类提供一些公共的事件注册机制。
    /// </summary>
    public abstract class WPFModuleBase : IModule
    {
        private IClientApp _app;

        protected IClientApp ClientApp
        {
            get { return this._app; }
        }

        public virtual ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        void IModule.Initialize(IClientApp app)
        {
            this._app = app;

            this.InitializeCore(app);
        }

        protected virtual void InitializeCore(IClientApp app) { }

        #region 定义一些通用的事件处理方法

        /// <summary>
        /// 定义模型
        /// </summary>
        /// <param name="action"></param>
        protected void DefineModels(Action<object> action)
        {
            this._app.ModelOpertions += (o, e) =>
            {
                action(null);
            };
        }

        /// <summary>
        /// 把指定的Resouce加入到应用程序中
        /// 
        /// 使用方法：
        /// this.AddResource("OEA.Module.WPF;component/Resources/ComboListControl.xaml");
        /// </summary>
        /// <param name="packUri"></param>
        protected void AddResource(string packUri)
        {
            var uri = new Uri(packUri, UriKind.Relative);
            var resouceDic = Application.LoadComponent(uri) as ResourceDictionary;
            var app = Application.Current;
            if (app != null) app.Resources.MergedDictionaries.Add(resouceDic);
        }

        #endregion
    }
}
