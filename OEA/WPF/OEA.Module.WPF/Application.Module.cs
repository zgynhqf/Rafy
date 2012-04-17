/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110218
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100218
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Utils;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 客户端应用程序API集合
    /// 
    /// 考虑到上层开发人员经常使用此类，所以命名不再使用 OEAApplication/OEAApp/ClientApp
    /// 将来服务端的App类请使用 ServerApp 命名
    /// </summary>
    public partial class App
    {
        #region 字段

        private ModuleViewModelCollection _userModulesView;

        private IList<ModuleMeta> _userModules;

        #endregion

        #region 界面初始化逻辑

        internal void InitUIModuleList()
        {
            //初始化用户模块列表
            this._userModules = CommonModel.Modules.GetRootsWithPermission().ToList();

            //初始化用户模块列表 - 视图模型
            //过滤出所有在界面中显示的模块。
            var modules = this._userModules.ToList();
            this._userModulesView = new ModuleViewModelCollection(modules);

            //根据模块页签切换时模块列表自动切换
            this.Workspace.WindowActived += (s, e) =>
            {
                if (e.ActiveWindow != null)
                {
                    this._userModulesView.SelectSingle(e.ActiveWindow.Title);
                }
            };
            this.Workspace.WindowClosed += (s, e) =>
            {
                this._userModulesView.Deselect(e.DeactiveWindow.Title);
            };
        }

        /// <summary>
        /// 获取当前用户可查看的 模块列表数据源
        /// 使用权限进行过滤
        /// </summary>
        /// <returns></returns>
        private IList<ModuleMeta> GetUserModules()
        {
            return this._userModules;
        }

        public ModuleViewModelCollection GetUserRootModules()
        {
            return this._userModulesView;
        }

        #endregion

        #region 打开模块的事件

        /// <summary>
        /// 打开指定类型的模块
        /// 
        /// 如果没有权限打开该模块，则弹出提示框。
        /// </summary>
        /// <param name="boType"></param>
        public IWorkspaceWindow OpenModuleOrAlert(string moduleName)
        {
            //在列表中获取指定类型的元数据
            var module = UIModel.Modules.FindModule(moduleName);
            if (module == null) throw new ArgumentNullException("没有找到对应的模块：" + moduleName);

            return this.OpenModuleOrAlert(module);
        }

        /// <summary>
        /// 打开指定类型的模块
        /// 
        /// 如果没有权限打开该模块，则弹出提示框。
        /// </summary>
        /// <param name="entityType"></param>
        public IWorkspaceWindow OpenModuleOrAlert(Type entityType)
        {
            //在列表中获取指定类型的元数据
            var module = UIModel.Modules.FindModule(entityType);
            if (module == null) throw new ArgumentNullException("没有找到对应的模块：" + entityType.FullName);

            return this.OpenModuleOrAlert(module);
        }

        /// <summary>
        /// 打开某个模块。
        /// 
        /// 如果没有权限打开该模块，则弹出提示框。
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IWorkspaceWindow OpenModuleOrAlert(ModuleMeta module)
        {
            if (!PermissionMgr.Provider.CanShowModule(module))
            {
                App.MessageBox.Show(string.Format(
                    "对不起，此功能需要 [ {0} ] 模块权限，您不具备此权限，如有需要，请与系统管理员联系！",
                    module.Label
                    ));

                return null;
            }

            return this.FindOrCreateWindow(module);
        }

        /// <summary>
        /// 直接打开某个模块。
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public IWorkspaceWindow OpenModule(ModuleMeta module)
        {
            return this.FindOrCreateWindow(module);
        }

        /// <summary>
        /// 创建/找到具体的视图控件。
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private IWorkspaceWindow FindOrCreateWindow(ModuleMeta module)
        {
            var workSpace = this.Workspace;

            var view = workSpace.GetWindow(module.Label);

            //如果已经打开则激活模块，否则新增模块窗体
            if (view == null)
            {
                view = this.CreateModule(module);

                workSpace.Add(view);
            }

            workSpace.TryActive(view);

            return view;
        }

        /// <summary>
        /// 构造模块的窗口
        /// </summary>
        /// <param name="moduleMeta"></param>
        public IWorkspaceWindow CreateModule(ModuleMeta moduleMeta)
        {
            if (moduleMeta == null) throw new ArgumentNullException("moduleMeta");

            //创建 IWindow
            IWorkspaceWindow window = null;
            if (!moduleMeta.IsCustomUI)
            {
                try
                {
                    AutoUI.AggtUIFactory.CurentModule = moduleMeta;

                    //在 WPF 中，TemplateType 属性应该是继承自 CustomModule 的类，表示使用的自定义实体模块类型
                    if (moduleMeta.TemplateType != null)
                    {
                        var module = Activator.CreateInstance(moduleMeta.TemplateType) as CustomTemplate;
                        if (module == null) throw new InvalidProgramException("WPF 中模板类需要从 CustomModule 类继承。");
                        var ui = module.CreateUI(moduleMeta.EntityType);
                        window = new EntityModule(ui, moduleMeta.Label);
                    }
                    else
                    {
                        AggtBlocks blocks = UIModel.AggtBlocks.GetModuleBlocks(moduleMeta);
                        var ui = AutoUI.AggtUIFactory.GenerateControl(blocks);
                        window = new EntityModule(ui, moduleMeta.Label);
                    }
                }
                finally
                {
                    AutoUI.AggtUIFactory.CurentModule = null;
                }
            }
            else
            {
                var templateType = Type.GetType(moduleMeta.CustomUI);
                window = Activator.CreateInstance(templateType) as IWorkspaceWindow;
                if (window == null) throw new InvalidProgramException(templateType.Name + " 类型必须实现 IWindow 接口。");
            }

            AutomationProperties.SetName(window as FrameworkElement, moduleMeta.Label);

            this.OnModuleCreated(new ModuleCreatedArgs(window, moduleMeta));

            return window;
        }

        #endregion

        #region 事件

        public event EventHandler<ModuleCreatedArgs> ModuleCreated;

        private void OnModuleCreated(ModuleCreatedArgs e)
        {
            var handler = this.ModuleCreated;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region 界面区域属性

        /// <summary>
        /// 主工作区
        /// </summary>
        public IWorkspace Workspace
        {
            get { return this.Shell.Workspace; }
        }

        /// <summary>
        /// 主工作区
        /// </summary>
        public IShell Shell
        {
            get
            {
                var shell = this.MainWindow as IShell;
                if (shell == null) throw new ArgumentNullException("shell");
                return shell;
            }
        }

        #endregion
    }

    /// <summary>
    /// 生成某模块时的事件的参数
    /// </summary>
    public class ModuleCreatedArgs : EventArgs
    {
        public ModuleCreatedArgs(IWorkspaceWindow result, ModuleMeta moduleMeta)
        {
            this.ResultWindow = result;
            this.ModuleMeta = moduleMeta;
        }

        /// <summary>
        /// 生成好的模块窗口。
        /// </summary>
        public IWorkspaceWindow ResultWindow { get; private set; }

        public ModuleMeta ModuleMeta { get; private set; }
    }
}