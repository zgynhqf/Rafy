/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AvalonDock;
using OEA.WPF.Command;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.CommandAutoUI;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Layout;
using OEA.WPF;
using System.Windows.Input;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 聚合控件生成器
    /// 
    /// 此类型并不是线程安全的。（由于控件生成只作为界面主线程使用，所以未考虑多线程设计。）
    /// </summary>
    public class AggtControlGenerator
    {
        #region 私有字段

        private ObjectViewFactory _viewFactory;

        /// <summary>
        /// 所有需要执行自动命令的 View 都临时存储在这里。
        /// 
        /// 此类型并不是线程安全的。（由于控件生成只作为界面主线程使用，所以未考虑多线程设计。）
        /// </summary>
        private List<WPFObjectView> _autoCommandViews;

        #endregion

        public AggtControlGenerator(ObjectViewFactory viewFactory)
        {
            if (viewFactory == null) throw new ArgumentNullException("viewFactory");
            this._viewFactory = viewFactory;
        }

        #region 公有接口

        /// <summary>
        /// 根据聚合元数据，生成最终的聚合控件
        /// </summary>
        /// <param name="aggt">
        /// 集合中的第一个，是主区域对应的View</param>
        /// <param name="recurChildren"></param>
        /// <param name="recurSurrounders"></param>
        /// <param name="ownerView"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public ControlResult GenerateControl(AggtBlocks aggt)
        {
            this._autoCommandViews = new List<WPFObjectView>();

            var mainView = this._viewFactory.CreateView(aggt.MainBlock);

            this.CreateCommandsUI(mainView, aggt.MainBlock);

            var result = this.GenerateCompoundControl(aggt, mainView);

            Zoom.EnableZoom(result.Control);

            return result;
        }

        #endregion

        #region 主要逻辑

        /// <summary>
        /// 为聚合对象生成组合控件。
        /// </summary>
        /// <param name="aggt">
        /// 需要生成聚合控件的聚合对象元数据
        /// </param>
        /// <param name="mainView">
        /// 已经生成好的聚合对象 aggt 中的“根”对象所对应的 ObjectView。
        /// </param>
        /// <returns></returns>
        private ControlResult GenerateCompoundControl(AggtBlocks aggt, WPFObjectView mainView)
        {
            var regions = new RegionContainer(aggt);

            //如果不要查询面板，则需要生成主区域
            var viewInfo = aggt.MainBlock.ViewMeta;

            regions.Add(TraditionalRegions.Main, AutoUIHelper.CreateBusyControlResult(mainView));
            if (mainView.CommandsContainer != null)
            {
                regions.Add(TraditionalRegions.CommandsContainer, new ControlResult(mainView.CommandsContainer, mainView));
            }

            //Surrounders
            this.SurroundersToRegions(aggt.Surrounders, mainView, regions);

            //Children
            this.ChildrenToRegions(aggt.Children, mainView, regions);

            //Layout
            var layout = CreateLayoutMethod(aggt);
            var result = layout.Arrange(regions);

            //在 View 中保存最终布局完成的控件。
            mainView.LayoutControl = result;

            //Commands
            if (aggt.MainBlock.BlockType != BlockType.Detail)
            {
                this._autoCommandViews.Add(mainView);
            }

            //返回布局后的整个控件。
            var ui = new ControlResult(result, mainView, aggt);

            this.CreateCommandBindings(ui);

            return ui;
        }

        /// <summary>
        /// 把环绕块生成控件并加入到 Regions 中。
        /// </summary>
        /// <param name="surrounders"></param>
        /// <param name="mainView"></param>
        /// <param name="regions"></param>
        private void SurroundersToRegions(IList<AggtBlocks> surrounders, WPFObjectView mainView, RegionContainer regions)
        {
            foreach (var surrounder in surrounders)
            {
                var surBlock = surrounder.MainBlock as SurrounderBlock;

                var surrounderType = surBlock.SurrounderType;
                var surrounderView = this.CreateSurrounderView(mainView, surBlock);

                //为 Surrouder 生成它的聚合控件
                var surrounderControl = this.GenerateCompoundControl(surrounder, surrounderView);

                regions.Add(surrounderType, surrounderControl);
            }
        }

        protected virtual WPFObjectView CreateSurrounderView(WPFObjectView mainView, SurrounderBlock surrounderBlock)
        {
            var surrounderType = surrounderBlock.SurrounderType;

            //相反的关系类型
            RelationView reverseRelation = null;
            if (surrounderType == ConditionBlock.Type)
            {
                reverseRelation = new RelationView(QueryObjectView.ResultSurrounderType, mainView);
            }
            else if (surrounderType == NavigationBlock.Type)
            {
                reverseRelation = new RelationView(QueryObjectView.ResultSurrounderType, mainView);
            }

            WPFObjectView surrounderView = this._viewFactory.CreateView(surrounderBlock);
            this.CreateCommandsUI(surrounderView, surrounderBlock);

            //直接使用 surrounderType 作为关系的类型，把 surrounderView 添加到 mainView 的关系。
            RelationView relation = null;
            if (surrounderBlock.RelationViewType != null)
            {
                relation = Activator.CreateInstance(
                    surrounderBlock.RelationViewType, surrounderType, surrounderView
                    ) as RelationView;
            }
            else
            {
                relation = new RelationView(surrounderType, surrounderView);
            }
            mainView.Relations.Add(relation);

            //相反的关系设置
            reverseRelation = reverseRelation ?? new RelationView("owner", mainView);
            surrounderView.Relations.Add(reverseRelation);

            return surrounderView;
        }

        /// <summary>
        /// 把孩子块生成控件并加入到 Regions 中。
        /// </summary>
        /// <param name="children"></param>
        /// <param name="mainView"></param>
        /// <param name="regions"></param>
        private void ChildrenToRegions(IList<AggtBlocks> children, WPFObjectView mainView, RegionContainer regions)
        {
            foreach (var child in children)
            {
                var childBlock = child.MainBlock as ChildBlock;

                if (this.NeedPermission && !PermissionMgr.Provider.CanShowBlock(this.CurentModule, childBlock)) continue;

                //生成 childView
                WPFObjectView childView = this._viewFactory.CreateView(childBlock);
                childView.ChildBlock = childBlock;

                //子视图默认不显示，当选中某个父对象时，再重新计算其可见性。
                childView.IsVisible = false;

                childView.Parent = mainView;

                //ChildView Commands Container
                this.CreateCommandsUI(childView, childBlock);

                //Child Control
                var childControl = this.GenerateCompoundControl(child, childView);

                //Region
                regions.AddChildren(childBlock.Label, childControl);

                this.OnChildUICreated(childControl);
            }
        }

        #region ChildUICreated Event

        public event EventHandler<ChildUICreatedEventArgs> ChildUICreated;

        protected virtual void OnChildUICreated(ControlResult childUI)
        {
            var handler = this.ChildUICreated;
            if (handler != null) handler(this, new ChildUICreatedEventArgs(childUI));
        }

        public class ChildUICreatedEventArgs : EventArgs
        {
            public ChildUICreatedEventArgs(ControlResult childUI)
            {
                this.ChildUI = childUI;
            }

            public ControlResult ChildUI { get; private set; }
        }

        #endregion

        #endregion

        #region 处理命令

        private void CreateCommandsUI(WPFObjectView view, Block block)
        {
            if (view == null) throw new ArgumentNullException("view");
            if (block == null) throw new ArgumentNullException("uiInfo");

            var commands = block.ViewMeta.WPFCommands.Where(c => c.IsVisible).ToList();
            if (commands.Count > 0)
            {
                if (this.NeedPermission)
                {
                    commands = commands.Where(c => PermissionMgr.Provider.HasCommand(this.CurentModule, block, c.Name)).ToList();
                }

                //Toolbar
                view.CommandsContainer = this.CreateCommandsContainer();
                this._viewFactory.BlockUIFactory.AppendCommands(
                    view.CommandsContainer, view, commands
                    );
            }
        }

        /// <summary>
        /// 把 CommandBinding 都创建在聚合视图上。
        /// </summary>
        /// <param name="compoundUI"></param>
        private void CreateCommandBindings(ControlResult compoundUI)
        {
            var view = compoundUI.MainView;

            var iptBindings = compoundUI.Control.InputBindings;
            var cmdBindings = compoundUI.Control.CommandBindings;
            foreach (var uiCmd in view.UICommands)
            {
                //添加一个 CommandBinding。
                var binding = CommandRepository.CreateCommandBinding(uiCmd);
                cmdBindings.Add(binding);

                //同时，添加一个 ImputBinding，并把它的 Parameter 设置为 View。
                var gestures = uiCmd.InputGestures;
                if (gestures != null && gestures.Count > 0)
                {
                    if (gestures.Count > 1)
                    {
                        throw new NotSupportedException("暂时不支持复杂快捷健！");
                    }
                    var iptBinding = new InputBinding(uiCmd, gestures[0]);
                    iptBinding.CommandParameter = view;
                    iptBindings.Add(iptBinding);
                }
            }
        }

        protected virtual ItemsControl CreateCommandsContainer()
        {
            var itemsControl = new ItemsControl();
            itemsControl.Style = OEAResources.CommandsContainer;
            return itemsControl;
        }

        #endregion

        #region 帮助类方法

        /// <summary>
        /// 根据元数据创建一个布局的方案
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private static LayoutMethod CreateLayoutMethod(AggtBlocks meta)
        {
            if (meta.Layout.Class != null)
            {
                var type = Type.GetType(meta.Layout.Class);
                var instance = Activator.CreateInstance(type);
                if (instance is ILayoutControl) { return new ControlLayoutMethod(instance as ILayoutControl); }
                if (instance is LayoutMethod) { return instance as LayoutMethod; }

                throw new InvalidProgramException(string.Format(
                    "{0} 类型不能用于布局。原因：WPF 中用于布局类型必须实现 ILayoutControl 接口或者继承自 LayoutMethod 类！",
                    type.FullName));
            }

            var mainBlock = meta.MainBlock;
            var generateType = mainBlock.BlockType;
            if (generateType == BlockType.Detail)
            {
                return new ControlLayoutMethod(new DetailLayout());
            }

            //var entityViewInfo = mainBlock.EntityViewInfo;
            //if (entityViewInfo.EntityInfo.IsDefaultObject &&
            //    entityViewInfo.BusinessObjectAttribute.ModuleType != ModuleType.Query)
            //{
            //    return new TraditionalLayout<NaviListDetailLayoutControl>();
            //}

            return new ControlLayoutMethod(new ListDetailLayout());
            //return new TraditionalLayout<ListDetailPopupChildrenLayoutControl>();
        }

        #endregion

        /// <summary>
        /// 如果此属性不为 null，表示当前正在为某个模块生成界面，这时需要全面检测权限。
        /// </summary>
        internal ModuleMeta CurentModule;

        private bool NeedPermission
        {
            get { return this.CurentModule != null; }
        }
    }
}