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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.Behaviors;
using OEA.Module.WPF.Controls;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 删除一个根对象
    /// </summary>
    [Command(ImageName = "Delete.bmp", Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteBillObjectCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            if (view == null) { return false; }
            if (base.CanExecute(view) == false) { return false; }

            var bindingList = view.Data as IBindingList;
            if (bindingList == null) { return false; }
            //|| bindingList.AllowRemove == false)

            return view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            var entityList = view.Data as EntityList;
            bool isTree = entityList.SupportTree;
            var content = string.Format("删除后将无法恢复，是否确认删除{0}?", isTree ? "当前记录及其子记录" : string.Empty);
            var result = App.MessageBox.Show(content, "确认删除", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                this.Deletetems(view, isTree);
            }
        }

        /// <summary>
        /// 删除树形的记录
        /// 树形记录需要删除所有的孩子记录，所以在判断删除后定位到一下记录时，需要根据所有删除的记录进行计算
        /// </summary>
        /// <param name="view"></param>
        private void Deletetems(ListObjectView view, bool isTree)
        {
            var list = view.Data as EntityList;

            //获取选中对象的副本（使用副本是因为下面的操作，会改变选中的对象列表。）
            var deleteList = new List<Entity>();
            if (isTree)
            {
                foreach (Entity item in view.SelectedObjects)
                {
                    var children = item.GetRecurChildren();
                    //先删除细记录再删除主记录
                    for (int j = children.Count - 1; j >= 0; j--)
                    {
                        var child = children[j] as Entity;
                        deleteList.Add(child);
                    }
                }
            }
            else
            {
                deleteList.AddRange(view.SelectedObjects.OfType<Entity>());
            }
            int deleteCount = deleteList.Count;

            //关闭行为
            var b = view.FindBehavior<SaveAsChangedBehavior>();
            if (b != null) b.SuppressSaveAction = true;

            var nextFocusItem = GetNextFocusItem(list, deleteList) as Entity;

            //遍历删除
            foreach (var item in deleteList) { list.Remove(item); }

            //保存
            RF.Save(list);

            if (b != null) b.SuppressSaveAction = false;

            view.RefreshControl();

            view.Current = nextFocusItem;
        }

        private static object GetNextFocusItem(IBindingList dataList, IList deleteList)
        {
            Object result = null;
            int deleteCount = deleteList.Count;
            if (dataList.Count > deleteCount)
            {
                var lastDeleteItem = deleteList[deleteCount - 1];
                int index = dataList.IndexOf(lastDeleteItem);

                //如果前面有不被删除的结点，则向前找
                if (index >= deleteCount)
                {
                    #region 向前找

                    for (int i = index - 1; i >= 0; i--)
                    {
                        var item = dataList[i];

                        bool notDelete = true;
                        for (int j = deleteCount - 1; j >= 0; j--)
                        {
                            if (deleteList[j] == item)
                            {
                                notDelete = false;
                                break;
                            }
                        }

                        if (notDelete)
                        {
                            result = item;
                            break;
                        }
                    }

                    #endregion
                }
                else
                {
                    //后面一个
                    result = dataList[index + 1];
                }
            }
            return result;
        }
    }
}