using System;
using System.Collections.Generic;
using OEA.Module.View;

using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA
{
    /// <summary>
    /// 某一领域模型的逻辑视图对象
    /// 可能是List、Detail
    /// </summary>
    public interface IObjectView : IActiveCurrentObjectContext, IDataContext, IControlWrapper
    {
        /// <summary>
        /// 当前View对应这个业务模型类型
        /// </summary>
        Type EntityType { get; }

        EntityViewMeta Meta { get; }

        /// <summary>
        /// 为这个View加载数据的对象
        /// </summary>
        IViewDataLoader DataLoader { get; set; }

        ///// <summary>
        ///// 区域的类型
        ///// </summary>
        //RegionType RegionType { get; set; }

        #region Control

        /// <summary>
        /// 当前是否可见。
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// 双击这个view时发生此事件。
        /// </summary>
        event EventHandler MouseDoubleClick;

        #endregion

        #region TreeRelation

        /// <summary>
        /// 当前视图是否处于活动状态
        /// 默认false
        /// </summary>
        bool IsActive { get; set; }

        ///// <summary>
        ///// 可以包含多个细表ListView
        ///// </summary>
        //IList<IObjectView> ChildrenViews { get; }

        //void AddChildView(IObjectView view);

        ////void RemoveChildView(IObjectView view);

        //IObjectView GetChildView(Type type);

        //IObjectView GetChildView(Type type, bool recur);

        ///// <summary>
        ///// 作为子视图和查询视图时有Parent
        ///// </summary>
        //IObjectView Parent { get; set; }

        /// <summary>
        /// 这个视图作为子视图时，对应的父Model中的属性名称
        /// </summary>
        string PropertyName { get; }

        //IObjectView GetParentView<TAncestor>() where TAncestor : IObjectView;

        //IObjectView GetParentView(Type type);

        //IObjectView GetRootView();

        #endregion
    }
}
