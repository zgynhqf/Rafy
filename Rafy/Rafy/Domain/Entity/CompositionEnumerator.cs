/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210912
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210912 23:40
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 使用尝试递归遍历实体的组合树。
    /// 包含：实体、树子节点、已经加载的组合子实体列表。
    /// </summary>
    public struct CompositionEnumerator : IEnumerator<Entity>
    {
        private Stack<Entity> _stack;
        private Entity _current;

        /// <summary>
        /// 是否同时递归遍历组合子集合
        /// </summary>
        public bool IncludesChildren { get; set; }

        /// <summary>
        /// 是否同时递归遍历树子集合
        /// </summary>
        public bool IncludesTreeChildren { get; set; }

        /// <summary>
        /// 是否同时递归遍历组合中所有被删除的元素。
        /// </summary>
        public bool IncludeDeletedItems { get; set; }

        internal CompositionEnumerator(Stack<Entity> stock)
        {
            _stack = stock ?? new Stack<Entity>(10);
            _current = null;
            this.IncludesChildren = true;
            this.IncludesTreeChildren = true;
            this.IncludeDeletedItems = false;
        }

        public Entity Current => _current;

        public bool MoveNext()
        {
            if (_stack.Count > 0)
            {
                _current = _stack.Pop();

                //由于 Stack 先进后出。这里，先把 TreeChildren 加入，表示先递归遍历 Children，再遍历 TreeChildren。
                if (this.IncludesTreeChildren)
                {
                    this.Push(_current.TreeChildrenField);
                }

                if (this.IncludesChildren)
                {
                    foreach (var item in _current.GetLoadedChildren())
                    {
                        var list = item.Value as EntityList;
                        if (list != null)
                        {
                            this.Push(list);
                        }
                        else
                        {
                            this.Push(item.Value as Entity);
                        }
                    }
                }

                return true;
            }

            return false;
        }

        internal void Push(Entity item)
        {
            _stack.Push(item);
        }

        internal void Push(Entity.EntityTreeChildren treeChildren)
        {
            if (treeChildren != null)
            {
                this.PushList(treeChildren.NodesField);
                if (this.IncludeDeletedItems)
                {
                    this.PushList(treeChildren.DeletedListField);
                }
            }
        }

        internal void Push(EntityList list)
        {
            if (list != null)
            {
                this.PushList(list);
                if (this.IncludeDeletedItems)
                {
                    this.PushList(list.DeletedListField);
                }
            }
        }

        private void PushList(IList<Entity> nodes)
        {
            if (nodes != null)
            {
                //倒序加入。
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    _stack.Push(nodes[i]);
                }
            }
        }

        public CompositionEnumerator GetEnumerator()
        {
            //添加此方法，使得可以使用 foreach 循环
            return this;
        }

        object IEnumerator.Current => _current;

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
        }
    }
}