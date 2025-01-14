﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：LogicalView的工厂
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF
{
    /// <summary>
    /// 通过元数据构造 LogicalView 的工厂类。
    /// </summary>
    public class LogicalViewFactory
    {
        private BlockUIFactory _uiFactory;

        /// <summary>
        /// 块内 UI 工厂。
        /// </summary>
        public BlockUIFactory BlockUIFactory
        {
            get { return this._uiFactory; }
        }

        public LogicalViewFactory(BlockUIFactory uiFactory)
        {
            if (uiFactory == null) throw new ArgumentNullException("factory");

            this._uiFactory = uiFactory;
        }

        /// <summary>
        /// 根据 blockInfo 中的 int 生成 LogicalView。
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public LogicalView CreateView(Block block)
        {
            var evm = block.ViewMeta as WPFEntityViewMeta;

            //如果是自定义视图，则自成该类的对象
            if (block.CustomViewType != null)
            {
                var viewType = Type.GetType(block.CustomViewType);
                var view = Activator.CreateInstance(viewType, evm) as LogicalView;

                this.OnViewCreated(view);

                return view;
            }

            //特殊处理环绕块
            var sur = block as SurrounderBlock;
            if (sur != null)
            {
                var surrounderType = sur.SurrounderType;
                if (surrounderType == NavigationBlock.Type)
                {
                    return this.CreateNavigationQueryView(evm);
                }

                if (surrounderType == ConditionBlock.Type)
                {
                    return this.CreateConditionQueryView(evm);
                }
            }

            //根据 BlockType 的值来生成 LogicalView
            var blockType = block.BlockType;
            if (blockType == BlockType.List) { return this.CreateListView(evm); }
            if (blockType == BlockType.Detail) { return this.CreateDetailView(evm); }
            if (blockType == BlockType.Report) { return this.CreateReportView(evm); }
            return this.CreateCustomBlockView(evm, blockType);
        }

        #region ListLogicalView

        /// <summary>
        /// 为某个实体类型生成逻辑视图。
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns></returns>
        public ListLogicalView CreateListView(Type entityType)
        {
            var evm = UIModel.Views.CreateBaseView(entityType) as WPFEntityViewMeta;

            return this.CreateListView(evm);
        }

        /// <summary>
        /// 为某个实体类型生成逻辑视图。
        /// </summary>
        /// <param name="entityViewInfo">实体类的视图元数据</param>
        /// <param name="isLookup"></param>
        /// <param name="properties">如果提供了这个参数，则表示创建的列表控件，只显示给定的这些属性</param>
        /// <returns></returns>
        public ListLogicalView CreateListView(WPFEntityViewMeta entityViewInfo, bool isLookup = false, IList<IManagedProperty> properties = null)
        {
            var view = new ListLogicalView(entityViewInfo);

            if (isLookup) { view.ShowInWhere = ListShowInWhere.DropDown; }

            this.InitListView(view, properties);

            this.OnViewCreated(view);

            return view;
        }

        private void InitListView(ListLogicalView view, IList<IManagedProperty> properties)
        {
            //如果是选择视图，则应该使用显示模型来创建控件。
            var resultControl = this._uiFactory.CreateTreeGrid(view.Meta, view.ShowInWhere, properties);

            //为 ListLogicalView 初始化 ListEditor
            var listEditor = new TreeGridListEditor(view);
            listEditor.SetControl(resultControl);
            view.InitializeEditor(listEditor);
        }

        #endregion

        #region Detail、Condition、Navigation

        /// <summary>
        /// 为某实体类构建一个详细面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public DetailLogicalView CreateDetailView(Type entityType)
        {
            var evm = UIModel.Views.CreateBaseView(entityType) as WPFEntityViewMeta;

            return this.CreateDetailView(evm);
        }

        /// <summary>
        /// 为某实体类构建一个详细面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public DetailLogicalView CreateDetailView(WPFEntityViewMeta entityViewInfo)
        {
            var view = new DetailLogicalView(entityViewInfo);

            this.InitDetailView(view);

            this.OnViewCreated(view);

            return view;
        }

        /// <summary>
        /// 为某实体类构建一个导航查询面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public NavigationQueryLogicalView CreateNavigationQueryView(WPFEntityViewMeta entityViewInfo)
        {
            var view = new NavigationQueryLogicalView(entityViewInfo);

            this.InitDetailView(view);

            view.AttachNewCriteria();

            this.OnViewCreated(view);

            return view;
        }

        /// <summary>
        /// 为某实体类构建一个条件查询面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public ConditionQueryLogicalView CreateConditionQueryView(WPFEntityViewMeta entityViewInfo)
        {
            var view = new ConditionQueryLogicalView(entityViewInfo);

            this.InitDetailView(view);

            view.AttachNewCriteria();

            this.OnViewCreated(view);

            return view;
        }

        private void InitDetailView(DetailLogicalView view)
        {
            var control = this._uiFactory.CreateDetailPanel(view);
            view.SetControl(control);
        }

        #endregion

        #region ReportView

        /// <summary>
        /// 通过指定的实体元数据，创建一个报表视图。
        /// </summary>
        /// <param name="evm"></param>
        /// <returns></returns>
        public ReportLogicalView CreateReportView(WPFEntityViewMeta evm)
        {
            var reportPath = evm.ReportPath;

            if (string.IsNullOrEmpty(reportPath)) { throw new InvalidOperationException("创建报表控件失败：没有为元数据设置需要显示的报表路径。"); }

            //开发期暂时使用的路径，方便开发。
            var relativePath = ConfigurationHelper.GetAppSettingOrDefault("Developing_ReportRootPath");
            if (!string.IsNullOrEmpty(relativePath)) { reportPath = System.IO.Path.Combine(relativePath, reportPath); }

            var view = new ReportLogicalView(evm);

            view.SetControl(new ReportHost()
            {
                ReportPath = reportPath
            });

            this.OnViewCreated(view);

            return view;
        }

        #endregion

        #region 自定义块类型

        private Dictionary<BlockType, ICustomViewFactory> _factories = new Dictionary<BlockType, ICustomViewFactory>();

        /// <summary>
        /// 创建指定自定义块类型对应的视图。
        /// </summary>
        /// <param name="evm"></param>
        /// <param name="customBlockType"></param>
        /// <returns></returns>
        private LogicalView CreateCustomBlockView(EntityViewMeta evm, BlockType customBlockType)
        {
            ICustomViewFactory factory = null;
            if (!this._factories.TryGetValue(customBlockType, out factory)) throw new InvalidOperationException("还没有注册对应块类型 ｛" + customBlockType + "｝ 的视图构造器。请使用 RegisterBlockType 方法注册。");

            var view = factory.CreateView(evm);

            this.OnViewCreated(view);

            return view;
        }

        /// <summary>
        /// 注册一个新的块类型。
        /// </summary>
        /// <param name="blockType">新的块类型</param>
        /// <param name="factory">该块类型对应的视图构造工厂。</param>
        public void RegisterCustomBlock(BlockType blockType, ICustomViewFactory factory)
        {
            if (blockType.Id <= 10) throw new InvalidOperationException("自定义块的类型值，需要大于 10。（10 以内的值是 Rafy 系统预留块）。");

            this._factories[blockType] = factory;
        }

        #endregion

        private void OnViewCreated(LogicalView view)
        {
            //模型标记不可编辑，则初始化只读属性。
            if (view.Meta.NotAllowEdit)
            {
                view.IsReadOnly = ReadOnlyStatus.ReadOnly;
            }

            this.RaiseViewCreated(view);
        }

        #region event ViewCreated

        public event EventHandler<InstanceEventArgs<LogicalView>> ViewCreated;

        private void RaiseViewCreated(LogicalView view)
        {
            var handler = this.ViewCreated;
            if (handler != null) handler(this, new InstanceEventArgs<LogicalView>(view));
        }

        #endregion
    }
}