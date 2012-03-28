using System.Collections;
using System.ComponentModel;
using SimpleCsla.Core;

namespace OEA.Library
{
    //概念：
    //当某一个模型不直接显示在视图上时，界面会显示另一类“视图模型”来进行“勾选”操作。
    //这时，如果选择上了，则应该添加一个真正的模型。
    //这时，构造的这个新模型类，如果实现了这个接口。
    //则框架会调用它的SetValues方法来生成一些初始的属性。

    /// <summary>
    /// 内在模型。
    /// </summary>
    public interface IUnderlyModel
    {
        /// <summary>
        /// 当选中某个displayModel时，表示添加一个UnderlyModel，
        /// 这时需要把displayModel的一些属性，复制到本对象中。
        /// </summary>
        /// <param name="displayModel"></param>
        void SetValues(IDisplayModel displayModel);

        /// <summary>
        /// 判断这个内在模型是否匹配指定的diaplayModel
        /// </summary>
        /// <param name="displayModel"></param>
        /// <returns></returns>
        bool IsMappingTo(IDisplayModel displayModel);
    }

    /// <summary>
    /// 一个IUnderlyModel的List
    /// </summary>
    public interface IUnderlyModelList : IBindingList
    {
        /// <summary>
        /// 根据criteria查找出所有用于显示的模型。
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns>
        /// 这里返回的IDisplayModel已经设置好IsSelected，并且同步了二者之间的数据关系。
        /// </returns>
        EntityList GetDisplayModels(IQueryObject criteria);

        /// <summary>
        /// 根据父对象查找出所有子对象的显示模型。
        /// </summary>
        /// <param name="parent">
        /// 拥有这个IUnderlyModelList的父对象。
        /// 如果为null，表示这个对象列表本身就是根对象。
        /// </param>
        /// <returns>
        /// 这里返回的IDisplayModel已经设置好IsSelected，并且同步了二者之间的数据关系。
        /// </returns>
        EntityList GetDisplayModels(Entity parent);
    }

    /// <summary>
    /// 一个可以进行过滤的内在模型列表。
    /// </summary>
    public interface IFilterUnderlyModelList : IUnderlyModelList
    {
        /// <summary>
        /// 根据criteria过滤出所有用于“显示”的模型。
        /// </summary>
        /// <param name="criteria">过滤条件</param>
        /// <returns>
        /// 返回过滤后的数据
        /// </returns>
        IUnderlyModelList FilterByCriteria(IQueryObject criteria);
    }

    /// <summary>
    /// 显示模型
    /// 
    /// 表示一个拥有IsSelected属性的对象
    /// </summary>
    public interface IDisplayModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 选中或者不选中，都会间接地操作内在模型。
        /// </summary>
        bool IsSelected { get; set; }
    }

    ///// <summary>
    ///// 目标对象生成器链表。
    ///// 
    ///// 当选择了可显示的对象时，如果需要生成底层的数据。
    ///// 则这个链表应该可以根据显示对象，生成新的底层对象。
    ///// </summary>
    //public interface IUnderlyObjectFactory : IBindingList
    //{
    //    void AddNew(ISelectable diaplayModel);
    //}
}
