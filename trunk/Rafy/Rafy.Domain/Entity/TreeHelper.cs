using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rafy.Domain.ORM;

namespace Rafy.Domain
{
    internal static class TreeHelper
    {
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

        internal static void MarkTreeFullLoaded(ITreeComponent component)
        {
            component.EachNode(e =>
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
        /// <param name="list"></param>
        /// <param name="nodes"></param>
        internal static void LoadTreeData(IList<Entity> list, IEnumerable nodes)
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
                    list.Add(entity);
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
                        list.Add(entity);
                    }
                    else
                    {
                        parentNode.TreeChildren.LoadAdd(entity);
                    }
                }

                lastNode = entity;
            }
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

    public static class TreeIndexHelper
    {
        /// <summary>
        /// 重新设置整个表的所有 TreeIndex。
        /// 注意，此方法只保证生成的 TreeIndex 有正确的父子关系，同时顺序有可能被打乱。
        /// </summary>
        /// <param name="repository"></param>
        public static void ResetTreeIndex(EntityRepository repository)
        {
            using (var tran =  RF.TransactionScope(repository))
            {
                //先清空
                var dp = repository.RdbDataProvider;
                var table = dp.DbTable;
                var column = table.Columns.First(c => (c as DbColumn).Property == Entity.TreeIndexProperty);
                using (var dba = dp.CreateDbAccesser())
                {
                    dba.ExecuteText(string.Format("UPDATE {0} SET {1} = NULL", table.Name, column.Name));
                }

                var all = repository.GetAll();
                if (all.Count > 0)
                {
                    //如果加载的过程中，第一个节点刚好是根节点，
                    //则加载完成后是一棵完整的树，Index 也生成完毕，不需要再次处理。
                    if (all.IsTreeRootList)
                    {
                        all.ResetTreeIndex();
                        repository.Save(all);
                    }
                    else
                    {
                        var cloneOptions = CloneOptions.ReadSingleEntity();
                        var oldList = new List<Entity>();
                        all.EachNode(e =>
                        {
                            var cloned = repository.New();
                            cloned.Clone(e, cloneOptions);
                            cloned.MarkUnchanged();

                            oldList.Add(cloned);
                            return false;
                        });

                        var newList = repository.NewList();
                        while (oldList.Count > 0)
                        {
                            foreach (var item in oldList)
                            {
                                var treePId = item.TreePId;
                                if (treePId == null)
                                {
                                    newList.Add(item);
                                    oldList.Remove(item);
                                    break;
                                }
                                else
                                {
                                    var parent = newList.EachNode(e => e.Id.Equals(treePId));
                                    if (parent != null)
                                    {
                                        parent.TreeChildren.LoadAdd(item);
                                        oldList.Remove(item);
                                        break;
                                    }
                                }
                            }
                        }
                        TreeHelper.MarkTreeFullLoaded(newList);
                        newList.ResetTreeIndex();
                        repository.Save(newList);
                    }
                }

                tran.Complete();
            }
        }
    }
}