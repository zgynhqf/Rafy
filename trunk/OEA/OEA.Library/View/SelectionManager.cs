/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100525
 * 说明：同步underlyList和displayList之间的关系
 * 原在 OEA.Module.WPF 项目中，
 * 由于IUnderlyModelList.GetDisplayModels方法的职责变大，需要在实体类中直接负责同步，
 * 所以把这个类提取到这个项目下，方便实体类重用。
 * 
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100525
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SimpleCsla;
using OEA.MetaModel;
using OEA.Threading;

namespace OEA.Library
{
    /// <summary>
    /// 同步underlyList和displayList之间的关系
    /// </summary>
    public class SelectionManager
    {
        private IBindingList _underlyList;

        private IList _displayList;

        /// <summary>
        /// 这个值表示是否勾选操作表示存在一条实际的数据。
        /// </summary>
        private bool _meansData;

        /// <summary>
        /// 这个列表中的实体，如果Selected被改变，将不再设置它的孩子节点。
        /// </summary>
        private List<IDisplayModel> _ignoreSetChildren = new List<IDisplayModel>();

        public SelectionManager(IBindingList underlyList, IList displayList)
            : this(underlyList, displayList, true) { }

        /// <summary>
        /// 同步underlyList和displayList之间的关系
        /// </summary>
        /// <param name="underlyList"></param>
        /// <param name="displayList"></param>
        /// <param name="meansData">
        /// 决定勾选check值触发添加记录还是删除记录
        /// true为正向选择，匹配则勾选，勾选时新增对象
        /// 如果为反向，则不匹配勾选，勾选时删除对象
        /// </param>
        public SelectionManager(IBindingList underlyList, IList displayList, bool meansData)
        {
            if (displayList == null) throw new ArgumentNullException("displayList");
            if (underlyList == null) throw new ArgumentNullException("underlyList");

            this._displayList = displayList;
            this._underlyList = underlyList;
            this._meansData = meansData;
        }

        public void DealSelection()
        {
            this.ResetData();

            this.DealSelectionChanged();

            this.LoadRelation();
        }

        /// <summary>
        /// 初始化现在的数据
        /// </summary>
        private void ResetData()
        {
            //给每个对象的IsSelected赋初始值，表示没有数据
            foreach (IDisplayModel displayModel in this._displayList)
            {
                displayModel.IsSelected = !this._meansData;
            }

            //在选择源中勾选已经拥有的目标数据
            for (int i = 0, uc = this._underlyList.Count; i < uc; i++)
            {
                var underlyModel = this._underlyList[i] as IUnderlyModel;

                //查找mappingSrcItem
                for (int j = 0, dc = this._displayList.Count; j < dc; j++)
                {
                    var displayModel = this._displayList[j] as IDisplayModel;

                    //如果都是一样的，则找到了。
                    if (underlyModel.IsMappingTo(displayModel))
                    {
                        //设置为“表示数据”
                        displayModel.IsSelected = this._meansData;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 处理每个对象的INotifyPropertyChanged事件
        /// 
        /// 勾选时需要触发增加或者删除目的数据对象，通过在给View的Data赋值时遍历源数据每条记录的INotifyPropertyChanged事件来处理
        /// </summary>
        private void DealSelectionChanged()
        {
            foreach (IDisplayModel srcItem in this._displayList)
            {
                //不能使用Action<object,PropertyChangedEventArgs>，因为Action<T,T>是可序列化的。
                PropertyChangedEventHandler action = (sender, pe) =>
                {
                    if (pe.PropertyName.EqualsIgnorecase(PropertyConvention.IsSelected))
                    {
                        var displayModel = sender as IDisplayModel;

                        var manager = new UnderlyObjectManager(this._underlyList as EntityList);

                        var treeNode = sender as Entity;

                        //相同，表示添加；不同，表示删除。
                        bool add = displayModel.IsSelected == this._meansData;
                        if (add)
                        {
                            manager.AddByDisplayModel(displayModel);

                            #region 如果是树节点，把所有的父节点选中。

                            if (treeNode != null)
                            {
                                var parentModel = treeNode.TreeParent as IDisplayModel;
                                if (parentModel != null)
                                {
                                    try
                                    {
                                        if (this._ignoreSetChildren.Contains(parentModel) == false)
                                        {
                                            this._ignoreSetChildren.Add(parentModel);
                                        }

                                        parentModel.IsSelected = true;
                                    }
                                    finally
                                    {
                                        this._ignoreSetChildren.Remove(parentModel);
                                    }
                                }
                            }

                            #endregion
                        }
                        else
                        {
                            manager.DeleteByDisplayModel(displayModel);
                        }

                        //如果没有设置忽略，则把所有的孩子都设置为同样的状态。
                        if (this._ignoreSetChildren.Contains(displayModel) == false)
                        {
                            if (treeNode != null)
                            {
                                foreach (IDisplayModel child in treeNode.TreeChildren)
                                {
                                    child.IsSelected = add;
                                }
                            }
                        }
                    }
                };

                //属性更改触发新增删除对象
                srcItem.PropertyChanged -= action;
                srcItem.PropertyChanged += action;
            }
        }

        #region 异步加载数据的关系

        private void LoadRelation()
        {
            throw new NotImplementedException();//huqf
            //var treeList = this._displayList as EntityList;
            //if (treeList != null && treeList.SupportTree)
            //{
            //    treeList.EnsureTreeRelations();
            //}
        }

        #endregion

        #region private class UnderlyObjectManager

        private class UnderlyObjectManager
        {
            private IBindingList _underlyList;

            public UnderlyObjectManager(EntityList underlyList)
            {
                if (underlyList == null) throw new ArgumentNullException("underlyList");

                this._underlyList = underlyList;
            }

            /// <summary>
            /// 为指定的显示模型添加对应的内在模型
            /// </summary>
            /// <param name="displayModel"></param>
            public void AddByDisplayModel(IDisplayModel displayModel)
            {
                //如果已经存在了这个显示模型对应的内存模型，则直接退出函数。
                var existUnderlyObject = this.FindUnderlyObject(displayModel);
                if (existUnderlyObject != null) return;

                var newObject = this._underlyList.AddNew();

                //如果实现了自定义拷贝接口，则调用这个接口。
                var underlyObject = newObject as IUnderlyModel;
                underlyObject.SetValues(displayModel);
            }

            /// <summary>
            /// 为指定的显示模型删除对应的内在模型
            /// </summary>
            /// <param name="displayModel"></param>
            public void DeleteByDisplayModel(IDisplayModel displayModel)
            {
                var underlyObject = this.FindUnderlyObject(displayModel);
                if (underlyObject != null)
                {
                    this._underlyList.Remove(underlyObject);
                }
                else
                {
                    throw new InvalidOperationException("没有找到匹配项，不能删除");
                }
            }

            /// <summary>
            /// 在列表中查找到对应显示模型的内存模型。
            /// </summary>
            /// <param name="displayModel"></param>
            /// <returns></returns>
            public object FindUnderlyObject(IDisplayModel displayModel)
            {
                return this._underlyList
                    .Cast<IUnderlyModel>()
                    .FirstOrDefault(underlyModel => underlyModel.IsMappingTo(displayModel));
            }
        }

        #endregion
    }
}
