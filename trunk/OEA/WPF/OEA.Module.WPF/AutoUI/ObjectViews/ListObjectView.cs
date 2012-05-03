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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;

using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OEA.ManagedProperty;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 显示一组对象的列表视图
    /// 
    /// 最主要的功能是对某一实体类型生成对应的列表控件（Control属性）。
    /// </summary>
    public class ListObjectView : WPFObjectView, IListObjectView, IEventListener, IListEditorContext
    {
        #region 构造，初始化

        /// <summary>
        /// ListObjectView 主要使用 ListEditor 来生成并控制控件
        /// </summary>
        private MTTGListEditor _listEditor;

        /// <summary>
        /// 为某一类型直接构造一个 ListObjectView
        /// </summary>
        /// <param name="boType"></param>
        internal protected ListObjectView(EntityViewMeta entityViewInfo)
            : base(entityViewInfo) { }

        internal void InitializeEditor(MTTGListEditor listEditor)
        {
            if (listEditor == null) throw new ArgumentNullException("listEditor");
            if (this._listEditor != null) throw new InvalidOperationException("不能多次调用此方法。");

            this._listEditor = listEditor;
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
                //return evm.EntityMeta.IsTreeEntity() || evm.TreeChildPropertyInfo != null;
            }
        }

        private ShowInWhere? _showInWhere;

        /// <summary>
        /// 指示这个 ListObjectView 正在被显示在哪里
        /// 目前只支持两个值：List,Lookup
        /// </summary>
        public ShowInWhere ShowInWhere
        {
            get
            {
                return this._showInWhere.GetValueOrDefault(ShowInWhere.List);
            }
            set
            {
                if (this._showInWhere.HasValue) throw new InvalidOperationException("只能设置一次。");
                if (value != ShowInWhere.List && value != ShowInWhere.DropDown) throw new ArgumentOutOfRangeException("propertyFilter");

                this._showInWhere = value;
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
                return this._listEditor.Current as Entity;
            }
            set
            {
                //如果数据已经被过滤掉，则把当前选择项清空。
                if (value != null)
                {
                    var filter = this.Filter;
                    if (filter != null && !filter(value)) value = null;
                }

                var oldObj = this.Current;
                if (oldObj != value)
                {
                    //更新控件上的当前选中对象
                    this._listEditor.Current = value;

                    this.OnCurrentObjectChanged();
                }
            }
        }

        /// <summary>
        /// 获取或设置控件当前的只读状态
        /// </summary>
        public bool IsReadOnly
        {
            get { return this._listEditor.IsReadOnly; }
            set { this._listEditor.IsReadOnly = value; }
        }

        #endregion

        #region ObjectView Relations

        /// <summary>
        /// 这个ListView可能会有一个 查询面板View
        /// CondtionQueryView 和 NavigateQueryView 只能一个不为空
        /// </summary>
        public ConditionQueryObjectView CondtionQueryView
        {
            get
            {
                return this.Relations.Find(ConditionBlock.Type) as ConditionQueryObjectView;
            }
        }

        /// <summary>
        /// 这个ListView可能会有导航View
        /// CondtionQueryView 和 NavigateQueryView 只能一个不为空
        /// </summary>
        public NavigationQueryObjectView NavigationQueryView
        {
            get
            {
                return this.Relations.Find(NavigationBlock.Type) as NavigationQueryObjectView;
            }
        }

        ///// <summary>
        ///// 这个ListView可能还会关联一个DetailView用于显示某一行。
        ///// </summary>
        //public DetailObjectView DetailView
        //{
        //    get
        //    {
        //        return this.TryFindRelation(DefaultSurrounderTypes.ListDetail.Detail) as DetailObjectView;
        //    }
        //}

        #endregion

        #region Data

        /// <summary>
        /// 是否允许加载数据:如果有导航项，则通过导航项装载数据
        /// </summary>
        protected override bool CouldLoadDataFromParent()
        {
            return base.CouldLoadDataFromParent() && this.NavigationQueryView == null;
        }

        /// <summary>
        /// Data 肯定是一组数据，所以返回类型是 IBindingList
        /// </summary>
        public new EntityList Data
        {
            get { return base.Data as EntityList; }
            set
            {
                Debug.Assert(value == null || value is IBindingList, "value == null || value is EntityList");

                base.Data = value;
            }
        }

        /// <summary>
        /// 重设置数据源后，当前选择的对象应该清空，重新设置控件的DataContext等
        /// </summary>
        protected override void OnDataChanged()
        {
            //重设置数据源后，当前选择的对象应该清空。
            this.Current = null;

            this._listEditor.NotifyContextDataChanged();

            base.OnDataChanged();
        }

        /// <summary>
        /// 如果是同一个Data被再次设置，有可能表示此列表中的项有可能被改变，
        /// 此时，同样需要重新绑定一个实际控制界面ListEditor。
        /// </summary>
        protected override void OnDataReseting()
        {
            this._listEditor.NotifyContextDataChanged();

            base.OnDataReseting();
        }

        /// <summary>
        /// 刷新当前对象（包含View中的。）
        /// </summary>
        public override void RefreshCurrentEntity()
        {
            var curObj = this.Current;
            if (curObj != null)
            {
                //为了发生 OnSelectedItemChanged 事件处理函数，先设置为 null，再设置回去。
                this._listEditor.Current = null;
                this._listEditor.Current = curObj;
            }
            else
            {
                //无论何时调用此方法，必须发生以下事件，
                //这样，整个接口的行为才和 ObjectView,DetailObjectView 一致。
                this.OnCurrentObjectChanged();
            }
        }

        #endregion

        #region Selection

        /// <summary>
        /// 控件的 “Check选择” 模式
        /// </summary>
        public CheckingMode CheckingMode
        {
            get { return this._listEditor.CheckingMode; }
            set { this._listEditor.CheckingMode = value; }
        }

        /// <summary>
        /// 在 CheckingMode 值为 CheckingRow 时，此属性有效。
        /// 它表示选中某行时，树型节点的级联选择行为。
        /// </summary>
        public CheckingRowCascade CheckingRowCascade
        {
            get { return this._listEditor.CheckingRowCascade; }
            set { this._listEditor.CheckingRowCascade = value; }
        }

        /// <summary>
        /// 选择的对象集合
        /// 
        /// ListEditor中选择的对象集
        /// </summary>
        public IList<Entity> SelectedEntities
        {
            get { return this._listEditor.SelectedEntities; }
        }

        /// <summary>
        /// 选择全部
        /// </summary>
        public void SelectAll()
        {
            this._listEditor.SelectAll();
        }

        public event EventHandler<SelectedEntityChangedEventArgs> SelectedItemChanged;

        /// <summary>
        /// 声明一个向父类路由的选择实体改变事件。
        /// </summary>
        public static readonly RoutedViewEvent SelectedItemChangedEvent = RoutedViewEvent.Register(typeof(ListObjectView), RoutedEventType.ToParent);

        /// <summary>
        /// 选择事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItemChanged(SelectedEntityChangedEventArgs e)
        {
            e.View = this;

            //界面选择事件，已经引发了CurrentObject的更改，这里需要发生此事件。
            this.OnCurrentObjectChanged();

            //触发外部事件
            var hander = this.SelectedItemChanged;
            if (hander != null) { hander(this, e); }

            this.RaiseRoutedEvent(SelectedItemChangedEvent, e);
        }

        #endregion

        //暂时不处理 动态可见性
        ///// <summary>
        ///// 如果这个ListObjectView是细表生成的，则处理它的可见性。
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

        /// <summary>
        /// 装载数据，考虑 AssociationOperateType.Selected
        /// </summary>
        public override void LoadDataFromParent()
        {
            if (this.CouldLoadDataFromParent())
            {
                var rawData = this.GetRawChildrenData();
                this.Data = rawData;
            }
        }

        #region RefreshControl

        /// <summary>
        /// Command 的代码中，对数据操作完毕后，需要调用此方法刷新界面中的控件。
        /// </summary>
        public void RefreshControl()
        {
            //命令编辑结束，刷新控件。
            this._listEditor.RefreshControl();

            var handler = this.Refreshed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler Refreshed;

        #endregion

        #region ReplaceControl

        /// <summary>
        /// 使用此方法可以把当前 View 的控件替换为所给的控件。
        /// 注意：传入的控件类型应该和当前的控件类型一致。
        /// </summary>
        /// <param name="control"></param>
        public void ReplaceControl(FrameworkElement control)
        {
            var oldControl = this.Control;

            oldControl.ReplaceInParent(control);
            if (oldControl.ContextMenu != null) { control.ContextMenu = oldControl.ContextMenu; }

            this._listEditor.SetControl(control);
            this.SetControl(control);
            this._listEditor.NotifyContextDataChanged();
        }

        public override void SetControl(object control)
        {
            var element = control as FrameworkElement;
            if (element == null) throw new ArgumentException("value 必须继承自 FrameworkElement。");

            base.SetControl(element);

            WPFMeta.SetObjectView(element, this);
        }

        #endregion

        #region Sort, Filter, Group

        /// <summary>
        /// 排序字段
        /// </summary>
        public IEnumerable<SortDescription> SortDescriptions
        {
            get { return this._listEditor.SortDescriptions; }
            set { this._listEditor.SortDescriptions = value; }
        }

        /// <summary>
        /// 根集合分组时使用的属性列表。
        /// 注意，如果是树型对象，则这个属性只会对所有根对象有用。
        /// </summary>
        public IEnumerable<string> RootGroupDescriptions
        {
            get { return this._listEditor.RootGroupDescriptions; }
            set { this._listEditor.RootGroupDescriptions = value; }
        }

        /// <summary>
        /// 过滤器
        /// </summary>
        public Predicate<Entity> Filter
        {
            get { return this._listEditor.Filter; }
            set { this._listEditor.Filter = value; }
        }

        #endregion

        #region 显式接口实现

        #region IListObjectView Members

        void IListObjectView.BindData(int? rootPid)
        {
            this._listEditor.BindData(rootPid);
        }

        #endregion

        #region IEventListener Members

        void IEventListener.NotifyMouseDoubleClick(object sender, EventArgs e)
        {
            this.OnMouseDoubleClick(e);
        }

        void IEventListener.NotifySelectedItemChanged(object sender, SelectedEntityChangedEventArgs e)
        {
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
            if (list != null && list.AllowNew)
            {
                //如果有导航面板，则必须先给导航值赋值
                var naviView = this.NavigationQueryView;
                if (naviView != null)
                {
                    var criteria = naviView.Current;
                    foreach (var naviProperty in naviView.NavigationProperties)
                    {
                        //naviProperty 是一个引用实体属性
                        var refProperty = naviProperty.PropertyMeta.ManagedProperty as IRefProperty;
                        if (refProperty != null && !refProperty.GetMeta(this.EntityType).Nullable)
                        {
                            var value = criteria.GetLazyRef(refProperty).Entity;
                            if (value == null) return false;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 在同级添加一个结点
        /// </summary>
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

        public Entity InsertNewChild(bool refreshView = true)
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
        /// </summary>
        /// <returns></returns>
        public Entity CreateNewItem()
        {
            var newEntity = Entity.New(this.EntityType);

            //为了避免所有的新建结点都是 -1 的 Id，这里需要重新赋值。
            newEntity.Id = OEAEnvironment.NewLocalId();

            this.InitRefProperties(newEntity);

            this.OnListViewItemCreated(new ListViewItemCreatedEventArgs(newEntity));

            return newEntity;
        }

        #region ListViewItemCreated Event

        /// <summary>
        /// 当 ListView 使用 CreateNewItem 创建某个实体时，发生此事件。
        /// </summary>
        public event EventHandler<ListViewItemCreatedEventArgs> ListViewItemCreated;

        protected virtual void OnListViewItemCreated(ListViewItemCreatedEventArgs e)
        {
            var handler = this.ListViewItemCreated;
            if (handler != null) handler(this, e);
        }

        public class ListViewItemCreatedEventArgs : EventArgs
        {
            public ListViewItemCreatedEventArgs(Entity item)
            {
                this.Item = item;
            }

            public Entity Item { get; private set; }
        }

        #endregion

        /// <summary>
        /// 设置某些属性
        /// 
        /// 添加记录时把导航关联值加入到当前记录中
        /// </summary>
        /// <param name="newEntity"></param>
        /// <param name="view"></param>
        private void InitRefProperties(Entity newEntity)
        {
            var navigateView = this.NavigationQueryView;
            if (navigateView != null) { navigateView.SetReferenceEntity(newEntity); }
            else
            {
                var conditionView = this.CondtionQueryView;
                if (conditionView != null) { conditionView.SetReferenceEntity(newEntity); }
            }

            //设置父对象
            newEntity.ResetParentEntity();
        }

        public void ExpandAll()
        {
            if (!this.IsShowingTree) { throw new InvalidOperationException("非树型实体不支持使用此方法"); }

            (this.Control as MultiTypesTreeGrid).ExpandAll();
        }

        public void CollapseAll()
        {
            if (!this.IsShowingTree) { throw new InvalidOperationException("非树型实体不支持使用此方法"); }

            (this.Control as MultiTypesTreeGrid).CollapseAll();
        }

        #endregion
    }
}