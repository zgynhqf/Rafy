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
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Rafy.Domain
{
    //所有树型实体集合类的基类
    partial class EntityList : ITreeComponent
    {
        #region 配置属性

        [NonSerialized]
        private bool _autoTreeIndexEnabled = true;

        /// <summary>
        /// 是否启用树型的自动生成索引功能。默认为 true。
        /// </summary>
        public bool AutoTreeIndexEnabled
        {
            get { return _autoTreeIndexEnabled; }
            set { _autoTreeIndexEnabled = value; }
        }

        /// <summary>
        /// 如果支持树型操作，需要重写 TreeId、OrderNo。
        /// </summary>
        public bool SupportTree
        {
            get
            {
                var repo = this.FindRepository();
                if (repo != null) return repo.SupportTree;
                return false;
            }
        }

        /// <summary>
        /// 返回当前的这个列表是否作为树结构中的根节点的集合。
        /// 注意，如果集合中没有元素时，同样会返回 false。
        /// 
        /// 如果本属性是 false 时，那么 EntityList 中与树相关的功能都不再可用。
        /// </summary>
        public bool IsTreeRootList
        {
            get
            {
                //如果本集合中有元素，而且第一个实体是一个根节点，则返回 true。
                return this.SupportTree && this.Count > 0 && this[0].TreePId == null;
            }
        }

        #endregion

        internal void LoadData(IEnumerable srcList)
        {
            if (this.SupportTree)
            {
                bool autoIndexEnabled = this.AutoTreeIndexEnabled;
                try
                {
                    //在加载数据时，自动索引功能都不可用。
                    this.AutoTreeIndexEnabled = false;

                    TreeHelper.LoadTreeData(this, srcList, this.GetRepository().TreeIndexOption);
                }
                finally
                {
                    this.AutoTreeIndexEnabled = autoIndexEnabled;
                }
            }
            else
            {
                foreach (Entity item in srcList) { this.Add(item); }
            }
        }

        private void OnTreeItemRemoving(Entity item)
        {
            if (!item.IsNew)
            {
                if (item.TreeChildrenField != null)
                {
                    item.TreeChildrenField.EachNode(c =>
                    {
                        c.PersistenceStatus = PersistenceStatus.Deleted;
                        return false;
                    });
                }
            }
        }

        private void OnTreeItemRemoved(int index, Entity item)
        {
            if (this.IsTreeRootList)
            {
                this.TryAutoIndex(index);
            }
        }

        private void OnTreeItemInserted(int index, Entity item)
        {
            if (this.IsTreeRootList)
            {
                Entity.EntityTreeChildren.DisconnectFromTreeParent(item);
                item.TreePId = null;

                this.TryAutoIndex(index);
            }
        }

        private void OnTreeItemsMoved()
        {
            if (this.IsTreeRootList)
            {
                this.TryAutoIndex();
            }
        }

        /// <summary>
        /// 对从指定的索引开始的根节点进行自动索引。
        /// </summary>
        /// <param name="from">从指定索引的节点开始。</param>
        /// <param name="force">是否强制修改 TreeIndex。</param>
        private void TryAutoIndex(int from = 0, bool force = false)
        {
            //向列表中添加根对象时，需要自动计算该实体及其后面实体的 TreeIndex
            if (_autoTreeIndexEnabled)
            {
                var option = this.GetRepository().TreeIndexOption;

                for (int i = from, c = this.Count; i < c; i++)
                {
                    var newIndex = option.CalculateChildIndex(null, i);

                    var root = this[i];
                    if (force || root.TreeIndex != newIndex)
                    {
                        root.TreeIndex = newIndex;
                        root.TreeChildren.ResetTreeIndex();
                    }
                }
            }
        }

        /// <summary>
        /// 如果实体支持树，那么递归对于整个树中的每一个节点都调用 action。
        /// 否则，只是简单地遍历整个集合。
        /// </summary>
        /// <param name="action">对每一个节点调用的方法。方法如何返回 true，则表示停止循环，返回该节点。</param>
        /// <returns>
        /// 第一个被调用 action 后返回 true 的节点。
        /// </returns>
        public Entity EachNode(Func<Entity, bool> action)
        {
            if (this.SupportTree)
            {
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var foundItem = (this[i] as ITreeComponent).EachNode(action);
                    if (foundItem != null) return foundItem;
                }
            }
            else
            {
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var item = this[i];
                    var found = action(item);
                    if (found) return item;
                }
            }

            return null;
        }

        /// <summary>
        /// 如果当前集合是一个根节点的集合，那么可以使用此方法来重新生成树中所有节点的索引。
        /// </summary>
        public void ResetTreeIndex()
        {
            if (!this.IsTreeRootList)
            {
                throw new InvalidOperationException("只有根节点的集合，才能调用本方法。");
            }

            this.TryAutoIndex(0, true);
        }

        internal void NotifyLoaded(IRepository repository)
        {
            _repository = repository;
        }

        //private void EnsureSupportTree()
        //{
        //    if (!this.SupportTree) throw new NotSupportedException("此操作需要本类支持树型操作，请重写 Entity 类的：SupportTree、TreePId 及 OrderNo属性。");
        //}

        #region ITreeComponent

        bool ITreeComponent.IsFullLoaded
        {
            get
            {
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var item = this[i] as ITreeComponent;
                    if (!item.IsFullLoaded) return false;
                }

                return true;
            }
        }

        int ITreeComponent.CountNodes()
        {
            return TreeHelper.CountNodes(this);
        }

        void ITreeComponent.LoadAllNodes()
        {
            if (this.Count > 0)
            {
                if (!this.IsTreeRootList)
                {
                    throw new InvalidOperationException("只有根节点的集合，才能调用本方法。");
                }

                var all = this.GetRepository().GetAll();
                for (int i = 0, c = this.Count; i < c; i++)
                {
                    var item = this[i];
                    if (!item.IsTreeLeafSure && !item.TreeChildren.IsFullLoaded)
                    {
                        var dbItem = all.Find(item.Id);
                        var field = dbItem.TreeChildrenField;
                        if (field != null)
                        {
                            item.TreeChildren.MergeFullTree(field.Cast<Entity>().ToList());
                        }
                    }
                }
            }
        }

        ITreeComponent ITreeComponent.TreeComponentParent
        {
            get
            {
                //如果列表在树的结构中，那么它永远是最上层的组件。
                return null;
            }
        }

        TreeComponentType ITreeComponent.ComponentType
        {
            get { return TreeComponentType.NodeList; }
        }

        #endregion
    }
}