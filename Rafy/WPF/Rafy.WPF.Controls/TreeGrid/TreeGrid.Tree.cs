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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 一个封装了系统 TreeView 控件，并提供更强大 API 的控件类型。
    /// </summary>
    partial class TreeGrid
    {
        #region 父子关系

        /// <summary>
        /// 获取某行数据对应的父对象的 Id。
        /// </summary>
        /// <param name="item"></param>
        /// <returns>如果返回 null，表示此行数据就是根结点，它没有父结点。</returns>
        protected abstract object GetPId(object item);

        /// <summary>
        /// 实现时应该返回指定实体对应的整形 Id
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public abstract object GetId(object item);

        /// <summary>
        /// 子类实体此方法，实现获取指定父节点的所有可用的子节点。
        /// 如果没有子节点，不能返回 null，请直接返回空集合。
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public abstract IEnumerable<object> GetChildItems(object parent);

        /// <summary>
        /// 获取指定实体对应的父实体，如果没有则返回 null。
        /// </summary>
        /// <param name="dataItem">The currently processed item.</param>
        /// <returns>The parent of the item, if available.</returns>
        public abstract object GetParentItem(object dataItem);

        /// <summary>
        /// 检测指定的结点是否有子结点。
        /// 本方法会被调用来检测是否某结点需要显示一个 Expander。
        /// 
        /// 默认实现是直接使用 <see cref="GetChildItems"/> 方法来检测子结点的个数是否大于 0。
        /// </summary>
        /// <remarks>
        /// You should override this method if invoking
        /// <see cref="GetChildItems"/> is an expensive operation
        /// (e.g. because data needs to be retrieved from a web
        /// service). In case there is no possibility for a cheaper solution,
        /// you may just return true: In that case, an expander will
        /// be rendered and removed as soon as the user attempts to
        /// expand the node, if there are no child items available.<br />
        /// However: Overriding this method is pointless if
        /// <see cref="ObserveChildItems"/> is set to true. In that
        /// case, this method will not be used as
        /// <see cref="GetChildItems"/> is being invoked anyway to get
        /// the observed collection.
        /// </remarks>
        protected virtual bool HasChildItems(object parent)
        {
            return GetChildItems(parent).Any();
        }

        /// <summary>
        /// 获取从根节点到指定节点的父节点的一个节点列表。
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        private List<object> GetParentItemList(object child)
        {
            var parents = new List<object>();
            object parentItem = GetParentItem(child);
            while (parentItem != null)
            {
                parents.Insert(0, parentItem);
                parentItem = GetParentItem(parentItem);
            }
            return parents;
        }

        /// <summary>
        /// 检测是否指定的两个实体是父子关系。
        /// </summary>
        /// <param name="child"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        private bool IsDescendantOf(object child, object parent)
        {
            object directParent = GetParentItem(child);

            if (directParent == null) return false;

            if (directParent == parent) return true;

            return this.IsDescendantOf(directParent, parent);
        }

        #endregion

        private object EachNode(Func<object, bool> filter)
        {
            var items = this.RootItems;
            if (items != null)
            {
                foreach (var item in items)
                {
                    var found = EachNode(item, filter);
                    if (found != null) return found;
                }
            }

            return null;
        }

        private object EachNode(object item, Func<object, bool> filter)
        {
            var found = filter(item);
            if (found) return item;

            var children = this.GetChildItems(item);
            foreach (var child in children)
            {
                var itemFound = EachNode(child, filter);
                if (itemFound != null) return itemFound;
            }

            return null;
        }
    }
}