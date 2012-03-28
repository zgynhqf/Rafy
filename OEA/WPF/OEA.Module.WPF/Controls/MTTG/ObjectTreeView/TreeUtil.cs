// hardcodet.net WPF TreeView control
// Copyright (c) 2008 Philipp Sumi, Evolve Software Technologies
// Contact and Information: http://www.hardcodet.net
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the Code Project Open License (CPOL);
// either version 1.0 of the License, or (at your option) any later
// version.
// 
// This software is provided "AS IS" with no warranties of any kind.
// The entire risk arising out of the use or performance of the software
// and source code is with you.
//
// THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.


using System.Collections.Generic;
using System.Windows.Controls;

using OEA.Module.WPF.Controls;

namespace Hardcodet.Wpf.GenericTreeView
{
    /// <summary>
    /// Provides static helper methods.
    /// </summary>
    public static class TreeUtil
    {
        /// <summary>
        /// Checks whether a given tree node contains a dummy node to
        /// ensure it's rendered with an expander, and removes the node.
        /// </summary>
        /// <param name="treeNode">The node to be checked for dummy
        /// child nodes.</param>
        public static void ClearDummyChildNode(TreeViewItem treeNode)
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
        public static bool ContainsDummyNode(TreeViewItem treeNode)
        {
            return treeNode.Items.Count == 1 && ((TreeViewItem)treeNode.Items[0]).Header == null;
        }

        /// <summary>
        /// Recursively browses all descendants of a given item, starting at
        /// the item's child collection
        /// </summary>
        /// <param name="nodes">A collection of <see cref="TreeViewItem"/>
        /// instances to be processed recursively.</param>
        /// <returns>An enumerator for the tree's items, starting with the
        /// submitted <paramref name="nodes"/> collection.</returns>
        public static IEnumerable<TreeViewItem> BrowseNodes(ItemCollection nodes)
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

                    foreach (TreeViewItem item in BrowseNodes(node.Items))
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