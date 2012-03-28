/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：树型列表的基类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using SimpleCsla;

namespace OEA.Library
{
    /// <summary>
    /// 所有树型实体集合类的基类
    /// 
    /// 树型实体列表的结构是比较复杂的：
    /// * 列表中中存放了整颗树的所有实体，并保证按照深度迭归的顺序存放。
    /// * 列表中每一个树型实体的 TreeParent、TreeChildren 属性分别表示树型实体间的父子关系。
    /// * 对于 TreeParent、TreeChildren 属性的变更，都会直接影响到实体在列表中的分布。
    /// 
    /// 另外，如果要获取整个树型列表的根结点，可以使用 FindRoots 方法。
    /// </summary>
    /// <typeparam name="TList"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public abstract partial class EntityList
    {
        #region 私有字段

        [NonSerialized]
        private bool _autoTreeCodeEnabled;

        [NonSerialized]
        private bool _disableTreeEditing;

        [NonSerialized]
        internal bool TreeRelationLoaded;

        private void OnContructTree()
        {
            //if (!this.SupportTree) return;
        }

        #endregion

        #region 配置属性

        /// <summary>
        /// 有时候一个列表其实只是一小块数据，这时不需要此功能。
        /// </summary>
        public bool AutoTreeCodeEnabled
        {
            get { return this._autoTreeCodeEnabled; }
            set { this._autoTreeCodeEnabled = value; }
        }

        /// <summary>
        /// 是否启用树型编辑操作。
        /// </summary>
        public bool DisableTreeEditing
        {
            get { return this._disableTreeEditing; }
            set { this._disableTreeEditing = value; }
        }

        /// <summary>
        /// 如果支持树型操作，需要重写 TreeId、OrderNo。
        /// </summary>
        public bool SupportTree
        {
            get { return this.FindRepository().SupportTree; }
        }

        #endregion

        /// <summary>
        /// 此方法用于在对象拷贝完成后，设置树型结点对象的 TreeParentEntity 等。
        /// </summary>
        /// <param name="targetList"></param>
        /// <param name="index"></param>
        protected virtual void OnItemCloned(EntityList targetList, int index)
        {
            if (!targetList.SupportTree) return;

            var target = targetList[index] as Entity;
            if (target.TreePId.HasValue)
            {
                //找到父结点所在的位置
                //由于树集合是按照正序排列，所以这里只需要找到 index 之前就可以了。
                var parentId = target.TreePId.Value;
                int parentIndex = -1;
                for (int j = 0; j < index; j++)
                {
                    var parent = targetList[j] as Entity;
                    if (parent.TreeId == parentId) parentIndex = j;
                }
                if (parentIndex < 0) throw new InvalidProgramException("没有在需要拷贝的集合中找到对应的父结点。");

                //找到了父对象，设置关系
                var newParent = this[parentIndex] as Entity;
                var src = this[index] as Entity;
                src.TreeParent = newParent;
            }
        }

        [NonSerialized]
        private bool _removingRecur = false;

        protected override void RemoveItem(int index)
        {
            if (this.SupportTree)
            {
                var item = this[index];

                this.RemoveItemCsla(index);

                //它的父应该也在本列表中，需要把它们之间的关系断开。
                //如果是迭归删除的子对象，那么他的父已经被删除，这时就不需要断开关系了。
                var removingRecurSaved = this._removingRecur;
                if (!removingRecurSaved) { item.TreeParent = null; }

                //删除某个父对象，应该把其所有的子对象都从列表中删除。
                try
                {
                    this._removingRecur = true;
                    for (int i = item.TreeChildren.Count - 1; i >= 0; i--)
                    {
                        //注意，这里会迭归删除。
                        this.Remove(item.TreeChildren[i]);
                    }
                }
                finally
                {
                    this._removingRecur = removingRecurSaved;
                }

                this.AutoCodeByRootChanged(item);
            }
            else
            {
                this.RemoveItemCsla(index);
            }
        }

        private void OnTreeItemInserted(int index, Entity item)
        {
            if (item != null)
            {
                this.AutoCodeByRootChanged(item);

                //添加一个有树型子的父对象，应该把其所有的子对象都加入到列表中。
                var nextIndex = index + 1;
                foreach (var child in item.TreeChildren)
                {
                    //注意，这里会迭归添加。
                    this.Insert(nextIndex++, child);

                    var childrenCount = child.GetTreeChildrenRecur().Count();
                    nextIndex += childrenCount;
                }
            }
        }

        private void AutoCodeByRootChanged(Entity item)
        {
            //向列表中添加根对象时，需要自动计算该实体及其后面实体的 TreeCode
            if (item.TreePId == null && this.AutoTreeCodeEnabled)
            {
                var option = this.FindRepository().TreeCodeOption;
                var i = 0;
                foreach (var root in this.FindRoots())
                {
                    var newCode = option.CalculateCode(null, i++);
                    if (root.TreeCode != newCode)
                    {
                        root.TreeCode = newCode;
                        root.TreeChildren.ResetTreeCode();
                    }
                }
            }
        }

        internal void NotifyLoaded(IRepository repository)
        {
            this._repository = repository;

            //在加载树型列表的时候，应该把关系也加载好。
            if (this.SupportTree)
            {
                if (this.Count > 0)
                {
                    foreach (Entity child in this.ToArray())
                    {
                        var pId = child.TreePId;
                        if (pId != null)
                        {
                            var parent = this.FirstOrDefault(p => p.TreeId == pId);
                            if (parent != null) { child.TreeParent = parent; }
                        }
                    }
                }

                this.TreeRelationLoaded = true;
                this.AutoTreeCodeEnabled = true;
            }
        }

        public IEnumerable<Entity> FindRoots()
        {
            this.EnsureSupportTree();

            return this.Where(n => n.TreePId == null);
        }

        private void EnsureSupportTree()
        {
            if (!this.SupportTree) throw new NotSupportedException("此操作需要本类支持树型操作，请重写 Entity 类的：SupportTree、TreePId 及 OrderNo属性。");
        }
    }
}