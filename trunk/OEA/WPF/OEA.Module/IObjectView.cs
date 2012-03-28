using System;
using System.Collections.Generic;
using OpenExpressApp.Module.View;
using OpenExpressApp.Template;
using OpenExpressApp.Types;

namespace OpenExpressApp
{
    /// <summary>
    /// 控件生成器
    /// </summary>
    public interface IControlGenerator
    {
        /// <summary>
        /// 创建当前ObjectView关联的Control
        /// </summary>
        /// <returns></returns>
        object CreateControl();
    }
    /// <summary>
    /// 可以附加一个控件在这个对象上
    /// </summary>
    public interface IControlWrapper
    {
        /// <summary>
        /// 被包含的控件，如果为null，表示还没有控件包含进来。
        /// </summary>
        object Control { get; }
        /// <summary>
        /// 是否已经包含了控件。
        /// </summary>
        bool HasControl { get; }
    }

    /// <summary>
    /// 某一领域模型的逻辑视图对象
    /// 可能是List、Detail
    /// </summary>
    public interface IObjectView : IActiveCurrentObjectContext, IControlWrapper
    {
        /// <summary>
        /// 当前View对应这个业务模型类型
        /// </summary>
        Type EntityType { get; }
        /// <summary>
        /// Controller
        /// </summary>
        IViewController Controller { get; set; }
        /// <summary>
        /// 当前视图是否处于活动状态
        /// 默认false
        /// </summary>
        bool IsActive { get; set; }
        /// <summary>
        /// 所有的这个View使用的属性Editor
        /// Key：BOType的属性
        /// Value：这个属性使用的编辑器
        /// </summary>
        IDictionary<IEntityPropertyInfo, IPropertyEditor> PropertyEditors { get; }
        /// <summary>
        /// 区域的类型
        /// </summary>
        RegionType RegionType { get; set; }

        /// <summary>
        /// 找到指定属性的Editor。
        /// 如果找不到，则把editor加入并返回。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="editor"></param>
        /// <returns></returns>
        IPropertyEditor AddPropertyEditor(IEntityPropertyInfo info, IPropertyEditor editor);
        /// <summary>
        /// 绑定指定的行为
        /// </summary>
        /// <param name="behavior"></param>
        void AttachBehavior(IViewBehavior behavior);

        #region TreeRelation

        /// <summary>
        /// 可以包含多个细表ListView
        /// </summary>
        IList<IObjectView> ChildrenViews { get; }

        //void AddChildView(IObjectView view);
        ////void RemoveChildView(IObjectView view);
        //IObjectView GetChildView(Type type);
        //IObjectView GetChildView(Type type, bool recur);

        /// <summary>
        /// 作为子视图和查询视图时有Parent
        /// </summary>
        IObjectView Parent { get; set; }
        /// <summary>
        /// 这个视图作为子视图时，对应的父Model中的属性名称
        /// </summary>
        string PropertyName { get; }

        //IObjectView GetParentView<TAncestor>() where TAncestor : IObjectView;
        //IObjectView GetParentView(Type type);
        //IObjectView GetRootView();

        #endregion
    }

    public interface IDetailObjectView : IObjectView
    {
        /// <summary>
        /// 可能会有一个列表对应这个详细视图
        /// </summary>
        IListObjectView ListView { get; }
    }

    public interface IListObjectView : IObjectView, IActiveSelectionContext
    {
        /// <summary>
        /// 这个ListView可能还会关联一个DetailView用于显示某一行。
        /// </summary>
        IDetailObjectView DetailView { get; }
    }
}
