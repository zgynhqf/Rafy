using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using AvalonDock;
using SimpleCsla;
using SimpleCsla.Core;
using SimpleCsla.OEA;
using SimpleCsla.Wpf;

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module;
using OEA.Module.WPF;
using OEA.Module.WPF.Editors;

using OEA.Utils;
using OEA.Module.WPF.Controls;
using OEA.Library;

namespace OEA.WPF.Command
{
    [Command(Label = "复制添加", ToolTip = "复制添加", GroupType = CommandGroupType.Edit)]
    public class CopyAndNewCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            if (!base.CanExecute(view)) { return false; }

            var bindingList = view.Data as IBindingList;
            if (bindingList == null || !bindingList.AllowNew) { return false; }

            return true;
        }

        public override void Execute(ListObjectView view)
        {
            this.ExecuteCore(view);
        }

        /// <summary>
        /// 提取这个虚方法用于子类重写。
        /// 不要直接重写Execute方法，因为其中要执行清理数据映射缓存的操作。
        /// </summary>
        /// <param name="view"></param>
        protected virtual void ExecuteCore(ListObjectView view)
        {
            var innerCommand = new InnerCopyAndNewCommand(view);
            innerCommand.Execute();
        }

        /// <summary>
        /// CopyAndNewCommand类的实现必须是无状态的。
        /// 但是由于这里要存储状态，所以申明了这个私有类。
        /// </summary>
        private class InnerCopyAndNewCommand
        {
            private ListObjectView _currentView;

            private bool? _isDealingTree;

            public InnerCopyAndNewCommand(ListObjectView view)
            {
                this._currentView = view;
            }

            /// <summary>
            /// 是否在处理树
            /// </summary>
            private bool IsDealingTree
            {
                get
                {
                    if (!this._isDealingTree.HasValue)
                    {
                        if (this._currentView == null) { return false; }

                        var list = this._currentView.Data as EntityList;
                        this._isDealingTree = list != null && list.SupportTree;
                    }
                    return this._isDealingTree.Value;
                }
            }

            public void Execute()
            {
                var currentObject = this._currentView.Current as Entity;

                //拷贝一个新的对象
                var newObject = this.CopyObject(currentObject, null, true, CloneOptions.NewAggregate());

                this._currentView.RefreshControl();

                this._currentView.Current = newObject;
            }

            /// <summary>
            /// 复制一个指定的对象
            /// 包括所有的孩子（如果是树节点的话），所有的实体属性和细表属性。
            /// </summary>
            /// <param name="oldObject">需要复制的对象</param>
            /// <param name="parentNode">如果不为null，表示需要设置新的对象成为这个对象的孩子</param>
            /// <param name="modifyName">是否在这个对象的Name属性上添加“-新增”</param>
            /// <returns></returns>
            private Entity CopyObject(Entity oldObject, Entity parentNode, bool modifyName, CloneOptions options)
            {
                throw new NotImplementedException();//huqf
                ////添加一个新的空对象
                //Entity newObject = null;
                //if (parentNode != null)
                //{
                //    newObject = this.NewObject(parentNode, true);
                //}
                //else
                //{
                //    newObject = this.NewObject(oldObject, false);
                //}

                //try
                //{
                //    newObject.BeforeCopy();

                //    if (this.IsDealingTree)
                //    {
                //        var newNode = newObject;
                //        //暂存ParentNode
                //        Entity cachedParent = null;
                //        cachedParent = newNode.TreeParentEntity;

                //        //暂存OrderNo
                //        int oldOrderNo = newObject.OrderNo;

                //        //拷贝所有属性值
                //        newObject.Clone(oldObject, options);

                //        newNode.TreeParentEntity = cachedParent;

                //        //还原OrderNo
                //        newObject.OrderNo = oldOrderNo;

                //        //拷贝所有的孩子
                //        var children = oldObject.ChildrenNodes;
                //        for (int i = 0, c = children.Count; i < c; i++)
                //        {
                //            var oldChild = children[i] as Entity;
                //            //复制一个新的节点作为newObject的孩子
                //            var newChild = this.CopyObject(oldChild, newNode, false, options);
                //        }
                //    }
                //    else
                //    {
                //        //拷贝所有属性值
                //        newObject.Clone(oldObject, options);
                //    }

                //    this.TryModifyName(newObject, modifyName);
                //}
                //finally
                //{
                //    newObject.AfterCopy();
                //}

                //return newObject;
            }

            /// <summary>
            /// 构造一个新的对象
            /// 
            /// 如果是在处理树：
            /// 在currentObject的同级最后 或者 作为currentObject的孩子最后，添加这个新的对象
            /// </summary>
            /// <param name="currentObject">作为位置目标的对象</param>
            /// <param name="newAsChild">如果是树的节点，表示是否添加的对象是currentObject的孩子节点</param>
            /// <returns></returns>
            private Entity NewObject(Entity currentObject, bool newAsChild)
            {
                throw new NotImplementedException();//huqf
                //if (currentObject == null) throw new ArgumentNullException("currentObject");

                //var treeList = this._currentView.Data as EntityList;

                ////是否已经选择了一个非根的树节点
                //bool selectedChildNode = currentObject != null && currentObject.TreeParent != null;

                //Entity newObject = null;

                ////构造新的对象
                //if (this.IsDealingTree)
                //{
                //    if (newAsChild)
                //    {
                //        newObject = treeList.CreateNode(currentObject, true, false) as Entity;
                //    }
                //    else
                //    {
                //        #region 同层添加

                //        //新节点应该加在这一层节点的最后
                //        IList<Entity> nodeOfALayer = null;

                //        //如果选择了一个非根的节点
                //        if (selectedChildNode)
                //        {
                //            nodeOfALayer = currentObject.TreeParent.TreeChildren;
                //        }
                //        else
                //        {
                //            if (treeList.Count > 0)
                //            {
                //                //加到最后一个根节点之后
                //                nodeOfALayer = treeList.FindRoots();
                //            }
                //        }

                //        if (nodeOfALayer != null && nodeOfALayer.Count > 0)
                //        {
                //            var targetNode = nodeOfALayer[nodeOfALayer.Count - 1];
                //            newObject = treeList.CreateNode(targetNode, false, false) as Entity;
                //        }

                //        #endregion
                //    }
                //}

                ////没有找到节点作为位置的目标，直接加到链表的最后。
                //if (newObject == null)
                //{
                //    newObject = treeList.AddNew() as Entity;
                //}

                //AddCommand.InitRefProperties(newObject, this._currentView);

                //return newObject;
            }

            /// <summary>
            /// 有必要的话，就修改Name
            /// </summary>
            /// <param name="newObject"></param>
            private void TryModifyName(Entity newObject, bool modifyName)
            {
                if (modifyName)
                {
                    if (newObject is IHasHame)
                    {
                        var no = newObject as IHasHame;
                        no.Name += "-新增";
                    }
                    else
                    {
                        var mp = newObject.FindProperty("Name");
                        if (mp != null && !mp.IsReadOnly)
                        {
                            string name = newObject.GetProperty(mp).ToString();
                            newObject.SetProperty(mp, name + "-新增");
                        }
                    }
                }
            }

            //Debug 用。
            //private void ListenSelfDirtyChange(Entity src)
            //{
            //    src.IsSelfDirtyChanged += new EventHandler(obj_IsSelfDirtyChanged);

            //    BusinessObjectInfo boInfo = ApplicationModel.GetBusinessObjectInfo(src.GetType());

            //    foreach (BusinessObjectsPropertyInfo propInfo in boInfo.BOsPropertyInfos)
            //    {
            //        if (propInfo.NotCopy)
            //        {
            //            continue;
            //        }

            //        var srcList = src.GetPropertyValue(propInfo.Name) as IList;
            //        foreach (Entity oldItem in srcList)
            //        {
            //            ListenSelfDirtyChange(oldItem);
            //        }
            //    }
            //}

            //void obj_IsSelfDirtyChanged(object sender, EventArgs e)
            //{

            //}
        }
    }
}