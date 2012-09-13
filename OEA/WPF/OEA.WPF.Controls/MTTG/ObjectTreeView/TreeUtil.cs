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
using System.Windows.Controls;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// Provides static helper methods.
    /// </summary>
    internal static class TreeUtil
    {
        /// <summary>
        /// Checks whether a given tree node contains a dummy node to
        /// ensure it's rendered with an expander, and removes the node.
        /// </summary>
        /// <param name="treeNode">The node to be checked for dummy
        /// child nodes.</param>
        internal static void ClearDummyChildNode(TreeViewItem treeNode)
        {
            //if the item has never been expanded yet, it contains a dummy
            //node - replace that one and insert real data
            if (ContainsDummyNode(treeNode))
            {
                treeNode.Items.Clear();
            }
        }

        /// <summary>
        /// Validates whether a given node contains a single dummy item,
        /// which was added to ensure the submitted tree node renders
        /// an expander.
        /// </summary>
        /// <param name="treeNode">The tree node to be validated.</param>
        /// <returns>True if the node contains a dummy item.</returns>
        internal static bool ContainsDummyNode(TreeViewItem treeNode)
        {
            return treeNode.Items.Count == 1 && ((TreeViewItem)treeNode.Items[0]).Header == null;
        }

        /// <summary>
        /// 迭归遍历所有子结点。
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        internal static IEnumerable<TreeViewItem> TraverseNodes(ItemCollection nodes)
        {
            //process child groups
            foreach (TreeViewItem node in nodes)
            {
                if (IsDummyItem(node))
                {
                    yield break;
                }
                else
                {
                    yield return node;

                    foreach (TreeViewItem item in TraverseNodes(node.Items))
                    {
                        yield return item;
                    }
                }
            }
        }

        internal static bool IsDummyItem(TreeViewItem item)
        {
            return item.Header == null;
        }
    }
}