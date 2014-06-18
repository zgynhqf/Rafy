/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100218
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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Utils;

namespace Rafy.WPF
{
    /// <summary>
    /// 客户端应用程序API集合
    /// 
    /// 考虑到上层开发人员经常使用此类，所以命名不再使用 RafyApplication/RafyApp/ClientApp
    /// 将来服务端的App类请使用 ServerApp 命名
    /// </summary>
    public partial class App
    {
        #region 模块初始化逻辑

        private ModuleViewModelCollection _userModulesView;

        private bool _userModulesViewLoaded;

        /// <summary>
        /// 获取用户权限内的可用的界面模板列表。
        /// 
        /// 可用于绑定界面菜单等元素。
        /// </summary>
        /// <returns></returns>
        public ModuleViewModelCollection UserRootModules
        {
            get
            {
                if (this._userModulesView == null)
                {
                    this._userModulesView = new ModuleViewModelCollection();
                }
                return this._userModulesView;
            }
            set
            {
                if (this._userModulesViewLoaded)
                {
                    throw new InvalidOperationException("请在登录完成前设置本集合的值。");
                }
                this._userModulesView = value;
            }
        }

        /// <summary>
        /// 初始化用户模块列表
        /// </summary>
        internal void InitUserModules()
        {
            this.HookWorkspace();

            //使用权限进行过滤出所有在界面中显示的模块。
            var userModules = CommonModel.Modules.GetRootsWithPermission().ToList();

            //初始化用户模块列表 - 视图模型
            this.UserRootModules.ReadEntityModules(userModules);

            this._userModulesViewLoaded = true;
        }

        private void HookWorkspace()
        {
            //根据模块页签切换时模块列表自动切换
            var workspace = this.Workspace;
            workspace.WindowActived += (s, e) =>
            {
                //选中唯一一个菜单。
                var win = e.ActiveWindow;
                if (win != null)
                {
                    var module = this.UserRootModules.FindByLabel(win.Title);
                    if (module != null)
                    {
                        try
                        {
                            //由于该模块已经激活，所以不再需要自动触发行为。
                            module._autoDoActionAsSelected = false;
                            module.IsSelected = true;
                        }
                        finally
                        {
                            module._autoDoActionAsSelected = true;
                        }
                    }
                }
            };
            workspace.WindowClosed += (s, e) =>
            {
                //把某个菜单的选中状态清除
                var win = e.DeactiveWindow;
                if (win != null)
                {
                    var module = this.UserRootModules.FindByLabel(win.Title);
                    if (module != null) module.IsSelected = false;
                }
            };
        }

        #endregion

        #region 打开模块的方法

        /// <summary>
        /// 打开指定类型的模块
        /// 
        /// 如果没有权限打开该模块，则弹出提示框。
        /// </summary>
        /// <param name="boType"></param>
        public WorkspaceWindow OpenModuleOrAlert(string moduleName)
        {
            //在列表中获取指定类型的元数据
            var module = CommonModel.Modules.FindModule(moduleName) as WPFModuleMeta;
            if (module == null) throw new ArgumentNullException("没有找到对应的模块：" + moduleName);

            return this.OpenModuleOrAlert(module);
        }

        /// <summary>
        /// 打开指定类型的模块
        /// 
        /// 如果没有权限打开该模块，则弹出提示框。
        /// </summary>
        /// <param name="entityType"></param>
        public WorkspaceWindow OpenModuleOrAlert(Type entityType)
        {
            //在列表中获取指定类型的元数据
            var module = CommonModel.Modules.FindModule(entityType) as WPFModuleMeta;
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
        public WorkspaceWindow OpenModuleOrAlert(WPFModuleMeta module)
        {
            if (!PermissionMgr.CanShowModule(module))
            {
                App.MessageBox.Show(string.Format(
                    "对不起，此功能需要 [ {0} ] 模块权限，您不具备此权限，如有需要，请与系统管理员联系！".Translate(),
                    module.Label.Translate()
                    ));

                return null;
            }

            return this.OpenModule(module);
        }

        /// <summary>
        /// 直接打开某个模块。
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        public WorkspaceWindow OpenModule(WPFModuleMeta module)
        {
            var view = this.FindOrCreateWindow(module);

            this.Workspace.TryActive(view);

            return view;
        }

        /// <summary>
        /// 创建/找到具体的视图控件。
        /// </summary>
        /// <param name="module"></param>
        /// <returns></returns>
        private WorkspaceWindow FindOrCreateWindow(WPFModuleMeta module)
        {
            var workSpace = this.Workspace;

            var view = workSpace.GetWindow(module.Label);

            //如果已经打开则激活模块，否则新增模块窗体
            if (view == null)
            {
                view = this.CreateModule(module);

                workSpace.Add(view);
            }

            return view;
        }

        /// <summary>
        /// 构造模块的窗口
        /// </summary>
        /// <param name="moduleMeta"></param>
        public WorkspaceWindow CreateModule(WPFModuleMeta moduleMeta)
        {
            if (moduleMeta == null) throw new ArgumentNullException("moduleMeta");

            //创建 WorkspaceWindow
            var window = new ModuleWorkspaceWindow
            {
                ModuleMeta = moduleMeta,
                Title = moduleMeta.Label
            };

            var args = new ModuleEventArgs(window);

            if (!moduleMeta.IsCustomUI)
            {
                try
                {
                    AutoUI.AggtUIFactory.PermissionModule = moduleMeta;

                    ControlResult ui = null;
                    //在 WPF 中，TemplateType 属性应该是继承自 UITemplate 的类，表示使用的自定义实体模块类型
                    if (moduleMeta.BlocksTemplate != null)
                    {
                        var module = Activator.CreateInstance(moduleMeta.BlocksTemplate) as UITemplate;
                        if (module == null) throw new InvalidProgramException("WPF 中模板类需要从 UITemplate 类继承。");
                        window.Template = module;
                        window.Template.EntityType = moduleMeta.EntityType;
                        this.OnModuleTemplateCreated(args);

                        window.Blocks = module.GetBlocks();
                        this.OnModuleBlocksCreated(args);

                        ui = module.CreateUI(window.Blocks);
                    }
                    else
                    {
                        AggtBlocks blocks = UIModel.AggtBlocks.GetModuleBlocks(moduleMeta);
                        window.Blocks = blocks;

                        this.OnModuleBlocksCreated(args);
                        ui = AutoUI.AggtUIFactory.GenerateControl(blocks);
                    }
                    window.WindowControl = ui.Control;
                    window.MainView = ui.MainView;

                    Focus(ui);

                    //刚创建的窗体，尝试加载数据。
                    if (moduleMeta.TryAutoLoadData) { this.AsyncLoadListData(ui); }
                }
                finally
                {
                    AutoUI.AggtUIFactory.PermissionModule = null;
                }
            }
            else
            {
                window.WindowControl = Activator.CreateInstance(moduleMeta.CustomUI) as FrameworkElement;
                if (window.WindowControl == null) throw new InvalidProgramException(moduleMeta.CustomUI + " 类型必须是一个 FrameworkElement。");
            }

            AutomationProperties.SetName(window.WindowControl, moduleMeta.Label);

            this.OnModuleCreated(args);

            return window;
        }

        /// <summary>
        /// 把焦点定位到第一个编辑器上
        /// </summary>
        /// <param name="ui"></param>
        private static void Focus(ControlResult ui)
        {
            ui.Control.Loaded += (o, e) =>
            {
                var mainView = ui.MainView;
                var focusedDetailView = mainView.FocusFirstEditor();
                if (focusedDetailView == null)
                {
                    mainView.Control.Focus();
                }
            };
        }

        /// <summary>
        /// 自动加载列表的数据
        /// </summary>
        /// <param name="ui"></param>
        private void AsyncLoadListData(ControlResult ui)
        {
            //如果是个列表，并且没有导航面板，则默认开始查询数据
            var listView = ui.MainView as ListLogicalView;
            if (listView != null &&
                listView.ConditionQueryView == null &&
                listView.NavigationQueryView == null
                )
            {
                listView.DataLoader.LoadDataAsync();
            }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 模块对应的模板创建完成，还没有进行界面生成时的事件。
        /// 
        /// 应用层可监听此事件，完成对模板的修改。
        /// 
        /// 注意，此事件只有模块指定了模板时，才会发生。
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleTemplateCreated;

        private void OnModuleTemplateCreated(ModuleEventArgs e)
        {
            var handler = this.ModuleTemplateCreated;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// 模块对应的聚合块创建完成，还没有进行界面生成时的事件。
        /// 
        /// 应用层可监听此事件，完成对聚合块的修改。
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleBlocksCreated;

        private void OnModuleBlocksCreated(ModuleEventArgs e)
        {
            var handler = this.ModuleBlocksCreated;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// 模块创建完成时事件。
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleCreated;

        private void OnModuleCreated(ModuleEventArgs e)
        {
            var handler = this.ModuleCreated;
            if (handler != null) handler(this, e);
        }

        /// <summary>
        /// 某模块被激活的事件。
        /// </summary>
        public event EventHandler<ModuleEventArgs> ModuleActivated;

        private void OnModuleActivated(ModuleEventArgs e)
        {
            var handler = this.ModuleActivated;
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
    public class ModuleEventArgs : EventArgs
    {
        public ModuleEventArgs(WorkspaceWindow window)
        {
            this.Window = window;
        }

        /// <summary>
        /// 生成好的模块窗口。
        /// </summary>
        public WorkspaceWindow Window { get; private set; }
    }
}