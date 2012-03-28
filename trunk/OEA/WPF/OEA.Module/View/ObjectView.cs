using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SimpleCsla.Core;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.View;


namespace OEA
{
    /// <summary>
    /// 对象的视图
    /// 角色是这个视图界面以及其对应数据的控制器。
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public abstract class ObjectView : IActiveCurrentObjectContext, IDataContext, IObjectView, ICustomParamsHolder
    {
        #region 字段

        /// <summary>
        /// 如果当前View作为其它
        /// </summary>
        private ChildBlock _childBlock;

        private EntityViewMeta _entityViewInfo;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityType">这个视图对应这个业务模型类型</param>
        protected ObjectView(EntityViewMeta entityViewInfo)
        {
            if (entityViewInfo == null) throw new ArgumentNullException("entityViewInfo");

            this._entityViewInfo = entityViewInfo;

            this.EnableResetVisibility = true;
            this._relations = new RelationViewCollection(this);
        }

        #region 公有属性

        /// <summary>
        /// Meta Model
        /// </summary>
        public EntityViewMeta Meta
        {
            get { return this._entityViewInfo; }
        }

        /// <summary>
        /// 为这个View加载数据的对象
        /// </summary>
        public IViewDataLoader DataLoader { get; set; }

        /// <summary>
        /// 这个视图作为子视图时，对应的父Model中的属性名称
        /// </summary>
        public string PropertyName
        {
            get
            {
                if (this._childBlock == null) return null;
                return this._childBlock.ChildrenPropertyName;
            }
        }

        /// <summary>
        /// 如果当前View作为其它
        /// </summary>
        public ChildBlock ChildBlock
        {
            get { return this._childBlock; }
            set
            {
                if (this._childBlock != null) { throw new InvalidOperationException("已经初始化完成，操作失败。只支持设置一次。"); }

                this._childBlock = value;
            }
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

        public void AttachBehavior<T>()
            where T : ViewBehavior, new()
        {
            this.AttachBehavior(new T());
        }

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

        public event EventHandler CurrentObjectChanged;

        /// <summary>
        /// 刷新CurrentObject
        /// 子类实现
        /// </summary>
        public abstract void RefreshCurrentEntity();

        /// <summary>
        /// CurrentObjectChanged事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnCurrentObjectChanged()
        {
            this.ResetChildrenVisibility();

            this.ResetChildrenData();

            foreach (var r in this._relations) r.OnOwnerCurrentObjectChanged();

            var handler = this.CurrentObjectChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region Data

        public event EventHandler DataChanged;

        private object _data;

        /// <summary>
        /// 绑定的对象
        /// </summary>
        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (this._data != value)
                {
                    this.OnDataChanging();
                    this._data = value;
                    this.OnDataChanged();
                }
                else
                {
                    this.OnDataReseting();
                }
            }
        }

        protected virtual void OnDataChanging()
        {
            foreach (var r in this._relations) r.OnOwnerDataChanging();
        }

        protected virtual void OnDataChanged()
        {
            foreach (var r in this._relations) r.OnOwnerDataChanged();

            if (this.DataChanged != null)
            {
                this.DataChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 同一个Data被再次设置
        /// </summary>
        protected virtual void OnDataReseting()
        {
            foreach (var r in this._relations) r.OnOwnerDataReseting();
        }

        #endregion

        #region Load Data

        public bool CanLoadDataFromParent()
        {
            return this.CouldLoadDataFromParent();
        }

        /// <summary>
        /// 是否允许加载数据
        /// </summary>
        protected virtual bool CouldLoadDataFromParent()
        {
            //此属性为真时，表示父ObjectView对象正在执行ResetChildrenVisibility方法，
            //这时，子对象可以不进行耗时的数据加载工作，因为父ObjectView对象会紧接着执行ResetChildrenData方法。
            if (this._isResetingVisibility) return false;

            //父视图的当前对象必须不为空
            if (this._parent == null || this._parent._control == null) return false;
            var parentObj = this._parent.Current;
            if (parentObj == null) return false;

            return this.CheckParentViewIsEntityParent();
        }

        /// <summary>
        /// 检测当前视图是否直接挂在和实体类相同结构的聚合父视图下。
        /// 
        /// 例如，当 C 类的聚合父类 B 和更上一层的聚合父类 A 显示在同一个树型控件时，导致 C 的视图直接挂接在 A 视图下，这时，应该返回 false。
        /// </summary>
        /// <returns></returns>
        protected bool CheckParentViewIsEntityParent()
        {
            bool result = true;

            var parentObj = this._parent.Current;
            var parentMeta = this.Meta.EntityMeta.AggtParent;
            if (parentMeta != null && parentObj != null)
            {
                var parentType = parentObj.GetType();
                result = parentMeta.EntityType.IsAssignableFrom(parentType);
            }

            return result;
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public virtual void LoadDataFromParent()
        {
            if (this.CouldLoadDataFromParent())
            {
                this.Data = this.GetRawChildrenData();
            }
        }

        protected object GetRawChildrenData()
        {
            return this._parent.Current.GetLazyChildren(this._childBlock.ChildrenProperty);
        }

        /// <summary>
        /// 绑定这个视图的子视图
        /// 
        /// 把CurrentObject中对应每个子视图的属性，取值出来，并赋值给子视图
        /// </summary>
        /// <param name="view"></param>
        private void ResetChildrenData()
        {
            foreach (var child in this._childrenViews)
            {
                //把CurrentObject中对应每个子视图的属性，取值出来，并赋值给子视图
                //child.Data = (this.CurrentObject as BusinessBase).GetPropertyValue(child.PropertyName);
                //考虑细表大采用懒加载属性时，这里只清空细表，在TabItem可见时更新当前ChildView.Data，以免一次全部装载所有细表
                child.Data = null;

                //为活动状态时才加载数据
                if (child._isActive)
                {
                    child.LoadDataFromParent();
                }
            }
        }

        #endregion

        #region IsVisible

        /// <summary>
        /// 表示当前的ObjectView对象是否正在被父ObjectView对象调用它的ResetVisibility方法。
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

        public event EventHandler IsVisibleChanged;

        protected virtual void OnIsVisibleChanged()
        {
            if (this.IsVisibleChanged != null)
            {
                this.IsVisibleChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 是否启用当前视图的 ResetVisibility 行为
        /// 
        /// 如果为 true，当父视图的当前对象改变时，会根据对象的属性值来设置子视图的可见性。
        /// 如果为 false，则可见性只可通过手工改变 IsVisible 属性来改变。
        /// </summary>
        public bool EnableResetVisibility { get; set; }

        private void ResetChildrenVisibility()
        {
            foreach (var childView in this._childrenViews)
            {
                if (childView.EnableResetVisibility)
                {
                    try
                    {
                        childView._isResetingVisibility = true;
                        childView.ResetVisibility();
                    }
                    finally
                    {
                        childView._isResetingVisibility = false;
                    }
                }
            }
        }

        /// <summary>
        /// 根据当前视图的 IsVisible 属性重设控件的可见性。
        /// 当父视图的当前对象改变时，会根据对象的属性值来设置子视图的可见性。
        /// </summary>
        protected virtual void ResetVisibility() { }

        #endregion

        #region IsActive

        private bool _isActive;

        /// <summary>
        /// 当前视图是否处于活动状态，TabItem处于当前页签时为isactive。
        /// 默认false
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

                if (value)
                {
                    //加载数据
                    if (this._data == null)
                    {
                        this.LoadDataFromParent();
                    }
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
        private void ActiveChild(ObjectView child)
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
        protected virtual void EnsureChildActived()
        {
            //如果已经有视图被激活，忽略此次请求。
            if (this._childrenViews.Any(c => c.IsActive)) return;

            this.ActiveDefaultChild();
        }

        /// <summary>
        /// 激活一个默认的孩子视图
        /// </summary>
        private void ActiveDefaultChild()
        {
            //找到第一个可见的孩子，激活它。
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
        private void DeactiveChild(ObjectView child)
        {
            this.EnsureChildActived();
        }

        #endregion

        #region Parent View & Child View

        private ObjectView _parent;

        private ObjectViewCollection _childrenViews = new ObjectViewCollection();

        /// <summary>
        /// 可以包含多个细表ListView
        /// </summary>
        public ObjectViewCollection ChildrenViews
        {
            get { return this._childrenViews; }
        }

        /// <summary>
        /// 作为子视图和查询视图时有Parent
        /// </summary>
        public ObjectView Parent
        {
            get
            {
                return this._parent;
            }
            set
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
        /// <param name="type">指定的视图</param>
        /// <param name="recur">是否递归在子控件中查询</param>
        /// <returns></returns>
        public ObjectView GetChildView(Type type, bool recur = false)
        {
            CommonModel.Entities.ReplaceIfOverrided(ref type);

            return this.GetChildViewCore(type, recur);
        }

        private ObjectView GetChildViewCore(Type type, bool recur)
        {
            foreach (var item in this._childrenViews)
            {
                if (item.EntityType == type) return item;
                if (recur)
                {
                    var view = item.GetChildViewCore(type, recur);
                    if (view != null) return view;
                }
            }

            return null;
        }

        /// <summary>
        /// 查找根对象视图
        /// </summary>
        /// <returns></returns>
        public ObjectView GetRootView()
        {
            var root = this;

            while (root._parent != null) { root = root._parent; }

            return root;
        }

        /// <summary>
        /// 查找指定类型的父视图
        /// </summary>
        /// <typeparam name="TParent">要找的父视图所对应的类型</typeparam>
        /// <returns></returns>
        public ObjectView GetParentView<TParent>()
        {
            return this.GetParentView(typeof(TParent));
        }

        /// <summary>
        /// 查找指定类型的父视图
        /// </summary>
        /// <param name="type">要找的父视图所对应的类型</param>
        /// <returns></returns>
        public ObjectView GetParentView(Type type)
        {
            if (this._parent == null) return null;

            if (type.IsAssignableFrom(this._parent.EntityType)) return this._parent;

            //多对象树形可能存在多个type，对于子对象来说，Parent可能指向同一个View
            if (type.IsAssignableFrom(this.EntityType)) return this;

            return this._parent.GetParentView(type);
        }

        #endregion

        #region RelationsViews

        private RelationViewCollection _relations;

        public void SetRelation(RelationView relation)
        {
            var exist = this._relations.FirstOrDefault(r => r.SurrounderType == relation.SurrounderType);
            if (exist != null)
            {
                this._relations.Remove(exist);
            }

            this._relations.Add(relation);
        }

        public ObjectView TryFindRelation(SurrounderType relationType)
        {
            return this.TryFindRelation(relationType.GetDescription());
        }

        public ObjectView TryFindRelation(string relationType)
        {
            if (this._relations.Count > 0)
            {
                var surrounder = this._relations.FirstOrDefault(s => s.SurrounderType == relationType);
                if (surrounder != null)
                {
                    return surrounder.View;
                }
            }

            return null;
        }

        #endregion

        ///// <summary>
        ///// 在界面中使用新的视图替换当前视图
        ///// </summary>
        ///// <param name="newView"></param>
        //public void ReplaceBy(ObjectView newView)
        //{
        //    if (newView == null) throw new ArgumentNullException("newView");

        //    this.ReplaceByViewCore(newView);
        //}

        //protected virtual void ReplaceByViewCore(ObjectView newView)
        //{
        //    if (this._parent != null)
        //    {
        //        var index = this._parent._childrenViews.IndexOf(this);
        //        this._parent._childrenViews[index] = newView;
        //        newView._parent = this._parent;
        //    }

        //    foreach (var childView in this._childrenViews.ToArray())
        //    {
        //        childView.Parent = newView;
        //    }

        //    //todo
        //    //替换后，之前的挂接的事件都丢失了。
        //    //可以把相关的事件都赋值过来，暂时未完成。
        //}

        #region MouseDoubleClick

        public event EventHandler MouseDoubleClick;

        /// <summary>
        /// 子类通知这个ObjectView控件发生了MouseDoubleClick事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMouseDoubleClick(EventArgs e)
        {
            if (MouseDoubleClick != null)
            {
                MouseDoubleClick(this, e);
            }
        }

        #endregion

        #region Control

        private object _control;

        /// <summary>
        /// 如果还没有创建Control，则调用CreateControl进行创建。
        /// </summary>
        public object Control
        {
            get { return this._control; }
        }

        /// <summary>
        /// 创建当前ObjectView关联的Control
        /// 子类实现
        /// </summary>
        /// <returns></returns>
        public virtual void SetControl(object control)
        {
            if (control == null) throw new ArgumentNullException("control");

            this._control = control;
        }

        #endregion

        #region Event Extesion

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

        #region IObjectView Members

        /// <summary>
        /// 当前View对应这个业务模型类型
        /// </summary>
        public Type EntityType
        {
            get { return this._entityViewInfo.EntityType; }
        }

        #endregion

        #region 扩充的参数

        private Dictionary<string, object> _customParams = new Dictionary<string, object>();

        /// <summary>
        /// 获取指定参数的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public T TryGetCustomParams<T>(string paramName)
        {
            object result;

            if (this._customParams.TryGetValue(paramName, out result)) { return (T)result; }

            return default(T);
        }

        /// <summary>
        /// 设置自定义参数
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="value"></param>
        public void SetCustomParams(string paramName, object value)
        {
            this._customParams[paramName] = value;
        }

        public IEnumerable<KeyValuePair<string, object>> GetAllCustomParams()
        {
            return this._customParams;
        }

        #endregion

        protected virtual string DebuggerDisplay
        {
            get { return this.GetType().Name + " : " + this.EntityType.Name; }
        }
    }
}