using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace OEA.Library
{
    public static class TreeEntityHelper
    {
        /// <summary>
        /// 深度遍历
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="action"></param>
        public static void TravelDepthFirst(this Entity rootNode, Action<Entity> action)
        {
            if (rootNode == null) throw new ArgumentNullException("rootNode");
            if (action == null) throw new ArgumentNullException("action");

            Stack<Entity> stack = new Stack<Entity>();

            stack.Push(rootNode);
            while (stack.Count > 0)
            {
                var currentNode = stack.Pop();

                action(currentNode);

                var children = currentNode.TreeChildren;
                if (children != null)
                {
                    for (int i = children.Count - 1; i >= 0; i--)
                    {
                        var child = children[i];
                        if (child != null)
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 广度遍历
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="action"></param>
        public static void TravelScopeFirst(this Entity rootNode, Action<Entity> action)
        {
            if (rootNode == null) throw new ArgumentNullException("rootNode");
            if (action == null) throw new ArgumentNullException("action");

            Queue<Entity> queue = new Queue<Entity>();
            queue.Enqueue(rootNode);
            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                action(node);

                foreach (var childNode in node.TreeChildren)
                {
                    if (childNode != null)
                    {
                        queue.Enqueue(childNode);
                    }
                }
            }
        }

        /// <summary>
        /// 得到级联子对象
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IList<Entity> GetRecurChildren(this Entity node)
        {
            if (node == null) throw new ArgumentNullException("node");

            var result = new List<Entity>();
            TreeEntityHelper.TravelDepthFirst(node, n =>
            {
                result.Add(n);
            });
            return result;
        }

        /// <summary>
        /// 查找集合中所有的根节点
        /// </summary>
        /// <param name="nodesCollection"></param>
        /// <returns></returns>
        public static Entity[] FindRootNodes(IEnumerable nodesCollection)
        {
            return nodesCollection.Cast<Entity>().Where(n => n.TreePId == null).ToArray();
        }

        /// <summary>
        /// 统计一个这个树的所有节点个数
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static int CountTreeNode(this Entity node)
        {
            int treeNodeCount = 0;
            node.TravelScopeFirst(n =>
            {
                treeNodeCount++;
            });
            return treeNodeCount;
        }
    }
}