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
using SimpleCsla.Wpf;
using OEA.Command;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module;
using OEA.Module.WPF;
using OEA.Module.WPF.Command;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;

namespace OEA.WPF.Command
{
    public abstract class EditOrderedTreeListViewCommand : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            var list = view.Data;
            return list != null && !list.DisableTreeEditing;
        }
    }

    #region 展开、折叠

    public abstract class ExpandBaseCommand : ListViewCommand
    {
        protected int Depth;

        public override bool CanExecute(ListObjectView view)
        {
            return view.Data != null && view.Data.Count > 0;
        }

        public override void Execute(ListObjectView view)
        {
            var tv = view.Control as MultiTypesTreeGrid;
            foreach (var root in tv.Items)
            {
                tv.ExpandToDepth(root, this.Depth);
            }
        }
    }

    [Command(Group = "展开到", Label = "全部展开", GroupType = CommandGroupType.View)]
    public class ExpandAllCommand : ExpandBaseCommand
    {
        public override void Execute(ListObjectView view)
        {
            MultiTypesTreeGrid tv = view.Control as MultiTypesTreeGrid;
            if (tv != null) { tv.ExpandAll(); }
        }
    }

    [Command(Group = "展开到", Label = "一级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelOneCommand : ExpandBaseCommand
    {
        public ExpandToLevelOneCommand()
        {
            this.Depth = 1;
        }
    }

    [Command(Group = "展开到", Label = "二级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelTwoCommand : ExpandBaseCommand
    {
        public ExpandToLevelTwoCommand()
        {
            this.Depth = 2;
        }
    }

    [Command(Group = "展开到", Label = "三级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelThreeCommand : ExpandBaseCommand
    {
        public ExpandToLevelThreeCommand()
        {
            this.Depth = 3;
        }
    }

    [Command(Group = "展开到", Label = "四级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelFourCommand : ExpandBaseCommand
    {
        public ExpandToLevelFourCommand()
        {
            this.Depth = 4;
        }
    }

    #endregion

    #region 上、下移

    public class MoveCommand : EditOrderedTreeListViewCommand
    {
        protected bool MoveUp;

        public override bool CanExecute(ListObjectView view)
        {
            if (base.CanExecute(view))
            {
                var node = view.Current;

                //找到这个实体所在层级的列表
                IList<Entity> context = null;
                if (node.TreeParent == null)
                {
                    context = view.Data.FindRoots().ToArray();
                }
                else
                {
                    context = node.TreeParent.TreeChildren;
                }

                if (this.MoveUp)
                {
                    return context[0] != node;
                }
                return context.Last() != node;
            }

            return false;
        }

        public override void Execute(ListObjectView view)
        {
            var current = view.Current;
            if (current.TreeParent != null)
            {
                //直接在本层移动子对象结点
                var children = current.TreeParent.TreeChildren;
                var index = children.IndexOf(current);
                children.Remove(current);

                if (this.MoveUp)
                {
                    children.Insert(index - 1, current);
                }
                else
                {
                    children.Insert(index + 1, current);
                }
            }
            else
            {
                var list = view.Data;
                var roots = list.FindRoots().ToList();
                list.Remove(current);

                var index = roots.IndexOf(current);
                if (this.MoveUp)
                {
                    //向上移动时，找到上一根节点，插入到它之前即可
                    var previousRoot = roots[index - 1];
                    var listIndex = list.IndexOf(previousRoot);
                    list.Insert(listIndex, current);
                }
                else
                {
                    //根节点向下移时，需要找到下一根节点的下一根节点，插入到它之前。
                    //此时，如果没有下下根节点，则插入到最后。
                    if (index >= roots.Count - 2)
                    {
                        list.Add(current);
                    }
                    else
                    {
                        var previousRoot = roots[index + 2];
                        var listIndex = list.IndexOf(previousRoot);
                        list.Insert(listIndex, current);
                    }
                }
            }

            view.RefreshControl();
            view.RefreshCurrentEntity();
        }
    }

    [Command(ImageName = "MoveUp.bmp", Label = "上移", GroupType = CommandGroupType.Edit)]
    public class MoveUpCommand : MoveCommand
    {
        public MoveUpCommand()
        {
            this.MoveUp = true;
        }
    }

    [Command(ImageName = "MoveDown.bmp", Label = "下移", GroupType = CommandGroupType.Edit)]
    public class MoveDownCommand : MoveCommand
    {
        public MoveDownCommand()
        {
            this.MoveUp = false;
        }
    }

    #endregion

    #region 升、降级

    [Command(ImageName = "LevelUp.bmp", Label = "升级", GroupType = CommandGroupType.Edit)]
    public class LevelUpCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return base.CanExecute(view) &&
                (view.Current as Entity).TreeParent != null;
        }

        public override void Execute(ListObjectView view)
        {
            var list = view.Data as EntityList;
            if (list == null) throw new ArgumentNullException("list");

            var currentObject = view.Current as Entity;
            var oldParent = currentObject.TreeParent;

            throw new NotImplementedException();//huqf
            ////在结构中更改数据
            //list.ChangeNodeLevel(currentObject, true);

            //view.RefreshControl();

            //view.Current = currentObject;
        }
    }

    [Command(ImageName = "LevelDown.bmp", Label = "降级", ToolTip = "降级", GroupType = CommandGroupType.Edit)]
    public class LevelDownCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            if (base.CanExecute(view))
            {
                throw new NotImplementedException();//huqf
                //var list = view.Data as EntityList;
                //if (list == null || !list.SupportTree) { return false; }

                //var preNode = list.FindNodeAtSameLevel(view.Current as Entity, true);
                //return preNode != null;
            }

            return false;
        }

        public override void Execute(ListObjectView view)
        {
            var list = view.Data as EntityList;
            if (list == null) throw new ArgumentNullException("list");

            var currentNode = view.Current as Entity;

            throw new NotImplementedException();//huqf
            //list.ChangeNodeLevel(currentNode, false);

            //view.RefreshControl();

            //view.Current = currentNode;
        }
    }

    #endregion

    #region 前插、后插、添加子

    [Command(Label = "插入", GroupType = CommandGroupType.Edit)]
    public class InsertBeforeCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return base.CanExecute(view) && view.CanAddItem() && view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            var list = view.Data;
            if (list == null) throw new ArgumentNullException("list");

            var treeView = view.Control as MultiTypesTreeGrid;
            var current = view.Current;

            var newEntity = view.CreateNewItem();

            if (current.TreeParent != null)
            {
                var children = current.TreeParent.TreeChildren;
                var index = children.IndexOf(current);
                children.Insert(index, newEntity);
            }
            else
            {
                var index = list.IndexOf(current);
                list.Insert(index, newEntity);
            }

            view.RefreshControl();

            view.Current = newEntity;
        }
    }

    //不需要后插按钮，统一为插入执行前插
    //[Command(Label = "后插", GroupType = CommandGroupType.Edit)]
    //public class InsertFollowCommand : InsertCommand
    //{
    //    protected override bool InsertBefore
    //    {
    //        get { return false; }
    //    }
    //}

    [Command(ImageName = "AddChild.bmp", Label = "添加子", GroupType = CommandGroupType.Edit)]
    public class AddChildCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return base.CanExecute(view) && view.CanAddItem() && view.Current != null;
        }

        public override void Execute(ListObjectView view)
        {
            view.InsertNewChild(true);
        }
    }

    #endregion
}