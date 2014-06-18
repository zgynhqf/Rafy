/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Command
{
    public abstract class EditOrderedTreeListViewCommand : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return view.Data != null;
        }

        internal static IList<Entity> GetParentList(ITreeComponent node)
        {
            IList<Entity> parentList = null;
            var parent = node.TreeComponentParent;
            if (parent.ComponentType == TreeComponentType.TreeChildren)
            {
                parentList = parent as Entity.EntityTreeChildren;
            }
            else if (parent.ComponentType == TreeComponentType.NodeList)
            {
                parentList = parent as EntityList;
            }
            return parentList;
        }
    }

    #region 展开、折叠

    public abstract class ExpandBaseCommand : ListViewCommand
    {
        protected int Depth;

        public override bool CanExecute(ListLogicalView view)
        {
            return view.Data != null && view.Data.Count > 0;
        }

        public override void Execute(ListLogicalView view)
        {
            var tv = view.Control;
            foreach (var root in tv.RootItemsControl.ItemsSource)
            {
                tv.ExpandToDepth(root, this.Depth);
            }
        }
    }

    [Command(Hierarchy = "展开到", Label = "全部展开", GroupType = CommandGroupType.View)]
    public class ExpandAllCommand : ExpandBaseCommand
    {
        public override void Execute(ListLogicalView view)
        {
            TreeGrid tv = view.Control;
            if (tv != null) { tv.ExpandAll(); }
        }
    }

    [Command(Hierarchy = "展开到", Label = "一级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelOneCommand : ExpandBaseCommand
    {
        public ExpandToLevelOneCommand()
        {
            this.Depth = 1;
        }
    }

    [Command(Hierarchy = "展开到", Label = "二级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelTwoCommand : ExpandBaseCommand
    {
        public ExpandToLevelTwoCommand()
        {
            this.Depth = 2;
        }
    }

    [Command(Hierarchy = "展开到", Label = "三级节点", GroupType = CommandGroupType.View)]
    public class ExpandToLevelThreeCommand : ExpandBaseCommand
    {
        public ExpandToLevelThreeCommand()
        {
            this.Depth = 3;
        }
    }

    [Command(Hierarchy = "展开到", Label = "四级节点", GroupType = CommandGroupType.View)]
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

        public override bool CanExecute(ListLogicalView view)
        {
            if (base.CanExecute(view))
            {
                var node = view.Current;

                //找到这个实体所在层级的列表
                IList<Entity> context = null;
                if (node.TreeParent == null)
                {
                    context = view.Data;
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

        public override void Execute(ListLogicalView view)
        {
            var current = view.Current;
            var parentList = GetParentList(current);

            var index = parentList.IndexOf(current);
            parentList.Remove(current);

            if (this.MoveUp)
            {
                parentList.Insert(index - 1, current);
            }
            else
            {
                parentList.Insert(index + 1, current);
            }

            view.RefreshControl();
            view.Current = current;
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
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) &&
                view.Current != null
                && view.Current.IsTreeParentLoaded
                && view.Current.TreeParent != null;
        }

        public override void Execute(ListLogicalView view)
        {
            var current = view.Current;
            var treeParent = current.TreeParent;

            var grandParentList = GetParentList(treeParent);
            if (grandParentList == view.Data)
            {
                if (!view.Data.IsTreeRootList)
                {
                    //不处理这种情况。
                    return;
                }
            }

            treeParent.TreeChildren.Remove(current);
            grandParentList.Add(current);

            view.RefreshControl();
            view.Current = current;
        }
    }

    [Command(ImageName = "LevelDown.bmp", Label = "降级", ToolTip = "降级", GroupType = CommandGroupType.Edit)]
    public class LevelDownCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            if (base.CanExecute(view))
            {
                var previous = FindPreviousNode(view.Current);
                return previous != null;
            }

            return false;
        }

        public override void Execute(ListLogicalView view)
        {
            var current = view.Current;
            var previous = FindPreviousNode(current);
            current.TreeParent = previous;

            view.RefreshControl();
            view.Current = current;
        }

        private static Entity FindPreviousNode(Entity node)
        {
            IList<Entity> parentList = GetParentList(node);

            var i = parentList.IndexOf(node);
            if (i > 0) { return parentList[i - 1]; }

            return null;
        }
    }

    #endregion

    #region 前插、后插、添加子

    [Command(Label = "插入", GroupType = CommandGroupType.Edit)]
    public class InsertBeforeCommand : EditOrderedTreeListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && view.CanAddItem() && view.Current != null;
        }

        public override void Execute(ListLogicalView view)
        {
            var list = view.Data;
            if (list == null) throw new ArgumentNullException("list");

            var current = view.Current;

            var newEntity = view.CreateNewItem();

            var parentList = GetParentList(current);
            var index = parentList.IndexOf(current);
            parentList.Insert(index, newEntity);

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
        public override bool CanExecute(ListLogicalView view)
        {
            return base.CanExecute(view) && view.CanAddItem() && view.Current != null;
        }

        public override void Execute(ListLogicalView view)
        {
            view.InsertNewChildNode(true);
        }
    }

    #endregion
}