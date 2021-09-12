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
    /// 高效递归遍历整个实体组合树节点。
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

        public Entity Current => _current;

        public bool MoveNext()
        {
            if (_stack != null && _stack.Count > 0)
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

        /// <summary>
        /// 将本集合中的元素全部遍历，并返回 List。
        /// 此操作不会变更本迭代器中的状态。
        /// </summary>
        /// <returns></returns>
        public IList<Entity> ToList()
        {
            var res = new List<Entity>();

            if (_stack != null)
            {
                var cloned = this.Clone();
                foreach (var item in cloned)
                {
                    res.Add(item);
                }
            }

            return res;
        }

        /// <summary>
        /// 完全复制一个迭代器。
        /// </summary>
        /// <returns></returns>
        public CompositionEnumerator Clone()
        {
            var cloned = this;
            cloned._stack = new Stack<Entity>(_stack.ToArray());
            return cloned;
        }

        public void Push(Entity item)
        {
            this.InitStack();
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

        /// <summary>
        /// 将指定的元素，加入的遍历列表中。
        /// </summary>
        /// <param name="nodes"></param>
        public void Push(IList<Entity> nodes)
        {
            if (nodes != null)
            {
                this.PushList(nodes);

                if (this.IncludeDeletedItems)
                {
                    var el = nodes as EntityList;
                    if (el != null)
                    {
                        this.PushList(el.DeletedListField);
                    }
                }
            }
        }

        private void PushList(IList<Entity> nodes)
        {
            if (nodes != null)
            {
                this.InitStack();

                //倒序加入。
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    _stack.Push(nodes[i]);
                }
            }
        }

        private void InitStack()
        {
            if (_stack == null) _stack = new Stack<Entity>(10);
        }

        public CompositionEnumerator GetEnumerator()
        {
            //添加此方法，使得可以使用 foreach 循环
            return this;
        }

        #region 使用工厂，而非直接使用构造器

        public static CompositionEnumerator Create(Entity entity, bool includesChildren = true, bool includesTreeChildren = true, bool includeDeletedItems = false)
        {
            var res = Create(includesChildren, includesTreeChildren, includeDeletedItems);
            res.Push(entity);
            return res;
        }

        public static CompositionEnumerator Create(IList<Entity> entityList, bool includesChildren = true, bool includesTreeChildren = true, bool includeDeletedItems = false)
        {
            var res = Create(includesChildren, includesTreeChildren, includeDeletedItems);
            res.Push(entityList);
            return res;
        }

        public static CompositionEnumerator Create(bool includesChildren = true, bool includesTreeChildren = true, bool includeDeletedItems = false)
        {
            var res = new CompositionEnumerator();
            res._stack = null;
            res._current = null;
            res.IncludesChildren = includesChildren;
            res.IncludesTreeChildren = includesTreeChildren;
            res.IncludeDeletedItems = includeDeletedItems;
            return res;
        }

        #endregion

        #region IEnumerator.Elements

        object IEnumerator.Current => _current;

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        void IDisposable.Dispose()
        {
        }

        #endregion
    }
}