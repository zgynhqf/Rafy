/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100429
 * 说明：选择显示模型和内在模型的实现类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100429
 * 重构静态方法，改为实例方法 胡庆访 20100429
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

using System.ComponentModel;
using System.Windows.Data;
using OpenExpressApp.MetaModel;
using OpenExpressApp.Module.WPF;
using System.Collections;
using Csla.Core;
using System.Diagnostics;
using System.Reflection;
using OpenExpressApp.Module.WPF.Controls;
using System.Threading;
using Csla;
using OpenExpressApp.MetaAttribute;

namespace OpenExpressApp.WPF.Controls
{
    public static class SelectedDataAttached
    {
        /// <summary>
        /// 选择编辑方式下的目标结果数据对象。
        /// 这个数据是真正的对象拥有的数据，而不一定是绑定到控件上面的数据。
        /// </summary>
        public static readonly DependencyProperty DestDataProperty =
            DependencyProperty.RegisterAttached("DestData", typeof(IUnderlyModelList), typeof(SelectedDataAttached),
                new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDestDataChanged)));

        /// <summary>
        /// Handles changes to the DestDataProperty property.
        /// </summary>
        private static void OnDestDataChanged(DependencyObject control, DependencyPropertyChangedEventArgs e)
        {
            if (null == e.NewValue) return;

            var view = control.GetValue(WPFMeta.ObjectViewProperty) as ListObjectView;
            var underlyList = e.NewValue as IBindingList;

            var selectionMgr = new SelectionManager(view, underlyList);
            selectionMgr.MainProcess();
        }

        private class SelectionManager
        {
            private ListObjectView _view;

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

            public SelectionManager(ListObjectView view, IBindingList underlyList)
            {
                if (view == null) throw new ArgumentNullException("view");
                if (underlyList == null) throw new ArgumentNullException("underlyList");

                this._view = view;
                this._underlyList = underlyList;
            }

            public void MainProcess()
            {
                //打开控件的勾选操作功能。
                var control = this._view.Control as ISelectableListControl;
                if (control == null) throw new ArgumentNullException("当前控件不支持勾选操作。");
                control.SelectionEnabled = true;

                this.InitFields();

                this.ResetData();

                this.DealSelectionChanged();

                this.ArrangeDataRelationAsync();
            }

            #region 初始化

            /// <summary>
            /// 初始化所有字段。
            /// </summary>
            private void InitFields()
            {
                this._displayList = this._view.Data as IList;
                var assoAttri = this._view.BOsPropInfo.AssociationAttribute;
                this._meansData = assoAttri.IsForwardSelected;
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

            #endregion

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

                            var manager = new UnderlyObjectManager(this._underlyList, this._view);

                            var treeNode = sender as ITreeNode;

                            //相同，表示添加；不同，表示删除。
                            bool add = displayModel.IsSelected == this._meansData;
                            if (add)
                            {
                                manager.AddByDisplayModel(displayModel);

                                #region 如果是树节点，把所有的父节点选中。

                                if (treeNode != null)
                                {
                                    this.WaitRelation();
                                    var parentModel = treeNode.ParentNode as IDisplayModel;
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
                                    this.WaitRelation();
                                    foreach (IDisplayModel child in treeNode.ChildrenNodes)
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

            private AsyncWorker _asyncWorker;

            /// <summary>
            /// 如果是树，则异步整理数据。
            /// 
            /// 由于本类的操作都要用到树的关系，但是建立这个关系需要一定时间，所以这里采用异步模式。
            /// </summary>
            private void ArrangeDataRelationAsync()
            {
                var treeList = this._displayList as IOrderedTreeNodeCollection;
                if (treeList != null)
                {
                    this._asyncWorker = new AsyncWorker();
                    this._asyncWorker.BeginInvoke(() =>
                    {
                        treeList.EnsureObjectRelations();
                    });
                }
            }

            private void WaitRelation()
            {
                if (this._asyncWorker != null)
                {
                    this._asyncWorker.WaitEnding();
                    this._asyncWorker = null;
                }
            }

            #endregion
        }

        #region private class UnderlyObjectManager

        private class UnderlyObjectManager
        {
            private IBindingList _underlyList;

            private ListObjectView _view;

            public UnderlyObjectManager(IBindingList underlyList, ListObjectView view)
            {
                if (underlyList == null) throw new ArgumentNullException("underlyList");
                if (view == null) throw new ArgumentNullException("view");

                this._underlyList = underlyList;
                this._view = view;
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

                #region 设置导航面板的ID值到新的对象中。

                var navigateView = this._view.NavigateQueryView;
                if (navigateView != null)
                {
                    navigateView.SetReferenceEntity(newObject);
                }

                #endregion

                //设置父对象
                var parent = this._view.Parent;
                if (parent != null)
                {
                    newObject.SetPropertyValue(this._view.GetKeyToParentView(), parent.CurrentID);
                    newObject.SetPropertyValue(this._view.GetKeyObjToParentView(), parent.CurrentObject);
                }

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