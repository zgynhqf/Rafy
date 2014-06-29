/*******************************************************
 * 
 * 作者：周金根
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy;
using Rafy.Reflection;
using Rafy.ManagedProperty;

namespace Rafy.WPF
{
    /// <summary>
    /// 实体的逻辑视图。
    /// <remarks>
    /// 视图使用的是 MVC 设计模式。本类充当 MVC 中的控制器（Controller），其中包含控件（View），以及实体对象（Model）的控件。
    /// </remarks>
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class LogicalView : Extendable, IDataContext
    {
        #region 字段

        /// <summary>
        /// 如果当前 View 作为其它视图的子视图时，这个字段表示对应的子块元数据。
        /// </summary>
        private ChildBlock _childBlock;

        private WPFEntityViewMeta _evm;

        #endregion

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="entityViewMeta">这个视图对应这个业务模型类型</param>
        internal LogicalView(WPFEntityViewMeta entityViewMeta)
        {
            if (entityViewMeta == null) throw new ArgumentNullException("entityViewInfo");

            this._evm = entityViewMeta;

            this.EnableResetVisibility = true;
            this._relations = new RelationViewCollection(this);

            this.DataLoader = new ViewDataLoader(this);
        }

        #region 公有属性

        /// <summary>
        /// Meta Model
        /// </summary>
        public WPFEntityViewMeta Meta
        {
            get { return this._evm; }
        }

        /// <summary>
        /// 当前View对应这个业务模型类型
        /// </summary>
        public Type EntityType
        {
            get { return this._evm.EntityType; }
        }

        /// <summary>
        /// 为这个View加载数据的对象
        /// </summary>
        public IAsyncDataLoader DataLoader { get; internal set; }

        /// <summary>
        /// 如果当前View作为其它
        /// </summary>
        public ChildBlock ChildBlock
        {
            get { return this._childBlock; }
            internal set
            {
                if (this._childBlock != null) { throw new InvalidOperationException("已经初始化完成，操作失败。只支持设置一次。"); }

                this._childBlock = value;
            }
        }

        #endregion

        #region Commands

        private ClientCommandCollection _commands = new ClientCommandCollection();

        /// <summary>
        /// 该视图对应的所有命令集合。
        /// </summary>
        public ClientCommandCollection Commands
        {
            get { return this._commands; }
        }

        #endregion

        #region ViewBehaviors

        private List<ViewBehavior> _behaviors;

        //public IList<ViewBehavior> Behaviors
        //{
        //    get
        //    {
        //        this.InitBehaviors();

        //        return this._behaviors.AsReadOnly();
        //    }
        //}

        private void InitBehaviors()
        {
            if (this._behaviors == null)
            {
                this._behaviors = new List<ViewBehavior>();
            }
        }

        /// <summary>
        /// 为这个视图附加一个行为。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AttachBehavior<T>()
            where T : ViewBehavior, new()
        {
            this.AttachBehavior(new T());
        }

        /// <summary>
        /// 为这个视图附加一个行为。
        /// </summary>
        /// <param name="behavior"></param>
        public void AttachBehavior(ViewBehavior behavior)
        {
            if (behavior == null) throw new ArgumentNullException("behavior");

            if (this._behaviors != null &&
                this._behaviors.Any(b => b.GetType() == behavior.GetType()))
            {
                throw new InvalidOperationException("已经附加了此类型的行为，一个类型的行为只能附加一次。");
            }

            this.InitBehaviors();

            this._behaviors.Add(behavior);
            behavior.View = this;

            behavior.Attach();
        }

        /// <summary>
        /// 在当前附加的行为中找到某个行为。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindBehavior<T>()
            where T : ViewBehavior
        {
            if (this._behaviors == null) return null;

            return this._behaviors.OfType<T>().FirstOrDefault();
        }

        #endregion

        #region CurrentObject

        /// <summary>
        /// 当前的对象
        /// 
        /// 空实现，子类实现。
        /// </summary>
        public abstract Entity Current { get; set; }

        /// <summary>
        /// 当前实体变化的事件。
        /// </summary>
        public event EventHandler CurrentChanged;

        /// <summary>
        /// 如果不期望刷新整个控件，而只是想刷新当前实体绑定的界面，可以调用此方法。
        /// </summary>
        public void RefreshCurrentEntity()
        {
            this.RefreshControlCore();
        }

        /// <summary>
        /// 子类实现此方法来实现 <see cref="RefreshCurrentEntity()"/> 的具体逻辑。
        /// </summary>
        protected abstract void RefreshCurrentEntityCore();

        /// <summary>
        /// CurrentObjectChanged事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCurrentChanged()
        {
            this.ResetChildrenVisibility();

            this.ResetChildrenData();

            foreach (var r in this._relations) r.OnOwnerCurrentObjectChanged();

            var handler = this.CurrentChanged;
            if (handler != null) handler(this, EventArgs.Empty);

            InvalidateCommands();
        }

        #endregion

        #region Data

        public event EventHandler DataChanged;

        private IDomainComponent _data;

        /// <summary>
        /// 绑定的对象
        /// </summary>
        public IDomainComponent Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (this._data != value)
                {
                    this.OnDataChanging(_data, value);
                    this._data = value;
                    this.OnDataChanged();
                }
            }
        }

        protected virtual void OnDataChanging(IDomainComponent oldValue, IDomainComponent newValue)
        {
            foreach (var r in this._relations)
            {
                r.OnOwnerDataChanging(oldValue, newValue);
            }
        }

        protected virtual void OnDataChanged()
        {
            foreach (var r in this._relations) r.OnOwnerDataChanged();

            var handler = this.DataChanged;
            if (handler != null) { handler(this, EventArgs.Empty); }

            //数据变化时，需要重新计算界面的 Command 的可使性。
            InvalidateCommands();
        }

        object IDataContext.Data
        {
            get { return this.Data; }
            set { this.Data = value as IDomainComponent; }
        }

        #endregion

        #region Load Data

        /// <summary>
        /// 绑定这个视图的子视图
        /// 
        /// 清空所有子视图的数据，同时重新加载当前激活的子视图的数据。
        /// </summary>
        /// <param name="view"></param>
        private void ResetChildrenData()
        {
            foreach (var child in this._childrenViews)
            {
                child.Data = null;

                //为活动状态时才加载数据。
                //（子属性都是懒加载属性，所以这里只清空细表，在激活（IsActive）时再更新当前 ChildView.Data，
                //以免一次全部装载所有细表数据。）
                if (child._isActive) { child.LoadDataFromParent(); }
            }
        }

        /// <summary>
        /// 从聚合父视图中通过实体类的列表子属性获取懒加载数据。
        /// </summary>
        internal virtual void LoadDataFromParent()
        {
            if (this.CouldLoadDataFromParent())
            {
                this.Data = this.GetRawChildrenData();
            }
        }

        /// <summary>
        /// 是否允许从组合父视图中加载数据
        /// </summary>
        internal virtual bool CouldLoadDataFromParent()
        {
            //此属性为真时，表示父视图对象正在执行 ResetChildrenVisibility 方法，
            //这时，子对象可以不进行耗时的数据加载工作，因为父视图对象会紧接着执行 ResetChildrenData 方法。
            if (this._isResetingVisibility) return false;

            //父视图及其控件、当前对象必须都不为空
            var p = this._parent;
            return p != null && p._control != null && p.Current != null;
        }

        /// <summary>
        /// 通过实体类的列表子属性获取懒加载数据。
        /// </summary>
        /// <returns></returns>
        private EntityList GetRawChildrenData()
        {
            return this._parent.Current.GetLazyList(
                this._childBlock.ChildrenProperty as IListProperty
                );
        }

        #endregion

        #region IsVisible

        /// <summary>
        /// 表示当前的LogicalView对象是否正在被父LogicalView对象调用它的ResetVisibility方法。
        /// </summary>
        private bool _isResetingVisibility = false;

        private bool _isVisible = true;

        /// <summary>
        /// 当前的视图，是否可见。
        /// 
        /// 注意：暂时不支持直接进行设置以控件。set方法暂留。
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return this._isVisible;
            }
            set
            {
                if (this._isVisible != value)
                {
                    this.SetIsVisible(value);

                    if (value)
                    {
                        //保证
                        if (this._parent != null)
                        {
                            this._parent.EnsureChildActived();
                        }
                    }
                    else
                    {
                        //如果不可见，则取消激活。
                        this.IsActive = false;
                    }
                }
            }
        }

        /// <summary>
        /// 直接设置IsVisible的值
        /// </summary>
        /// <param name="value"></param>
        private void SetIsVisible(bool value)
        {
            if (this._isVisible != value)
            {
                this._isVisible = value;

                this.OnIsVisibleChanged();
            }
        }

        /// <summary>
        /// 可见性变更事件。
        /// <remarks>
        /// 界面层会根据此事件来设置视图的可见性。一般情况，只有 TabControl 中的子视图的这个属性可用。
        /// </remarks>
        /// </summary>
        public event EventHandler IsVisibleChanged;

        /// <summary>
        /// 可见性变更事件。
        /// </summary>
        protected virtual void OnIsVisibleChanged()
        {
            var handler = this.IsVisibleChanged;
            if (handler != null) { handler(this, EventArgs.Empty); }
        }

        /// <summary>
        /// 是否启用当前视图的 ResetVisibility 行为。
        /// 默认为 true。
        /// <remarks>
        /// 如果为 true，当父视图的当前对象改变时，会根据对象的属性值来设置子视图的可见性。
        /// 如果为 false，则可见性只可通过手工改变 IsVisible 属性来改变。
        /// </remarks>
        /// </summary>
        public bool EnableResetVisibility { get; set; }

        private void ResetChildrenVisibility()
        {
            foreach (var childView in this._childrenViews)
            {
                if (childView.EnableResetVisibility)
                {
                    childView.ResetVisibility();
                }
            }
        }

        /// <summary>
        /// 根据当前视图的 IsVisible 属性重设控件的可见性。
        /// 当父视图的当前对象改变时，会根据对象的属性值来设置子视图的可见性。
        /// </summary>
        private void ResetVisibility()
        {
            try
            {
                this._isResetingVisibility = true;
                this.IsVisible = this.Parent.Current != null;
            }
            finally
            {
                this._isResetingVisibility = false;
            }
        }

        #endregion

        #region IsActive

        private bool _isActive;

        /// <summary>
        /// 当前视图是否处于活动状态。
        /// 一个视图的子视图列表中，只能有一个子视图是激活状态。。
        /// 默认为：false。
        /// </summary>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (this._isActive != value)
                {
                    if (value)
                    {
                        //Active 的ChildView肯定是可见的。
                        this.SetIsVisible(true);
                    }

                    this.SetIsActive(value);

                    if (this._parent != null)
                    {
                        if (value)
                        {
                            this._parent.ActiveChild(this);
                        }
                        else
                        {
                            //激活一个默认的ChildView
                            this._parent.DeactiveChild(this);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 直接设置IsActive的值
        /// </summary>
        /// <param name="value"></param>
        private void SetIsActive(bool value)
        {
            if (this._isActive != value)
            {
                this._isActive = value;

                //加载数据
                if (value && this._data == null)
                {
                    this.LoadDataFromParent();
                }

                this.OnIsActiveChanged();
            }
        }

        public event EventHandler IsActiveChanged;

        protected virtual void OnIsActiveChanged()
        {
            if (this.IsActiveChanged != null)
            {
                this.IsActiveChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 激活某个孩子视图
        /// 
        /// 同时把其它子视图设置为“未激活”。
        /// </summary>
        /// <param name="child"></param>
        private void ActiveChild(LogicalView child)
        {
            for (int i = 0, c = this._childrenViews.Count; i < c; i++)
            {
                var childView = this._childrenViews[i];
                childView.SetIsActive(childView == child);
            }
        }

        /// <summary>
        /// 当有孩子可见时，激活一个孩子。
        /// </summary>
        private void EnsureChildActived()
        {
            //如果已经有视图被激活，忽略此次请求。
            if (this._childrenViews.Any(c => c.IsActive)) return;

            //激活一个默认的孩子视图：找到第一个可见的孩子，激活它。
            for (int i = 0, c = this._childrenViews.Count; i < c; i++)
            {
                var childView = this._childrenViews[i];
                if (childView.IsVisible)
                {
                    childView.SetIsActive(true);
                    return;
                }
            }
        }

        /// <summary>
        /// 去除某个孩子视图的激活状态
        /// 
        /// 同时把其它子视图设置为“未激活”。
        /// </summary>
        /// <param name="child"></param>
        private void DeactiveChild(LogicalView child)
        {
            this.EnsureChildActived();
        }

        #endregion

        #region IsReadOnly

        private ReadOnlyStatus _isReadOnly = ReadOnlyStatus.Dynamic;

        /// <summary>
        /// 获取或设置控件当前的只读状态
        /// </summary>
        public ReadOnlyStatus IsReadOnly
        {
            get { return this._isReadOnly; }
            set
            {
                if (this._isReadOnly != value)
                {
                    this._isReadOnly = value;

                    this.SetControlReadOnly(value);
                }
            }
        }

        /// <summary>
        /// 子类实现本方法设置控件的只读性。
        /// </summary>
        /// <param name="value"></param>
        protected virtual void SetControlReadOnly(ReadOnlyStatus value)
        {
            throw new NotSupportedException("当前视图不支持设置 IsReadOnly 属性。");
        }

        #endregion

        #region Parent View & Child View

        private LogicalView _parent;

        private LogicalViewCollection _childrenViews = new LogicalViewCollection();

        /// <summary>
        /// 可以包含多个组合子实体类的视图。
        /// </summary>
        public LogicalViewCollection ChildrenViews
        {
            get { return this._childrenViews; }
        }

        /// <summary>
        /// 作为子视图和查询视图时有Parent
        /// </summary>
        public LogicalView Parent
        {
            get
            {
                return this._parent;
            }
            internal set
            {
                if (this._parent != value)
                {
                    if (this._parent != null)
                    {
                        this._parent._childrenViews.Remove(this);
                    }
                    this._parent = value;
                    if (value != null)
                    {
                        value._childrenViews.Add(this);

                        if (value._childrenViews.Count == 1)
                        {
                            this.SetIsActive(true);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查找指定类型的子视图
        /// </summary>
        /// <param name="entityType">指定的视图</param>
        /// <param name="behavior">是否递归在子控件中查询</param>
        /// <returns></returns>
        public LogicalView GetChildView(Type entityType, FindChildViewBehavior behavior = FindChildViewBehavior.DirectChildren)
        {
            return this.GetChildViewCore(entityType, behavior);
        }

        private LogicalView GetChildViewCore(Type entityType, FindChildViewBehavior behavior)
        {
            foreach (var item in this._childrenViews)
            {
                if (item.EntityType == entityType) return item;
                if (behavior == FindChildViewBehavior.Descendants)
                {
                    var view = item.GetChildViewCore(entityType, behavior);
                    if (view != null) return view;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找根对象视图
        /// </summary>
        /// <returns></returns>
        public LogicalView GetRootView()
        {
            var root = this;

            while (root._parent != null) { root = root._parent; }

            return root;
        }

        /// <summary>
        /// 查找指定类型的父视图
        /// </summary>
        /// <param name="entityType">要找的父视图所对应的类型</param>
        /// <returns></returns>
        public LogicalView GetParentView(Type entityType)
        {
            if (this._parent == null) return null;

            if (entityType.IsAssignableFrom(this._parent.EntityType)) return this._parent;

            return this._parent.GetParentView(entityType);
        }

        #endregion

        #region RelationsViews

        private RelationViewCollection _relations;

        /// <summary>
        /// 该视图的所有关系视图。
        /// </summary>
        public RelationViewCollection Relations
        {
            get { return this._relations; }
        }

        /// <summary>
        /// 这个视图如果有条件查询视图，则这个属性返回第一个导航视图。
        /// <remarks>
        /// 许多视图都可以被查询，例如列表、图表、报表。
        /// </remarks>
        /// </summary>
        public ConditionQueryLogicalView ConditionQueryView
        {
            get
            {
                return _relations.Find(ConditionBlock.Type) as ConditionQueryLogicalView;
            }
        }

        /// <summary>
        /// 这个视图如果有导航视图，则这个属性返回第一个导航视图。
        /// <remarks>
        /// 许多视图都可以被查询，例如列表、图表、报表。
        /// </remarks>
        /// </summary>
        public NavigationQueryLogicalView NavigationQueryView
        {
            get
            {
                return _relations.Find(NavigationBlock.Type) as NavigationQueryLogicalView;
            }
        }

        /// <summary>
        /// 如果当前视图是另一个视图的环绕视图，那么这个属性表示拥有这个环绕视图的视图对象。
        /// <remarks>
        /// 如果是已经为本环绕块的主块添加了特殊的关系，那么这个属性将返回 null，需要使用特定的关系来查找该主块。
        /// （例如条件面板对应的结果视图，其实是条件面板的主块。这时在条件面板对应视图对象中，OwnerView 属性将返回 null。）
        /// </remarks>
        /// </summary>
        public LogicalView OwnerView
        {
            get { return _relations.Find(SurrounderBlock.TypeOwner) as LogicalView; }
        }

        ///// <summary>
        ///// 这个ListView可能还会关联一个DetailView用于显示某一行。
        ///// </summary>
        //public DetailLogicalView DetailView
        //{
        //    get
        //    {
        //        return this.TryFindRelation(DefaultSurrounderTypes.ListDetail.Detail) as DetailLogicalView;
        //    }
        //}

        #endregion

        #region Control

        private FrameworkElement _control;

        /// <summary>
        /// 如果还没有创建Control，则调用CreateControl进行创建。
        /// </summary>
        public FrameworkElement Control
        {
            get { return this._control; }
        }

        /// <summary>
        /// 设置当前视图关联的控件。
        /// </summary>
        /// <returns></returns>
        internal void SetControl(FrameworkElement control)
        {
            if (control == null) throw new ArgumentNullException("control");

            this._control = control;

            this.OnControlChanged(control);
        }

        internal virtual void OnControlChanged(FrameworkElement control) { }

        /// <summary>
        /// 外界调用此方法来刷新整个界面上的控件。
        /// <remarks>
        /// LogicalView 支持的控件绑定模式是一种延迟刷新的模式。
        /// 即对 LogicalView 的数据的修改，并不会即时地显示在控件上，只有调用 RefreshControl 之后，界面才会刷新。
        /// 
        /// 一般使用命令编辑结束，调用此方法来刷新控件。
        /// 
        /// 此方法刷新后的控件，会保留当前项（Current）。
        /// </remarks>
        /// </summary>
        public void RefreshControl()
        {
            this.RefreshControlCore();

            this.OnRefreshed();
        }

        /// <summary>
        /// 子类重写此方法实现特定控件的逻辑逻辑。
        /// 此方法需要保留当前项（Current）。
        /// </summary>
        protected abstract void RefreshControlCore();

        /// <summary>
        /// 控件刷新后事件。
        /// </summary>
        public event EventHandler Refreshed;

        /// <summary>
        /// 控件刷新后事件。
        /// </summary>
        protected virtual void OnRefreshed()
        {
            var handler = this.Refreshed;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region RoutedEvent

        /// <summary>
        /// 发生某个路由事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRoutedEvent(object sender, RoutedViewEventArgs e)
        {
            if (e.Event.Type == RoutedEventType.ToParent)
            {
                if (this._parent != null)
                {
                    this._parent.OnRoutedEvent(sender, e);
                }
            }
            else
            {
                for (int i = 0, c = this._childrenViews.Count; i < c; i++)
                {
                    var child = this._childrenViews[i];
                    child.OnRoutedEvent(sender, e);
                }
            }
        }

        /// <summary>
        /// 触发某个路由事件
        /// </summary>
        /// <param name="indicator"></param>
        /// <param name="args"></param>
        protected void RaiseRoutedEvent(RoutedViewEvent indicator, EventArgs args)
        {
            var arg = new RoutedViewEventArgs
            {
                SourceView = this,
                Event = indicator,
                Args = args
            };

            this.OnRoutedEvent(this, arg);
        }

        #endregion

        #region WPF

        private ItemsControl _commandsContainer;

        /// <summary>
        /// 最终布局完成的控件。
        /// 在这个布局控件中，本视图是它的主视图。
        /// </summary>
        public FrameworkElement LayoutControl { get; internal set; }

        /// <summary>
        /// 工具栏
        /// </summary>
        public ItemsControl CommandsContainer
        {
            get { return this._commandsContainer; }
            internal set
            {
                if (value == null) throw new ArgumentNullException("commandsContainer");
                if (this._commandsContainer != null) throw new InvalidOperationException("只能设置一次！");

                this._commandsContainer = value;

                value.SetServicedControl(this.Control);
            }
        }

        private static void InvalidateCommands()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion

        protected virtual string DebuggerDisplay
        {
            get { return this.GetType().Name + " : " + this.EntityType.Name; }
        }
    }

    /// <summary>
    /// 查找孩子视图的行为。
    /// </summary>
    public enum FindChildViewBehavior
    {
        /// <summary>
        /// 只查找直接子。
        /// </summary>
        DirectChildren,
        /// <summary>
        /// 递归查找所有孩子。
        /// </summary>
        Descendants
    }
}