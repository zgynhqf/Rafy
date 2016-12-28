/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140528
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140528 20:36
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    public static class TreeHelper
    {
        /// <summary>
        /// 树形EntityList转化为全部节点集合
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static List<T> ConvertToList<T>(ITreeComponent tree) where T:Entity
        {
            List<T> list = new List<T>();
            tree.EachNode(e =>
            {
                list.Add((T)e);
                return false;
            });
            return list;
        }

        internal static int CountNodes(ITreeComponent component)
        {
            int count = 0;
            component.EachNode(e =>
            {
                count++;
                return false;
            });
            return count;
        }

        internal static void MarkTreeFullLoaded(IList<Entity> nodes)
        {
            for (int i = 0, c = nodes.Count; i < c; i++)
            {
                var item = nodes[i] as ITreeComponent;
                item.EachNode(e =>
                {
                    var tc = e.TreeChildrenField;
                    if (tc == null)
                    {
                        e.IsTreeLeafSure = true;
                    }
                    else
                    {
                        tc.MarkLoaded();
                    }
                    return false;
                });
            }
        }

        /// <summary>
        /// 先让树完整加载，然后再把树中的节点全部加入到指定的列表中。
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="nodes"></param>
        internal static void FullAddIntoList(ITreeComponent tree, IList<Entity> nodes)
        {
            tree.LoadAllNodes();
            tree.EachNode(e =>
            {
                nodes.Add(e);
                return false;
            });
        }

        /// <summary>
        /// 以树节点加载算法加载数据。
        /// </summary>
        /// <param name="list">The list.</param>
        /// <param name="nodes">The nodes.</param>
        /// <param name="indexOption">The index option.</param>
        internal static void LoadTreeData(IList<Entity> list, IEnumerable nodes, TreeIndexOption indexOption)
        {
            /*********************** 代码块解释 *********************************
             * 树节点加载算法：
             * 由于树节点在查询时已经通过 TreeIndex 来排序了，本质上是一种树的深度遍历序。
             * 所以我们只需要记录最后一次添加的节点，然后通过它尝试找到与要添加的节点的父节点，
             * 如果没有找到，则直接把要添加的节点添加到列表中。
             * 这样，就以一种顺序的方式完成了整个节点的加载。
            **********************************************************************/

            //最后一次添加的节点。
            Entity lastNode = null;
            foreach (Entity entity in nodes)
            {
                var treePId = entity.TreePId;
                if (treePId == null)
                {
                    var added = TryAddToList(list, entity, indexOption);
                    if (added) lastNode = entity;
                }
                else
                {
                    //尝试通过最后一次添加的节点来找到与 entity 关联的父节点。
                    var parentNode = lastNode;
                    while (parentNode != null && !treePId.Equals(parentNode.Id))
                    {
                        parentNode = parentNode.TreeParentData;
                    }

                    //如果没有找到 parentNode，则加入到列表中。否则，加入到 parentNode 下。
                    if (parentNode == null)
                    {
                        var added = TryAddToList(list, entity, indexOption);
                        if (added) lastNode = entity;
                    }
                    else
                    {
                        parentNode.TreeChildren.LoadAdd(entity);
                        lastNode = entity;
                    }
                }
            }
        }

        /// <summary>
        /// 必须要同一级的节点才能加入到实体列表中。
        /// Test：TET_Query_LoadSubTreeIgnoreOtherNodes
        /// </summary>
        /// <param name="list"></param>
        /// <param name="node"></param>
        /// <param name="indexOption"></param>
        private static bool TryAddToList(IList<Entity> list, Entity node, TreeIndexOption indexOption)
        {
            if (list.Count > 0)
            {
                var listLevel = indexOption.CountLevel(list[0].TreeIndex);
                var nodeLevel = indexOption.CountLevel(node.TreeIndex);
                if (listLevel == nodeLevel)
                {
                    list.Add(node);
                    return true;
                }
            }
            else
            {
                list.Add(node);
                return true;
            }

            return false;
        }

        ///// <summary>
        ///// 深度遍历
        ///// </summary>
        ///// <param name="rootNode"></param>
        ///// <param name="action"></param>
        //public static void TravelDepthFirst(this Entity rootNode, Action<Entity> action)
        //{
        //    if (rootNode == null) throw new ArgumentNullException("rootNode");
        //    if (action == null) throw new ArgumentNullException("action");

        //    Stack<Entity> stack = new Stack<Entity>();

        //    stack.Push(rootNode);
        //    while (stack.Count > 0)
        //    {
        //        var currentNode = stack.Pop();

        //        action(currentNode);

        //        var children = currentNode.TreeChildren;
        //        if (children != null)
        //        {
        //            for (int i = children.Count - 1; i >= 0; i--)
        //            {
        //                var child = children[i];
        //                if (child != null)
        //                {
        //                    stack.Push(child);
        //                }
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 广度遍历
        ///// </summary>
        ///// <param name="rootNode"></param>
        ///// <param name="action"></param>
        //public static void TravelScopeFirst(this Entity rootNode, Action<Entity> action)
        //{
        //    if (rootNode == null) throw new ArgumentNullException("rootNode");
        //    if (action == null) throw new ArgumentNullException("action");

        //    Queue<Entity> queue = new Queue<Entity>();
        //    queue.Enqueue(rootNode);
        //    while (queue.Count > 0)
        //    {
        //        var node = queue.Dequeue();
        //        action(node);

        //        foreach (var childNode in node.TreeChildren)
        //        {
        //            if (childNode != null)
        //            {
        //                queue.Enqueue(childNode);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// 得到级联子对象
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //public static IList<Entity> GetRecurChildren(this Entity node)
        //{
        //    if (node == null) throw new ArgumentNullException("node");

        //    var result = new List<Entity>();
        //    TreeEntityHelper.TravelDepthFirst(node, n =>
        //    {
        //        result.Add(n);
        //    });
        //    return result;
        //}

        ///// <summary>
        ///// 查找集合中所有的根节点
        ///// </summary>
        ///// <param name="nodesCollection"></param>
        ///// <returns></returns>
        //public static Entity[] FindRootNodes(IEnumerable nodesCollection)
        //{
        //    return nodesCollection.Cast<Entity>().Where(n => n.TreePId == null).ToArray();
        //}

        ///// <summary>
        ///// 统计一个这个树的所有节点个数
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //public static int CountTreeNode(this Entity node)
        //{
        //    int treeNodeCount = 0;
        //    node.TravelScopeFirst(n =>
        //    {
        //        treeNodeCount++;
        //    });
        //    return treeNodeCount;
        //}
    }
}