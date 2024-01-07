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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF
{
    /// <summary>
    /// 显示一组对象的列表视图
    /// 
    /// 最主要的功能是对某一实体类型生成对应的列表控件（Control属性）。
    /// </summary>
    public class ListLogicalView : LogicalView, IEventListener, IListEditorContext
    {
        /// <summary>
        /// 对应的表格控件。
        /// </summary>
        public new RafyTreeGrid Control
        {
            get { return base.Control as RafyTreeGrid; }
        }

        #region 构造，初始化

        /// <summary>
        /// ListLogicalView 主要使用 ListEditor 来生成并控制控件
        /// </summary>
        private TreeGridListEditor _listEditor;

        /// <summary>
        /// 为某一类型直接构造一个 ListLogicalView
        /// </summary>
        /// <param name="boType"></param>
        internal ListLogicalView(WPFEntityViewMeta entityViewInfo)
            : base(entityViewInfo) { }

        internal void InitializeEditor(TreeGridListEditor listEditor)
        {
            if (listEditor == null) throw new ArgumentNullException("listEditor");
            if (_listEditor != null) throw new InvalidOperationException("不能多次调用此方法。");

            _listEditor = listEditor;
            this.SetControl(listEditor.Control);
        }

        #endregion

        #region 一般属性

        /// <summary>
        /// 是否是显示一个树型的列表
        /// </summary>
        public bool IsShowingTree
        {
            get
            {
                //如果当前对象支持树，或者有树型的孩子，都表示其正在显示为一个树型列表。
                return this.Meta.EntityMeta.IsTreeEntity;
            }
        }

        private ListShowInWhere? _showInWhere;

        /// <summary>
        /// 指示这个 ListLogicalView 正在被显示在哪里。
        /// <remarks>
        /// 请在生成控件时才设置本属性的值，其它情况不要进行设置。
        /// </remarks>
        /// </summary>
        public ListShowInWhere ShowInWhere
        {
            get
            {
                return _showInWhere.GetValueOrDefault(ListShowInWhere.List);
            }
            internal set
            {
                if (_showInWhere.HasValue) throw new InvalidOperationException("只能设置一次。");
                _showInWhere = value;
            }
        }

        /// <summary>
        /// ListEditor中选择的当前对象
        /// </summary>
        public override Entity Current
        {
            get
            {
                //返回控件的值
                return _listEditor.Current as Entity;
            }
            set
            {
                #region 如果数据已经被过滤掉，则把当前选择项清空。

                if (value != null)
                {
                    var filter = this.Filter;
                    if (filter != null && !filter(value)) value = null;
                }

                #endregion

                var oldObj = this.Current;
                if (oldObj != value)
                {
                    #region 检查设置的值在 Data 中。

                    if (value != null)
                    {
                        var list = this.Data;
                        if (list == null)
                        {
                            throw new InvalidOperationException("设置当前项失败。请先设置 Data 属性。");
                        }
                        else
                        {
                            var found = list.EachNode(e => e == value);
                            if (found == null)
                            {
                                throw new InvalidOperationException("设置当前项失败。列表中不存在指定的实体。");
                            }
                        }
                    }

                    #endregion

                    //更新控件上的当前选中对象
                    _listEditor.Current = value;

                    this.OnCurrentChanged();
                }
            }
        }

        #endregion

        #region Data

        /// <summary>
        /// 是否允许加载数据。
        /// 如果有导航项，则只能通过导航项装载数据。
        /// </summary>
        internal override bool CouldLoadDataFromParent()
        {
            return base.CouldLoadDataFromParent() && this.NavigationQueryView == null;
        }

        /// <summary>
        /// 列表视图中的实体列表。
        /// </summary>
        public new IEntityList Data
        {
            get { return base.Data as IEntityList; }
            set { base.Data = value; }
        }

        /// <summary>
        /// 重设置数据源后，当前选择的对象应该清空，重新设置控件的DataContext等
        /// </summary>
        protected override void OnDataChanged()
        {
            if (this.Disposed) return;

            //重设置数据源后，列表就没有当前行了。
            this.Current = null;

            _listEditor.NotifyContextDataChanged();

            base.OnDataChanged();
        }

        /// <summary>
        /// 刷新当前对象（包含View中的。）
        /// </summary>
        protected override void RefreshCurrentEntityCore()
        {
            if (this.Disposed) return;

            var curObj = this.Current;
            if (curObj != null)
            {
                //为了发生 OnSelectedItemChanged 事件处理函数，先设置为 null，再设置回去。
                _listEditor.Current = null;
                _listEditor.Current = curObj;
            }
            else
            {
                //无论何时调用此方法，必须发生以下事件，
                //这样，整个接口的行为才和 LogicalView,DetailLogicalView 一致。
                this.OnCurrentChanged();
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// 列表视图中的控件的 勾选 模式
        /// </summary>
        public CheckingMode CheckingMode
        {
            get { return _listEditor.CheckingMode; }
            set { _listEditor.CheckingMode = value; }
        }

        /// <summary>
        /// 在 CheckingMode 值为 CheckingRow 时，此属性有效。
        /// 它表示选中某行时，树型节点的级联选择行为。
        /// </summary>
        public CheckingCascadeMode CheckingCascadeMode
        {
            get { return _listEditor.CheckingCascadeMode; }
            set { _listEditor.CheckingCascadeMode = value; }
        }

        /// <summary>
        /// 选择的对象集合
        /// 
        /// ListEditor中选择的对象集
        /// </summary>
        public IList<Entity> SelectedEntities
        {
            get { return _listEditor.SelectedEntities; }
        }

        /// <summary>
        /// 选择全部
        /// </summary>
        public void SelectAll()
        {
            _listEditor.SelectAll();
        }

        /// <summary>
        /// 如果是批量操作，可以使用此接口来统一处理最后的事件。
        /// </summary>
        /// <returns></returns>
        public IDisposable BatchSelection()
        {
            return _listEditor.BeginBatchSelection();
        }

        public event EventHandler<SelectedEntityChangedEventArgs> SelectedItemChanged;

        /// <summary>
        /// 声明一个向父类路由的选择实体改变事件。
        /// </summary>
        internal static readonly RoutedViewEvent SelectedItemChangedEvent = RoutedViewEvent.Register(typeof(ListLogicalView), RoutedEventType.ToParent);

        /// <summary>
        /// 选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemChanged(SelectedEntityChangedEventArgs e)
        {
            //界面选择事件，已经引发了CurrentObject的更改，这里需要发生此事件。
            this.OnCurrentChanged();

            //触发外部事件
            var hander = this.SelectedItemChanged;
            if (hander != null) { hander(this, e); }

            this.RaiseRoutedEvent(SelectedItemChangedEvent, e);
        }

        /// <summary>
        /// 勾选项变更事件
        /// </summary>
        public event CheckItemsChangedEventHandler CheckItemsChanged
        {
            add { _listEditor.CheckItemsChanged += value; }
            remove { _listEditor.CheckItemsChanged -= value; }
        }

        #endregion

        #region ReadOnly

        protected override void SetControlReadOnly(ReadOnlyStatus value)
        {
            _listEditor.IsReadOnly = value;
        }

        #endregion

        #region RefreshControl

        /// <summary>
        /// Command 的代码中，对数据操作完毕后，需要调用此方法刷新界面中的控件。
        /// </summary>
        protected override void RefreshControlCore()
        {
            _listEditor.RefreshControl();
        }

        #endregion

        #region ReplaceControl

        /// <summary>
        /// 使用此方法可以把当前 View 的控件替换为所给的控件。
        /// <remarks>
        /// 使用场景：如果期望完全使用全新的表格控件，可以使用这个方法来重新设置视图与控件的绑定。
        /// 例如：要实现动态列需求时，需要不断地重新生成表格控件。
        /// </remarks>
        /// </summary>
        /// <param name="treeGrid"></param>
        public void ReplaceControl(TreeGrid treeGrid)
        {
            var oldControl = this.Control;

            oldControl.ReplaceInParent(treeGrid);
            if (oldControl.ContextMenu != null) { treeGrid.ContextMenu = oldControl.ContextMenu; }

            _listEditor.SetControl(treeGrid);
            this.SetControl(treeGrid);
            _listEditor.NotifyContextDataChanged();
        }

        internal override void OnControlChanged(FrameworkElement control)
        {
            base.OnControlChanged(control);

            WPFMeta.SetLogicalView(control, this);
        }

        #endregion

        #region Sort, Filter, Group

        /// <summary>
        /// 排序字段
        /// </summary>
        public IEnumerable<SortDescription> SortDescriptions
        {
            get { return _listEditor.SortDescriptions; }
            set { _listEditor.SortDescriptions = value; }
        }

        /// <summary>
        /// 根集合分组时使用的属性列表。
        /// 注意，如果是树型对象，则这个属性只会对所有根对象有用。
        /// </summary>
        public IEnumerable<string> RootGroupDescriptions
        {
            get { return _listEditor.RootGroupDescriptions; }
            set { _listEditor.RootGroupDescriptions = value; }
        }

        /// <summary>
        /// 视图的过滤器
        /// </summary>
        public Predicate<Entity> Filter
        {
            get { return _listEditor.Filter; }
            set { _listEditor.Filter = value; }
        }

        #endregion

        #region 显式接口实现

        #region IEventListener Members

        void IEventListener.NotifySelectedItemChanged(object sender, SelectedEntityChangedEventArgs e)
        {
            e.View = this;
            this.OnSelectedItemChanged(e);
        }

        #endregion

        #region IListEditorContext Members

        IEventListener IListEditorContext.EventReporter
        {
            get { return this; }
        }

        #endregion

        #endregion

        #region 方便 CRUD 的方法

        /// <summary>
        /// 判断当前的视图是否可以添加项
        /// </summary>
        /// <returns></returns>
        public bool CanAddItem()
        {
            var list = this.Data;
            if (list != null)
            {
                //如果有导航面板，则必须先给导航值赋值
                var naviView = this.NavigationQueryView;
                if (naviView != null)
                {
                    var criteria = naviView.Current;
                    foreach (var naviProperty in naviView.NavigationProperties)
                    {
                        //naviProperty 是一个引用实体属性
                        var refProperty = RefPropertyHelper.Find(naviProperty.PropertyMeta.ManagedProperty);
                        if (refProperty != null && !refProperty.Nullable)
                        {
                            var value = criteria.GetRefEntity(refProperty);
                            if (value == null) return false;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 在当前级别添加一个新的实体。
        /// </summary>
        /// <param name="refreshView">是否添加完成后刷新整个视图。</param>
        /// <returns></returns>
        public Entity AddNew(bool refreshView)
        {
            if (!this.CanAddItem()) throw new InvalidOperationException("当前视图不能添加任何项。");

            var newEntity = this.CreateNewItem();

            var current = this.Current;
            if (current != null && this.IsShowingTree && current.TreeParent != null)
            {
                newEntity.TreeParent = current.TreeParent;
            }
            else
            {
                this.Data.Add(newEntity);
            }

            if (refreshView)
            {
                this.RefreshControl();

                this.Current = newEntity;
            }

            return newEntity;
        }

        /// <summary>
        /// 树型实体，可以使用这个方法来为当前选中的实体添加一个新的子实体。
        /// </summary>
        /// <param name="refreshView">是否添加完成后刷新整个视图。</param>
        /// <returns></returns>
        public Entity InsertNewChildNode(bool refreshView)
        {
            if (!this.IsShowingTree) { throw new InvalidOperationException("非树型实体不支持使用此方法"); }
            if (!this.CanAddItem()) throw new InvalidOperationException("当前视图不能添加任何项。");

            var current = this.Current;
            if (current == null) throw new InvalidOperationException("当前视图没有选择任何项，无法添加子。");

            var newEntity = this.CreateNewItem();
            newEntity.TreeParent = current;

            if (refreshView)
            {
                this.RefreshControl();

                this.Current = newEntity;
            }

            return newEntity;
        }

        /// <summary>
        /// 创建一个实体，并根据关系设置好其相应的值。
        /// <remarks>
        /// 本方法不会把这个实体加入到视图中，只是创建一个全新的实体。
        /// </remarks>
        /// </summary>
        /// <returns></returns>
        public Entity CreateNewItem()
        {
            var newEntity = Entity.New(this.EntityType);

            //为了避免所有的新建结点都是 -1 的 Id，这里需要重新赋值。
            newEntity.Id = RafyEnvironment.NewLocalId();

            this.InitRefProperties(newEntity);

            this.OnItemCreated(new ListViewItemCreatedEventArgs(newEntity));

            return newEntity;
        }

        #region ListViewItemCreated Event

        /// <summary>
        /// 当本视图使用 <see cref="CreateNewItem"/> 方法创建某个实体时，会发生此事件。
        /// <remarks>
        /// 应用层可以使用此事件，来对
        /// </remarks>
        /// </summary>
        public event EventHandler<ListViewItemCreatedEventArgs> ItemCreated;

        private void OnItemCreated(ListViewItemCreatedEventArgs e)
        {
            var handler = this.ItemCreated;
            if (handler != null) handler(this, e);
        }

        #endregion

        /// <summary>
        /// 设置某些属性
        /// 
        /// 添加记录时把导航关联值加入到当前记录中
        /// </summary>
        /// <param name="newEntity">需要写入值的新创建的实体</param>
        /// <param name="view"></param>
        private void InitRefProperties(Entity newEntity)
        {
            var navigateView = this.NavigationQueryView;
            if (navigateView != null) { navigateView.SyncRefEntities(newEntity); }
            else
            {
                var conditionView = this.ConditionQueryView;
                if (conditionView != null) { conditionView.SyncRefEntities(newEntity); }
            }

            //设置父对象
            var pv = this.Parent;
            if (pv != null)
            {
                var parent = pv.Current;
                if (parent != null)
                {
                    newEntity.SetParentEntity(parent);
                }
            }
        }

        public void ExpandAll()
        {
            if (!this.IsShowingTree) { throw new InvalidOperationException("非树型实体不支持使用此方法"); }

            this.Control.ExpandAll();
        }

        public void CollapseAll()
        {
            if (!this.IsShowingTree) { throw new InvalidOperationException("非树型实体不支持使用此方法"); }

            this.Control.CollapseAll();
        }

        #endregion
        public override void Dispose()
        {
            _listEditor.Dispose();

            base.Dispose();
        }

        //暂时不处理 动态可见性
        ///// <summary>
        ///// 如果这个ListLogicalView是细表生成的，则处理它的可见性。
        ///// </summary>
        //protected override void ResetVisibility()
        //{
        //    //如果是细表生成的，先判断是否可见
        //    if (this.ChildBlock != null)
        //    {
        //        var visibilityIndicator = this.ChildBlock.VisibilityIndicator;
        //        if (visibilityIndicator.IsDynamic)
        //        {
        //            //获取当前View所属的TabItem
        //            TabItem ti = this.Control.GetLogicalParent<TabItem>();
        //            if (ti != null)
        //            {
        //                if (this.CheckParentViewIsEntityParent())
        //                {
        //                    //设置Visible的Binding
        //                    var be = ti.GetBindingExpression(TabItem.VisibilityProperty);
        //                    if (be == null)
        //                    {
        //                        //绑定到IsVisibleAttribute.BindingPath
        //                        Binding visibleBinding = new Binding(visibilityIndicator.PropertyName);
        //                        visibleBinding.NotifyOnTargetUpdated = true;
        //                        Binding.AddTargetUpdatedHandler(ti, (o, e) =>
        //                        {
        //                            if (e.Property == UIElement.VisibilityProperty)
        //                            {
        //                                //http://ipm.grandsoft.com.cn/issues/247400
        //                                //当 DataContext 为空时，Binding 会把 ti.Visibility 设置为默认的 Visible，
        //                                //不需要这样的行为，所以在这里判断如果 DataContext 为空时，直接把可见性设置为元数据中的默认值。
        //                                if (ti.DataContext == null)
        //                                {
        //                                    this.IsVisible = this.ChildBlock.Owner.ChildrenDefaultVisibility;
        //                                    ti.Visibility = this.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        //                                }
        //                                else
        //                                {
        //                                    var tabVisible = ti.Visibility == Visibility.Visible;
        //                                    tabVisible &= this.CheckParentViewIsEntityParent();
        //                                    this.IsVisible = tabVisible;
        //                                }
        //                            }
        //                        });

        //                        visibleBinding.Mode = BindingMode.OneWay;
        //                        visibleBinding.Converter = new BooleanToVisibilityConverter();
        //                        ti.SetBinding(TabItem.VisibilityProperty, visibleBinding);
        //                    }
        //                }
        //                else
        //                {
        //                    this.IsVisible = false;
        //                }

        //                //设置DataContext
        //                ti.DataContext = this.Parent.Current;
        //            }
        //        }
        //        else
        //        {
        //            this.IsVisible = this.ChildBlock.Owner.ChildrenDefaultVisibility ||
        //                this.Parent.Current != null;
        //        }
        //    }
        //}
    }
}