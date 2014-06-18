/*******************************************************
 * 
 * 作者：hardcodet
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 hardcodet 2008
 * 2.0 胡庆访 20120911 14:42
 * 
*******************************************************/

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// Provides static helper methods.
    /// </summary>
    public static class TreeGridHelper
    {
        #region DummyItem

        //DummyItem 意思是展开树节点时，显示子节点正在加载中的一个临时树节点。

        /// <summary>
        /// Checks whether a given tree node contains a dummy node to
        /// ensure it's rendered with an expander, and removes the node.
        /// </summary>
        /// <param name="treeNode">The node to be checked for dummy
        /// child nodes.</param>
        internal static void ClearDummyChildNode(TreeGridRow treeNode)
        {
            ////if the item has never been expanded yet, it contains a dummy
            ////node - replace that one and insert real data
            //if (ContainsDummyNode(treeNode))
            //{
            //    treeNode.Items.Clear();
            //}
        }

        internal static void CreateDummyItem(TreeGridRow treeNode)
        {
            ////clear items and insert dummy
            //treeNode.Items.Clear();
            //treeNode.Items.Add(this.CreateDummyItem());
        }

        /// <summary>
        /// Validates whether a given node contains a single dummy item,
        /// which was added to ensure the submitted tree node renders
        /// an expander.
        /// </summary>
        /// <param name="treeNode">The tree node to be validated.</param>
        /// <returns>True if the node contains a dummy item.</returns>
        internal static bool ContainsDummyNode(TreeGridRow treeNode)
        {
            return false;
            //return treeNode.Items.Count == 1 && TreeGrid.GetEntity(((TreeGridRow)treeNode.Items[0])) == null;
        }

        internal static bool IsDummyItem(TreeGridRow item)
        {
            return false;
            //return TreeGrid.GetEntity(item) == null;
        }

        internal static TreeGridRow CreateDummyItem()
        {
            return new TreeGridRow();
        }

        #endregion

        /// <summary>
        /// 迭归遍历所有子结点。
        /// </summary>
        /// <param name="itemsControl"></param>
        /// <returns></returns>
        internal static IEnumerable<TreeGridRow> TraverseRows(ItemsControl itemsControl, bool recur = false)
        {
            foreach (var dataItem in itemsControl.Items)
            {
                var row = itemsControl.ItemContainerGenerator.ContainerFromItem(dataItem) as TreeGridRow;

                //如果这时行还没有生成，则 row == null。
                if (row != null)
                {
                    if (IsDummyItem(row))
                    {
                        yield break;
                    }
                    else
                    {
                        yield return row;

                        if (recur)
                        {
                            foreach (TreeGridRow item in TraverseRows(row, true))
                            {
                                yield return item;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保证 desiredList 与 columns 长度一致。
        /// </summary>
        internal static void EnsureDesiredWidthList(ref List<double> desiredList, TreeGridColumnCollection columns)
        {
            if (columns != null)
            {
                int count = columns.Count;

                if (desiredList == null)
                {
                    desiredList = new List<double>(count);
                }

                //对于还没有在 _desiredWidthList 添加对应值的列，都添加相应的数据，
                int num = count - desiredList.Count;
                for (int i = 0; i < num; i++)
                {
                    desiredList.Add(double.NaN);
                }
            }
        }

        /// <summary>
        /// 保证 desiredList 与 columns 长度一致。
        /// </summary>
        internal static void UpdateDesiredWidthListOnColumnChanged(ref List<double> desiredList, TreeGridColumnCollectionChangedEventArgs e)
        {
            if (desiredList != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (desiredList.Count > e.StableIndex)
                    {
                        desiredList.RemoveAt(e.StableIndex);
                        return;
                    }
                }
                else
                {
                    if (e.Action == NotifyCollectionChangedAction.Reset)
                    {
                        desiredList = null;
                    }
                }
            }
        }

        internal static bool IsCtrlPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        }

        internal static bool IsShiftPressed()
        {
            return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        }

        /// <summary>
        /// 提交树型表格内部某元素引发的修改。
        /// </summary>
        public static void ExecuteCommitEdit(IInputElement target)
        {
            if (TreeGrid.CommitEditCommand.CanExecute(null, target))
            {
                TreeGrid.CommitEditCommand.Execute(null, target);
            }
        }

        #region MS Internal Methods

        internal static bool IsTemplateSelectorDefined(DependencyProperty templateSelectorProperty, DependencyObject d)
        {
            object obj = d.ReadLocalValue(templateSelectorProperty);
            return obj != DependencyProperty.UnsetValue && obj != null && (obj is DataTemplateSelector);// || obj is ResourceReferenceExpression);
        }

        internal static bool IsTemplateDefined(DependencyProperty templateProperty, DependencyObject d)
        {
            object obj = d.ReadLocalValue(templateProperty);
            return obj != DependencyProperty.UnsetValue && obj != null && (obj is FrameworkTemplate);// || obj is ResourceReferenceExpression);
        }

        internal static void CheckTemplateAndTemplateSelector(string name, DependencyProperty templateProperty, DependencyProperty templateSelectorProperty, DependencyObject d)
        {
            //if (TraceData.IsEnabled && Helper.IsTemplateSelectorDefined(templateSelectorProperty, d) && Helper.IsTemplateDefined(templateProperty, d))
            //{
            //    Trace
            //    TraceData.Trace(TraceEventType.Error, TraceData.TemplateAndTemplateSelectorDefined(new object[]
            //    {
            //        name
            //    }), d);
            //}
        }

        #endregion
    }
}