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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Threading;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// Observes nested items of the tree's bound items, and updates the tree
    /// accordingly, if items are being added or removed.
    /// </summary>
    /// <typeparam name="object"></typeparam>
    public class ItemMonitor
    {
        #region fields

        /// <summary>
        /// Stores the currently observed child collections of
        /// monitored parent items, stored by their parent's
        /// key.
        /// </summary>
        private readonly Dictionary<int, INotifyCollectionChanged> _childCollections = new Dictionary<int, INotifyCollectionChanged>();

        /// <summary>
        /// The tree that renders the observed items.
        /// </summary>
        private readonly CompositeTreeView tree;

        #endregion

        #region properties

        /// <summary>
        /// Gets the currently observed child collections of
        /// monitored parent items, stored by their parent's
        /// key.
        /// </summary>
        public Dictionary<int, INotifyCollectionChanged> ChildCollections
        {
            get { return _childCollections; }
        }


        /// <summary>
        /// Gets the tree that renders the observed items.
        /// </summary>
        public CompositeTreeView Tree
        {
            get { return tree; }
        }

        #endregion

        #region collection change event bubbling

        /// <summary>
        /// Bubbles an <see cref="INotifyCollectionChanged.CollectionChanged"/>
        /// event of one of the observed collections. The sender is the
        /// changed collection.
        /// </summary>
        public event NotifyCollectionChangedEventHandler MonitoredCollectionChanged;

        #endregion

        #region construction

        /// <summary>
        /// Creates the monitor with the tree to be processed.
        /// </summary>
        /// <param name="tree">The tree that renders the monitored
        /// items.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="tree"/>
        /// is a null reference.</exception>
        public ItemMonitor(CompositeTreeView tree)
        {
            if (tree == null) throw new ArgumentNullException("tree");
            this.tree = tree;
        }

        #endregion

        #region register root items

        /// <summary>
        /// Removes a change listener for the root collection that has been bound
        /// to the tree's <see cref="TreeViewBase{object}.Items"/> dependency property.<br/>
        /// <seealso cref="RegisterRootItems"/>
        /// </summary>
        /// <param name="items">A collection of items that have been bound to the
        /// tree's <see cref="TreeViewBase{object}.Items"/> dependency property.
        /// </param>
        public void UnregisterRootItems(IEnumerable items)
        {
            lock (this)
            {
                INotifyCollectionChanged observable = items as INotifyCollectionChanged;
                if (observable != null)
                {
                    observable.CollectionChanged -= OnRootItemCollectionChanged;
                }
            }
        }


        /// <summary>
        /// Registers a change listener for the root collection that has been bound
        /// to the tree's <see cref="TreeViewBase{object}.Items"/> dependency property.<br/>
        /// Picking up changes only works if <paramref name="items"/> implements
        /// <see cref="INotifyCollectionChanged"/>.
        /// <seealso cref="UnregisterRootItems"/>
        /// </summary>
        /// <param name="items">A collection of items that have been bound to the
        /// tree's <see cref="TreeViewBase{object}.Items"/> dependency property.
        /// </param>
        public void RegisterRootItems(IEnumerable items)
        {
            lock (this)
            {
                INotifyCollectionChanged observable = items as INotifyCollectionChanged;
                if (observable != null)
                {
                    observable.CollectionChanged += OnRootItemCollectionChanged;
                }
            }
        }


        /// <summary>
        /// This method is being invoked if the root collection that was bound to the
        /// monitored tree's <see cref="TreeViewBase{object}.Items"/> property was changed. Just like
        /// changes of a nested tree item, this method bubbles the event by invoking the
        /// <see cref="MonitoredCollectionChanged"/> event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRootItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!tree.Dispatcher.CheckAccess())
            {
                //make sure we're on the right thread
                NotifyCollectionChangedEventHandler handler = OnRootItemCollectionChanged;
                tree.Dispatcher.BeginInvoke(DispatcherPriority.Normal, handler, null, new object[] { sender, e });
                return;
            }

            lock (this)
            {
                if (tree.ObserveRootItems) tree.Refresh();
            }

            //bubble event
            if (MonitoredCollectionChanged != null)
            {
                MonitoredCollectionChanged(sender, e);
            }
        }

        #endregion

        #region register item

        /// <summary>
        /// Creates a change listener for a given item's
        /// child nodes and caches the collection.<br/>
        /// This requires the submitted <paramref name="childItems"/>
        /// collection to be of type <see cref="INotifyCollectionChanged"/>.
        /// If the collection does not implement this interface, a debug
        /// warning is issued without an exception.
        /// </summary>
        /// <param name="itemKey">The unique key of the parent
        /// item, as returned by <see cref="TreeViewBase{object}.GetItemKey"/>.
        /// </param>
        /// <param name="childItems">The item's childs as returned by
        /// <see cref="TreeViewBase{object}.GetChildItems"/>.</param>
        public void RegisterItem(int itemKey, ICollection<object> childItems)
        {
            lock (this)
            {
                INotifyCollectionChanged observable = childItems as INotifyCollectionChanged;
                if (observable != null)
                {
                    _childCollections.Add(itemKey, observable);
                    observable.CollectionChanged += OnItemCollectionChanged;
                }
                else
                {
                    //the collection cannot be monitored - issue a warning
                    string msg =
                      "Cannot observe childs of {0} instance '{1}': The child collection does not implement the required {2} interface!";
                    msg = String.Format(msg, typeof(object).Name, itemKey, typeof(INotifyCollectionChanged).FullName);
                    Debug.WriteLine(msg);
                }
            }
        }

        #endregion

        #region remove nodes

        /// <summary>
        /// Deregisters event listeners for all nodes.
        /// </summary>
        /// <param name="treeNodes">A collection of tree
        /// nodes to be removed.</param>
        public void RemoveNodes(ItemCollection treeNodes)
        {
            lock (this)
            {
                foreach (TreeViewItem treeNode in treeNodes)
                {
                    UnregisterListeners(treeNode);
                }
            }
        }


        /// <summary>
        /// Removes listener for an item that is represented
        /// by  a given <paramref name="node"/> from the cache, and also
        /// processes all the node's descendants recursively.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        public void UnregisterListeners(TreeViewItem node)
        {
            lock (this)
            {
                //try to get the represented item
                object item = node.Header as object;
                if (item == null) return;

                //get the item's observed child collection and
                //deregister it
                var itemKey = tree.GetId(item);
                RemoveCollectionFromCache(itemKey);

                foreach (TreeViewItem childNode in node.Items)
                {
                    //recursively deregister descendants by
                    //removing all child nodes
                    UnregisterListeners(childNode);
                }
            }
        }

        #endregion

        #region clear cache

        /// <summary>
        /// Deregisters the all listeners and clears the cache. Do note however, that
        /// deregistering the bound root items requires the <see cref="UnregisterRootItems"/>
        /// method to be invoked.
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                foreach (var pair in _childCollections)
                {
                    pair.Value.CollectionChanged -= OnItemCollectionChanged;
                }

                _childCollections.Clear();
            }
        }

        #endregion

        #region handle collection change

        /// <summary>
        /// Invokes if one of the observed collections is being changed. Triggers
        /// updates of the tree control.
        /// </summary>
        private void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!tree.Dispatcher.CheckAccess())
            {
                //make sure we're on the right thread
                NotifyCollectionChangedEventHandler handler = OnItemCollectionChanged;
                tree.Dispatcher.BeginInvoke(DispatcherPriority.Normal, handler, null, new object[] { sender, e });
                return;
            }

            lock (this)
            {
                //get the collection
                ICollection collection = (ICollection)sender;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        HandleNewChildItems(collection, e);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        HandleRemovedChildItems(collection, e);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        var parentNode = GetParentNode((object)e.OldItems[0], collection);
                        if (parentNode == null) return;

                        //check if the node is expanded - if the "remove" part
                        //clears all items, the node will be collapsed and not
                        //automatically re-expanded in the "add" part
                        bool expanded = parentNode.IsExpanded;
                        //remove old items, than add new ones
                        HandleRemovedChildItems(collection, e);
                        HandleNewChildItems(collection, e);
                        //re-expand if necessary
                        if (expanded) parentNode.IsExpanded = true;
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        //we don't have any items ready - search the cache
                        var parentKey = TryFindParentKey(sender);
                        if (parentKey == 0)
                        {
                            //if we cannot find an entry in the collection, quit
                            //here
                            string msg = "An observed child item collection issued a Reset event. ";
                            msg += "However, the cache does not contain the collection that raised the ";
                            msg += "event anymore";
                            Debug.Fail(msg);
                            return;
                        }

                        //clear all items
                        ClearChilds(parentKey);
                        break;
                }


                //bubble event
                if (MonitoredCollectionChanged != null)
                {
                    MonitoredCollectionChanged(sender, e);
                }
            }
        }

        #endregion

        #region clear childs of item

        /// <summary>
        /// Removes all childs of a given parent node.
        /// </summary>
        /// <param name="parentItemKey"></param>
        private void ClearChilds(int parentItemKey)
        {
            //deregister and re-register the parent
            var parentNode = tree.TryFindNodeByKey(parentItemKey);

            if (parentNode == null)
            {
                //the parent node is longer available which should not be the case
                //-> clear the collection and quit
                RemoveCollectionFromCache(parentItemKey);

                string msg = "Could not clear childs of item '{0}': The tree does not ";
                msg += "contain a matching tree node for the item itself anymore.";
                msg = String.Format(msg, parentItemKey);
                Debug.Fail(msg);

                return;
            }

            foreach (GridTreeViewRow childNode in parentNode.Items)
            {
                //unregister items
                UnregisterListeners(childNode);
            }

            //clear nodes
            parentNode.Items.Clear();
            parentNode.IsExpanded = false;
        }

        #endregion

        #region handle removed items.

        /// <summary>
        /// Updates the tree if items were removed from an observed
        /// child collection. This might cause rendered tree nodes
        /// to be removed. In case lazy loading is enabled, the update
        /// of the UI may be as subtle as to remove an expander from
        /// a collapsed node if the represented item's childs were
        /// removed (or the childs that pass an active filter).<br/>
        /// </summary>
        /// <param name="observed">The observed collection.</param>
        /// <param name="e">The event arguments that provide the
        /// removed items.</param>
        private void HandleRemovedChildItems(ICollection observed, NotifyCollectionChangedEventArgs e)
        {
            IList childs = e.OldItems;
            if (childs.Count == 0) return;

            //get the node of the parent item that contains the evented childs
            var parentNode = GetParentNode((object)childs[0], observed);
            if (parentNode == null) return;

            foreach (object childItem in childs)
            {
                var itemKey = tree.GetId(childItem);

                //check if we have a corresponding open node
                //-> not necessarily the case if we're doing lazy loading
                var childNode = tree.TryFindItemNode(parentNode.Items, itemKey, false);
                if (childNode != null)
                {
                    //unregister listeners
                    UnregisterListeners(childNode);
                    //remove node from UI
                    parentNode.Items.Remove(childNode);
                }
            }

            //in case of lazy loading, the tree might contain a dummy node
            //(has not been expanded). However, it might be that it's now
            //completely empty...
            if (observed.Count == 0)
            {
                TreeUtil.ClearDummyChildNode(parentNode);
                parentNode.IsExpanded = false;
            }
        }

        #endregion

        #region handle added items

        /// <summary>
        /// Updates the tree with newly added items.
        /// </summary>
        /// <param name="observed">The observed collection.</param>
        /// <param name="e">Collection event args.</param>
        public void HandleNewChildItems(ICollection observed, NotifyCollectionChangedEventArgs e)
        {
            IList childs = e.NewItems;
            if (childs.Count == 0) return;

            //get the node of the parent item that contains the evented childs
            var parentNode = GetParentNode((object)childs[0], observed);
            if (parentNode == null) return;

            //if the node is expanded or does not load lazily, or
            //already contains valid items, create nodes
            if (parentNode.IsExpanded || !tree.IsLazyLoading || !TreeUtil.ContainsDummyNode(parentNode))
            {
                foreach (object child in childs)
                {
                    tree.CreateItemNode(child, parentNode.Items, null);
                }

                //refresh the node in order to apply sorting (or any other
                //features that will be incorporated)
                parentNode.Items.Refresh();
            }
            else if (parentNode.Items.Count == 0)
            {
                //if the tree is in lazy loading mode and the item did
                //not contain any childs before, we have to add a dummy
                //node to render a expander
                parentNode.Items.Add(this.tree.CreateDummyItem());
            }
        }

        #endregion

        #region get parent node for child collection

        /// <summary>
        /// Gets the tree node that represents the parent
        /// of a given item.
        /// </summary>
        /// <param name="childItem">A currently processed item that
        /// contains a parent.</param>
        /// <param name="collection">The collection that contains
        /// <paramref name="childItem"/>.</param>
        /// <returns>The parent tree node (UI control) that reprents the
        /// logical parent of <paramref name="childItem"/>.</returns>
        private TreeViewItem GetParentNode(object childItem, ICollection collection)
        {
            object parent = tree.GetParentItem(childItem);
            var parentNode = parent == null ? null : tree.TryFindNode(parent);

            //if there is no parent according to the tree implementation,
            //the implementation is flawed
            if (parentNode == null)
            {
                INotifyCollectionChanged col = (INotifyCollectionChanged)collection;
                RemoveCollectionFromCache(col);

                var itemKey = tree.GetId(childItem);
                string msg =
                  "The tree does not contain the parent tree node for a monitored child item collection that contains item '{0}'. ";
                msg += "This can only happen if the node was removed without proper deregistration. ";
                msg += "The collection will be removed from the monitor.";
                msg = string.Format(msg, itemKey);
                Debug.Fail(msg);
            }

            return parentNode;
        }

        #endregion

        #region remove collection from cache

        /// <summary>
        /// Removes a given collection from the internal cache
        /// and deregisters its event listener.
        /// </summary>
        /// <param name="itemKey">The item key under which the
        /// collection is stored in the cache.</param>
        private void RemoveCollectionFromCache(int itemKey)
        {
            INotifyCollectionChanged childs;
            if (_childCollections.TryGetValue(itemKey, out childs))
            {
                childs.CollectionChanged -= OnItemCollectionChanged;
                _childCollections.Remove(itemKey);
            }
        }


        /// <summary>
        /// Removes a given collection from the cache. This method is
        /// only called if something went wrong and a collection's item
        /// cannot be found anymore.
        /// </summary>
        /// <param name="col">The collection to be removed.</param>
        private void RemoveCollectionFromCache(INotifyCollectionChanged col)
        {
            int itemKey = 0;
            foreach (var pair in _childCollections)
            {
                if (ReferenceEquals(pair.Value, col))
                {
                    col.CollectionChanged -= OnItemCollectionChanged;
                    itemKey = pair.Key;
                    break;
                }
            }
            _childCollections.Remove(itemKey);
        }

        #endregion

        #region find dictionary entry by value

        /// <summary>
        /// Searches the cache for a given collection.
        /// </summary>
        /// <returns>The key of the parent item that contains the
        /// submitted child collection. If no matching entry was found
        /// in the cache, a null reference is being returned.</returns>
        private int TryFindParentKey(object collection)
        {
            lock (this)
            {
                foreach (var pair in _childCollections)
                {
                    if (ReferenceEquals(collection, pair.Value)) return pair.Key;
                }

                return 0;
            }
        }

        #endregion
    }
}