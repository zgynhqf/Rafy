/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：ObjectView的工厂
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
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 通过元数据构造 ObjectView 的工厂类。
    /// </summary>
    public class ObjectViewFactory
    {
        private BlockUIFactory _uiFactory;

        /// <summary>
        /// 块内 UI 工厂。
        /// </summary>
        public BlockUIFactory BlockUIFactory
        {
            get { return this._uiFactory; }
        }

        public ObjectViewFactory(BlockUIFactory uiFactory)
        {
            if (uiFactory == null) throw new ArgumentNullException("factory");

            this._uiFactory = uiFactory;
        }

        /// <summary>
        /// 根据 blockInfo 中的 int 生成 ObjectView。
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public WPFObjectView CreateView(Block block)
        {
            //如果是自定义视图，则自成该类的对象
            if (block.CustomViewType != null)
            {
                var viewType = Type.GetType(block.CustomViewType);
                var view = Activator.CreateInstance(viewType, block.ViewMeta) as WPFObjectView;

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
                    return this.CreateNavigationQueryView(block.ViewMeta);
                }

                if (surrounderType == ConditionBlock.Type)
                {
                    return this.CreateConditionQueryView(block.ViewMeta);
                }
            }

            //根据 BlockType 的值来生成 ObjectView
            var blockType = block.BlockType;
            if (blockType == BlockType.List) { return this.CreateListObjectView(block.ViewMeta); }
            if (blockType == BlockType.Detail) { return this.CreateDetailObjectView(block.ViewMeta); }
            if (blockType == BlockType.Report) { return this.CreateReportView(block.ViewMeta); }
            return this.CreateCustomBlockView(block.ViewMeta, blockType);
        }

        #region ListObjectView

        /// <summary>
        /// 为某个实体类型生成逻辑视图。
        /// </summary>
        /// <param name="entityType">实体类型</param>
        /// <returns></returns>
        public ListObjectView CreateListObjectView(Type entityType)
        {
            var evm = UIModel.Views.CreateBaseView(entityType);

            return this.CreateListObjectView(evm);
        }

        /// <summary>
        /// 为某个实体类型生成逻辑视图。
        /// </summary>
        /// <param name="entityViewInfo">实体类的视图元数据</param>
        /// <returns></returns>
        public ListObjectView CreateListObjectView(EntityViewMeta entityViewInfo, bool isLookup = false)
        {
            var view = new ListObjectView(entityViewInfo);

            if (isLookup) { view.ShowInWhere = ShowInWhere.DropDown; }

            this.InitListView(view);

            this.OnViewCreated(view);

            return view;
        }

        private void InitListView(ListObjectView view)
        {
            //如果是选择视图，则应该使用显示模型来创建控件。
            var resultControl = this._uiFactory.CreateTreeListControl(view.Meta, view.ShowInWhere);

            //为 ListObjectView 初始化 ListEditor
            var listEditor = new MTTGListEditor(view);
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
        public DetailObjectView CreateDetailObjectView(Type entityType)
        {
            var evm = UIModel.Views.CreateBaseView(entityType);

            return this.CreateDetailObjectView(evm);
        }

        /// <summary>
        /// 为某实体类构建一个详细面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public DetailObjectView CreateDetailObjectView(EntityViewMeta entityViewInfo)
        {
            var view = new DetailObjectView(entityViewInfo);

            this.InitDetailView(view);

            this.OnViewCreated(view);

            return view;
        }

        /// <summary>
        /// 为某实体类构建一个导航查询面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public NavigationQueryObjectView CreateNavigationQueryView(EntityViewMeta entityViewInfo)
        {
            var view = new NavigationQueryObjectView(entityViewInfo);

            this.InitDetailView(view);

            view.AttachNewQueryObject();

            this.OnViewCreated(view);

            return view;
        }

        /// <summary>
        /// 为某实体类构建一个条件查询面板视图
        /// </summary>
        /// <param name="entityViewInfo"></param>
        /// <returns></returns>
        public ConditionQueryObjectView CreateConditionQueryView(EntityViewMeta entityViewInfo)
        {
            var view = new ConditionQueryObjectView(entityViewInfo);

            this.InitDetailView(view);

            view.AttachNewQueryObject();

            this.OnViewCreated(view);

            return view;
        }

        private void InitDetailView(DetailObjectView view)
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
        public ReportObjectView CreateReportView(EntityViewMeta evm)
        {
            if (string.IsNullOrEmpty(evm.ReportPath)) { throw new InvalidOperationException("创建报表控件失败：没有为元数据设置需要显示的报表路径。"); }

            var view = new ReportObjectView(evm);

            view.SetControl(new ReportHost()
            {
                ReportPath = evm.ReportPath
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
        private WPFObjectView CreateCustomBlockView(EntityViewMeta evm, BlockType customBlockType)
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
            if (blockType.Id <= 10) throw new InvalidOperationException("自定义块的类型值，需要大于 10。（10 以内的值是 OEA 系统预留块）。");

            this._factories[blockType] = factory;
        }

        #endregion

        #region event ViewCreated

        public event EventHandler<InstanceCreatedEventArgs<ObjectView>> ViewCreated;

        private void OnViewCreated(WPFObjectView view)
        {
            var handler = this.ViewCreated;
            if (handler != null) handler(this, new InstanceCreatedEventArgs<ObjectView>(view));
        }

        #endregion
    }
}