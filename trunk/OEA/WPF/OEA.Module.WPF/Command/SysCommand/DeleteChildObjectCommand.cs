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


using OEA.Module.WPF.Controls;

using OEA.Library;

namespace OEA.WPF.Command
{
    /// <summary>
    /// 删除子对象
    /// </summary>
    [Command(ImageName = "Delete.bmp", Label = "删除", GroupType = CommandGroupType.Edit)]
    public class DeleteChildObjectCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            if (view == null) { return false; }
            if (!base.CanExecute(view)) { return false; }

            //var bindingList = view.Data as IBindingList;
            //if (bindingList == null || bindingList.AllowRemove == false)
            //{
            //    return false;
            //}

            var glist = view.Data as EntityList;
            if (glist == null || !glist.AllowRemove) { return false; }

            return view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            //获取选中对象的副本（使用副本是因为下面的操作，会改变选中的对象列表。）
            var selectedList = view.SelectedObjects.OfType<Entity>().ToArray();
            var rootList = view.Data as EntityList;
            var isDealingTree = rootList != null && rootList.SupportTree;

            Entity item = null;
            int minIndex = rootList.Count - 1;

            for (int i = selectedList.Length - 1; i >= 0; i--)
            {
                item = selectedList[i];
                if (item.SupportTree)
                {
                    var children = item.GetRecurChildren();
                    //先删除细记录再删除主记录
                    for (int j = children.Count - 1; j >= 0; j--)
                    {
                        var child = children[j] as Entity;
                        minIndex = Math.Min(minIndex, rootList.IndexOf(child));
                        item.ParentList.Remove(child);
                    }
                }
                else
                {
                    minIndex = rootList.IndexOf(item);
                    item.ParentList.Remove(item);
                }
            }

            view.RefreshControl();

            Entity nextSelection = null;
            if (isDealingTree)
            {
                throw new NotImplementedException();//huqf
                //nextSelection = rootList.FindNodeAtSameLevel(item, false) ??
                //    rootList.FindNodeAtSameLevel(item, true);
            }
            else
            {
                //下一选择项是删除元素的前一个
                nextSelection = minIndex >= 1 && minIndex <= rootList.Count
                    ? rootList[minIndex - 1] : null;
            }

            //之前抑制了 CurrentObjectChanged 事件，而中间操作数据的过程中，
            //可能 WPF 界面中的 CollectionView 已经自动把 CurrentObject 定位到同一目标对象上了。
            //这样，只需要直接刷新一下界面就好了。
            if (nextSelection == view.Current)
            {
                view.RefreshCurrentEntity();
            }
            else
            {
                view.Current = nextSelection;
            }
        }
    }
}