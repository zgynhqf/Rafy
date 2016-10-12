/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：树型实体类的基类
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 树的表达方式，由一维的列表结构，改为层次结构。2.0.0 胡庆访 20140531
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy.Serialization;
using Rafy.Serialization.Mobile;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    //树型实体相关的代码。
    public partial class Entity : ITreeComponent, ITreeEntity
    {
        [NonSerialized]
        private Entity _treeParent;

        private EntityTreeChildren _treeChildren;

        /// <summary>
        /// 表明当前的实体是否已经确定是一个叶子节点。
        /// 注意，当本属性返回 false 时，并不表示本节点下还有节点。
        /// （这是因为如果还没有对本节点的子节点进行查询时，是不能确定它是否还有子节点的。）
        /// </summary>
        internal bool IsTreeLeafSure
        {
            get { return this.GetFlags(EntitySerializableFlags.isTreeLeaf); }
            set { this.SetFlags(EntitySerializableFlags.isTreeLeaf, true); }
        }

        /// <summary>
        /// 是否为树型实体。
        /// </summary>
        public bool SupportTree
        {
            [DebuggerStepThrough]
            get
            {
                var repo = this.FindRepository();
                if (repo != null) return repo.SupportTree;
                return false;
            }
        }

        #region TreeIndex 属性

        /// <summary>
        /// 树型实体的树型索引编码属性。
        /// </summary>
        public static readonly Property<string> TreeIndexProperty = P<Entity>.Register(e => e.TreeIndex);
        /// <summary>
        /// 树型实体的树型索引编码
        /// 这个属性是实现树型实体的关键所在！
        /// </summary>
        public string TreeIndex
        {
            get { return this.GetProperty(TreeIndexProperty); }
            set { this.SetProperty(TreeIndexProperty, value); }
        }

        #endregion

        #region TreePId 属性

        /// <summary>
        /// 树型父实体的 Id 属性
        /// </summary>
        public static readonly Property<object> TreePIdProperty = P<Entity>.Register(e => e.TreePId, new PropertyMetadata<object>
        {
            PropertyChangedCallBack = (o, e) => (o as Entity).OnTreePIdChanged(e)
        });
        /// <summary>
        /// 树型父实体的 Id 属性
        /// 
        /// 默认使用存储于数据库中的字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        public object TreePId
        {
            get { return this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }
        /// <summary>
        /// 子类重写此方法实现 TreePId 属性变更逻辑。
        /// </summary>
        /// <param name="e">The <see cref="ManagedPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.InvalidOperationException"></exception>
        protected virtual void OnTreePIdChanged(ManagedPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue;
            if (newValue != null)
            {
                RemoveFromParentList(true);

                #region 重新加载 TreeIndex、TreeParent

                //如果没有加载树的父节点，说明是外界直接设置的 TreePId。
                //此时，需要加载父节点，并通过父节点来计算本节点的索引。
                if (_treeParent == null || !_treeParent.Id.Equals(newValue))
                {
                    var hasId = this.IdProvider.IsAvailable(newValue);
                    if (hasId)
                    {
                        //直接设置 TreeParent 属性，在 TreeParent 属性中会重新生成 TreeIndex。
                        var repo = this.GetRepository() as IRepositoryInternal;
                        //var count = repo.CountByTreePId(newValue);
                        //this.TreeIndex = repo.TreeIndexOption.CalculateCode()
                        //在查询父节点时，同时把父节点的一级子节点也查询出来，这样可以防止多次查询。
                        var tree = repo.GetByIdOrTreePId(newValue);
                        if (tree.Count == 0)
                        {
                            throw new InvalidOperationException(string.Format(
                                "设置 TreePId 失败：设置的 TreePId 的值是：{0}，在仓库中没有找到 Id 是这个值的节点。",
                                newValue
                                ));
                        }
                        this.TreeParent = tree[0];
                    }
                }

                #endregion
            }
            else
            {
                /*********************** 代码块解释 *********************************
                 * 只需要简单地清空 _treeParent，而没有再重新整理 _treeParent 中子节点的索引。
                 * 原因：
                 * 1.如果只是获取单一实体，并设置它的 TreePId 为空。
                 * 那么保存到数据库中后，原 _treeParent 中的子节点会出现断码，但此时索引的顺序还是正确的。
                 * 2.另外，就算这里重新获取 _treeParent 并重新整理其下的索引，
                 * 但是应用层只是在保存单一实体，这些变更的索引也无法保存到数据库中，同样形成断码。
                 * 所以，
                 * 为了简单起见，这种情况就先容忍了。应用层为了保证不出现断码，应该尽量使用 TreeChildren 集合。
                **********************************************************************/
                _treeParent = null;
            }
        }

        /// <summary>
        /// 如果一个根节点变为非根节点，那么需要调用此方法，使它从 List 中移除。
        /// </summary>
        /// <param name="addIntoDeletedList">是否在删除完成后，添加到删除列表中。</param>
        private void RemoveFromParentList(bool addIntoDeletedList)
        {
            var parentList = (this as IEntity).ParentList;
            if (parentList != null)
            {
                //由于当前节点可能是 parentList 中的第一个，所以不能使用 IsTreeRootList 属性来直接检测是否在根节点集合中。
                if (parentList.Count > 1)
                {
                    var toCheck = parentList[0];
                    if (toCheck == this) toCheck = parentList[1];
                    if (toCheck.TreePId == null)
                    {
                        //从列表中删除本对象。
                        parentList.Remove(this);

                        //是否需要从父列表的删除队列中移除。
                        if (!addIntoDeletedList)
                        {
                            var deletedList = parentList.DeletedListField;
                            if (deletedList != null)
                            {
                                deletedList.Remove(this);
                                if (deletedList.Count == 0)
                                {
                                    parentList.DeletedListField = null;
                                }
                            }
                        }

                        DisconnectFromParent();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 返回当前的 TreeParent 的值是否已经加载。
        /// </summary>
        bool ITreeEntity.IsTreeParentLoaded
        {
            get { return _treeParent != null || this.TreePId == null; }
        }

        /// <summary>
        /// 封装 _treeParent 与 TreePId 之间的关系
        /// </summary>
        internal Entity TreeParentData
        {
            get { return _treeParent; }
            set
            {
                if (_treeParent != value)
                {
                    _treeParent = value;

                    if (value != null)
                    {
                        this.TreePId = value.Id;
                    }
                    else
                    {
                        this.TreePId = null;
                    }
                }
            }
        }

        /// <summary>
        /// 树中的父对象。
        /// <remarks>
        /// 操作此属性，同样引起 TreeChildren、EntityList 的变化。
        /// 同时，注意此属性并不是懒加载属性。
        /// </remarks>
        /// </summary>
        /// <value>
        /// The tree parent.
        /// </value>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Entity TreeParent
        {
            get
            {
                if (_treeParent == null)
                {
                    var treePId = this.TreePId;
                    if (treePId != null)
                    {
                        if (this.IdProvider.IsAvailable(treePId))
                        {
                            var repo = this.GetRepository();
                            _treeParent = repo.GetById(treePId);
                        }
                    }
                }

                return _treeParent;
            }
            set
            {
                /*********************** 代码块解释 *********************************
                 * 注意几个属性的依赖层次：
                 * TreeParent
                 * TreeChildren
                 * TreeParentData、TreeChildren._nodes
                 * TreePId
                **********************************************************************/

                if (_treeParent != value)
                {
                    if (value == null)
                    {
                        //设置 value 为 null 时执行以下操作，同时，该节点会从整个树中删除。
                        if (_treeParent != null)
                        {
                            _treeParent.TreeChildren.Remove(this);
                            //throw new InvalidOperationException(string.Format(
                            //    "树节点 {0} 已经加载了它的父节点 {1}，不能直接设置 TreeParent 属性为 null。要从父节点中移除，请使用父节点的 TreeChildren 集合操作。",
                            //    this, _treeParent
                            //    ));
                        }
                        this.TreeParentData = null;
                    }
                    else
                    {
                        value.TreeChildren.Add(this);
                    }
                }
            }
        }

        /// <summary>
        /// 此节点在树中的级别。
        /// 根节点是第一级。
        /// 此级别是完全根据 <see cref="TreeIndex"/> 计算出来的。
        /// 
        /// 如果此实体不是一个树实体，则返回 -1。
        /// </summary>
        int ITreeEntity.TreeLevel
        {
            get
            {
                var repo = this.FindRepository();
                if (repo != null && repo.SupportTree)
                {
                    var option = repo.TreeIndexOption;
                    return option.CountLevel(this.TreeIndex);
                }
                return -1;
            }
        }

        /// <summary>
        /// 树中的子对象集合。
        /// <remarks>
        /// 操作此属性，同样引起 TreeParent、EntityList 的变化。
        /// 同时，注意此属性并不是懒加载属性。
        /// </remarks>
        /// </summary>
        public EntityTreeChildren TreeChildren
        {
            [DebuggerStepThrough]
            get
            {
                if (_treeChildren == null && this.SupportTree)
                {
                    _treeChildren = new EntityTreeChildren(this);
                }

                return _treeChildren;
            }
        }

        ///// <summary>
        ///// 用于判断是否已经加载了 <see cref="TreeChildren"/> 属性。
        ///// 直接使用 TreeChildren 属性来进行一些判断，可能会创建新的 <see cref="EntityTreeChildren"/> 实例。
        ///// 所以给出此属性，以优化性能。
        ///// </summary>
        //public bool HasTreeChildrenField
        //{
        //    get { return _treeChildren != null; }
        //}

        internal EntityTreeChildren TreeChildrenField
        {
            get { return _treeChildren; }
        }

        private void OnTreeItemCloned(Entity source, CloneOptions options)
        {
            if (options.HasAction(CloneActions.ParentRefEntity))
            {
                this.TreeParentData = source._treeParent;
            }

            if (options.HasAction(CloneActions.GrabChildren))
            {
                _treeChildren = source._treeChildren;
                if (_treeChildren != null)
                {
                    for (int i = 0, c = _treeChildren.Count; i < c; i++)
                    {
                        var child = _treeChildren[i];
                        child._treeParent = this;
                    }
                }
            }
            else if (options.HasAction(CloneActions.ChildrenRecur))
            {
                if (source._treeChildren == null)
                {
                    _treeChildren = null;
                }
                else
                {
                    this.TreeChildren.Clone(source._treeChildren, options);
                }
            }
        }

        private void EnsureSupportTree()
        {
            if (!this.SupportTree) throw new NotSupportedException("此操作需要本类支持树型操作，请重写：SupportTree、TreePId、OrderNo。");
        }

        private void SyncTreeChildrenPId()
        {
            if (_treeChildren != null && _treeChildren.IsLoaded)
            {
                for (int i = 0, c = _treeChildren.Count; i < c; i++)
                {
                    var treeChild = _treeChildren[i];
                    treeChild.TreePId = this.Id;
                }
            }
        }

        #region ITreeComponent

        bool ITreeComponent.IsFullLoaded
        {
            get
            {
                //如果确定是个叶子，返回真。
                if (this.IsTreeLeafSure) { return true; }

                return this.TreeChildren.IsFullLoaded;
            }
        }

        void ITreeComponent.LoadAllNodes()
        {
            if (!this.IsTreeLeafSure)
            {
                this.TreeChildren.LoadAllNodes();
            }
        }

        Entity ITreeComponent.EachNode(Func<Entity, bool> action)
        {
            var found = action(this);
            if (found) return this;

            if (_treeChildren != null) return _treeChildren.EachNode(action);

            return null;
        }

        int ITreeComponent.CountNodes()
        {
            return TreeHelper.CountNodes(this);
        }

        ITreeComponent ITreeComponent.TreeComponentParent
        {
            get
            {
                if (_treeParent != null) { return _treeParent._treeChildren; }
                return (this as IEntity).ParentList;
            }
        }

        TreeComponentType ITreeComponent.ComponentType
        {
            get { return TreeComponentType.Node; }
        }

        #endregion

        /// <summary>
        /// 树型实体的子实体列表类。
        /// </summary>
        /// 防止重入、设置父子关系
        [Serializable]
        [DebuggerDisplay("{DebuggerDisplay}")]
        public sealed class EntityTreeChildren : IList<ITreeEntity>, IList<Entity>, ITreeComponent
        {
            #region 字段

            [NonSerialized]
            private Entity _owner;

            /// <summary>
            /// 是否自动在集体变更时计算 TreeIndex，默认：null。表示从最上层组件中取值。
            /// </summary>
            [NonSerialized]
            private bool? _autoIndexEnabled;

            /// <summary>
            /// 返回当前集合中的节点是否已经加载完成。
            /// </summary>
            private bool _loaded;

            /// <summary>
            /// 当前已经加载的所有子节点。
            /// 如果实体是叶子节点，那么它没有任何的子节点，这个列表将保持 null。
            /// </summary>
            private List<Entity> _nodes;

            /// <summary>
            /// 需要被删除的节点。
            /// </summary>
            private List<Entity> _deleted;

            #endregion

            /// <summary>
            /// 序列化使用。
            /// </summary>
            private EntityTreeChildren() { }

            internal EntityTreeChildren(Entity owner)
            {
                _owner = owner;

                //如果是一个全新的对象，那么默认值就是已经加载完成的。
                _loaded = owner.IsNew || owner.IsTreeLeafSure;
            }

            /// <summary>
            /// 在二进制反序列化后调用此方法，来重建父子节点之间的关系。
            /// </summary>
            /// <param name="owner"></param>
            internal void NotifyDeserialized(Entity owner)
            {
                _owner = owner;

                if (_nodes != null)
                {
                    for (int i = 0, c = _nodes.Count; i < c; i++)
                    {
                        _nodes[i]._treeParent = owner;
                    }
                }

                if (_deleted != null)
                {
                    for (int i = 0, c = _deleted.Count; i < c; i++)
                    {
                        _deleted[i]._treeParent = owner;
                    }
                }
            }

            internal void Clone(EntityTreeChildren source, CloneOptions options)
            {
                _loaded = source._loaded;
                if (_loaded)
                {
                    var srcNodes = source._nodes;
                    if (srcNodes != null)
                    {
                        _nodes = new List<Entity>();

                        var repo = _owner.FindRepository();
                        var entityType = _owner.GetType();

                        for (int i = 0, c = srcNodes.Count; i < c; i++)
                        {
                            var src = srcNodes[i];

                            Entity entity = null;
                            if (repo != null)
                            {
                                entity = repo.New();
                            }
                            else
                            {
                                entity = Entity.New(entityType);
                            }

                            entity.Clone(src, options);

                            entity._treeParent = _owner;
                            _nodes.Add(entity);
                        }
                    }
                }
            }

            internal List<Entity> DeletedListField
            {
                get { return _deleted; }
                set { _deleted = value; }
            }

            #region 加载节点数据

            /// <summary>
            /// 从数据库中查询时，使用此方法来为集合快速添加元素。
            /// </summary>
            /// <param name="item"></param>
            internal void LoadAdd(Entity item)
            {
                if (_nodes == null) { _nodes = new List<Entity>(); }

                _nodes.Add(item);

                item._treeParent = _owner;
            }

            /// <summary>
            /// 当从数据库中添加完毕时，使用此方法标记集合中的节点已经加载完全。
            /// </summary>
            internal void MarkLoaded()
            {
                _loaded = true;
            }

            #endregion

            #region IDirtyAware

            /// <summary>
            /// 当前的模型，是否是脏的。
            /// 一个脏的对象，表示它的状态还没有保存起来。
            /// </summary>
            public bool IsDirty
            {
                get
                {
                    if (_nodes != null)
                    {
                        for (int i = 0, c = _nodes.Count; i < c; i++)
                        {
                            var child = _nodes[i];
                            if (child.IsDirty) return true;
                        }
                    }

                    if (_deleted != null && _deleted.Count > 0) return true;

                    return false;
                }
            }

            /// <summary>
            /// 标记为已经保存。IsDirty 为 false。
            /// </summary>
            public void MarkSaved()
            {
                _deleted = null;

                if (_nodes != null)
                {
                    for (int i = 0, c = _nodes.Count; i < c; i++)
                    {
                        var child = _nodes[i];
                        (child as IDirtyAware).MarkSaved();
                    }
                }
            }

            #endregion

            #region 懒加载节点

            /// <summary>
            /// 返回当前集合中的节点元素是否已经加载完成。
            /// </summary>
            public bool IsLoaded
            {
                get { return _loaded; }
            }

            /// <summary>
            /// 返回当前树是否已经加载完全。
            /// </summary>
            public bool IsFullLoaded
            {
                get
                {
                    if (_loaded)
                    {
                        //叶子节点，直接返回 true。
                        if (_nodes == null) { return true; }

                        for (int i = 0, c = _nodes.Count; i < c; i++)
                        {
                            var node = _nodes[i] as ITreeComponent;
                            if (!node.IsFullLoaded) return false;
                        }

                        return true;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 加载当前集合中的节点元素。
            /// </summary>
            public void Load()
            {
                if (!_loaded)
                {
                    var repo = _owner.GetRepository();
                    var children = repo.GetByTreePId(_owner.Id);

                    this.MergeFullTree(children.ToList());
                }
            }

            /// <summary>
            /// 递归加载所有树节点。
            /// </summary>
            /// <exception cref="System.InvalidProgramException">还没有存储到数据库中的节点，它的 IsFullLoaded 属性应该返回 true。</exception>
            public void LoadAllNodes()
            {
                if (!this.IsFullLoaded)
                {
                    var repo = _owner.GetRepository();
                    var dbOwner = _owner;
                    if (_owner._status != PersistenceStatus.Unchanged)
                    {
                        dbOwner = repo.GetById(_owner.Id);
                        if (dbOwner == null)
                        {
                            throw new InvalidProgramException("还没有存储到数据库中的节点，它的 IsFullLoaded 属性应该返回 true。");
                        }
                    }
                    var children = repo.GetByTreeParentIndex(dbOwner.TreeIndex);
                    this.MergeFullTree(children.ToList());
                }
            }

            /// <summary>
            /// 把完整加载的子树，合并到当前树中。
            /// </summary>
            /// <param name="fullLoadedChildren">一个已经从数据库中完整加载的子树。该集合可以是 null。</param>
            internal void MergeFullTree(List<Entity> fullLoadedChildren)
            {
                if (!_loaded)
                {
                    _nodes = fullLoadedChildren;
                    if (_nodes != null && _nodes.Count > 0)
                    {
                        for (int i = 0, c = _nodes.Count; i < c; i++)
                        {
                            var child = _nodes[i];
                            child._treeParent = _owner;
                        }
                    }
                    else
                    {
                        //加载后，如果发现没有子节点，则修改实体的属性。
                        _owner.IsTreeLeafSure = true;
                    }

                    _loaded = true;
                }
                else
                {
                    if (_nodes != null && fullLoadedChildren != null && fullLoadedChildren.Count > 0)
                    {
                        //由于当前集合已经加载完毕，所以直接遍历 _nodes 中的所有节点即可。
                        for (int i = 0, c = _nodes.Count; i < c; i++)
                        {
                            var child = _nodes[i];

                            //由于有一些节点是新加入到集合中的，
                            //所以我们需要先尝试在数据库集合中查找对应的节点。
                            var loadedChild = fullLoadedChildren.Find(e => e.Id.Equals(child.Id));
                            if (loadedChild != null)
                            {
                                //同时，递归检查该子节点如果没有加载完成，则进行合并。
                                var childChildren = child.TreeChildren;
                                if (!childChildren.IsFullLoaded)
                                {
                                    childChildren.MergeFullTree(loadedChild.TreeChildren._nodes);
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region 自动索引

            /// <summary>
            /// 是否自动在集体变更时计算 TreeIndex，默认：null。表示从上层列表中取值。
            /// </summary>
            public bool? AutoIndexEnabled
            {
                get { return _autoIndexEnabled; }
                set { _autoIndexEnabled = value; }
            }

            private void TryAutoIndex(int from)
            {
                if (this.IsAutoTreeIndexEnabled()) { this.ResetTreeIndex(from); }
            }

            /// <summary>
            /// 是否自动在集体变更时计算 TreeIndex，默认：null。表示从最上层组件中取值。
            /// </summary>
            /// <returns></returns>
            private bool IsAutoTreeIndexEnabled()
            {
                if (_autoIndexEnabled.HasValue)
                {
                    return _autoIndexEnabled.Value;
                }

                var ocp = (_owner as ITreeComponent).TreeComponentParent;
                if (ocp != null)
                {
                    switch (ocp.ComponentType)
                    {
                        case TreeComponentType.NodeList:
                            return (ocp as EntityList).AutoTreeIndexEnabled;
                        case TreeComponentType.Node:
                            ocp = ocp.TreeComponentParent;
                            if (ocp != null)
                            {
                                switch (ocp.ComponentType)
                                {
                                    case TreeComponentType.NodeList:
                                        return (ocp as EntityList).AutoTreeIndexEnabled;
                                    case TreeComponentType.TreeChildren:
                                        return (ocp as EntityTreeChildren).IsAutoTreeIndexEnabled();
                                    default:
                                        throw new InvalidProgramException("实体的 TreeComponentParent 只能是 EntityList 或 EntityTreeChildren 两种类型。");
                                }
                            }
                            break;
                        case TreeComponentType.TreeChildren:
                            return (ocp as EntityTreeChildren).IsAutoTreeIndexEnabled();
                        default:
                            break;
                    }
                }

                return true;
            }

            /// <summary>
            /// 根据当前对象的 TreeIndex 重设当前对象的整个树型子列表的 TreeIndex。
            /// </summary>
            /// <param name="from">从指定的索引开始重新整理</param>
            public void ResetTreeIndex(int from = 0)
            {
                this.LoadAllNodes();

                this.ResetTreeIndexSimple(from);
            }

            /// <summary>
            /// 根据当前对象的 TreeIndex 重设当前对象树中子节点的 TreeIndex。
            /// </summary>
            /// <param name="from">From.</param>
            private void ResetTreeIndexSimple(int from)
            {
                if (_nodes != null)
                {
                    var option = _owner.GetRepository().TreeIndexOption;
                    var pIndex = _owner.TreeIndex;

                    for (int i = from, c = _nodes.Count; i < c; i++)
                    {
                        var child = _nodes[i];

                        var newIndex = option.CalculateChildIndex(pIndex, i);

                        //不管父对象的 TreeIndex 是否改变，子对象集合的 TreeIndex 都需要重新计算一遍。
                        //这是因为有可能子集合发生了改变。
                        //if (child.TreeIndex != newIndex)
                        //{
                        child.TreeIndex = newIndex;
                        child.TreeChildren.ResetTreeIndexSimple(0);
                        //}
                    }
                }
            }

            #endregion

            #region 查询节点

            /// <summary>
            /// 统计当前树中已经加载的节点的个数。
            /// </summary>
            /// <returns></returns>
            public int CountNodes()
            {
                if (_nodes == null) { return 0; }

                return TreeHelper.CountNodes(this);
            }

            /// <summary>
            /// 递归对于整个树中的每一个节点都调用 action。
            /// </summary>
            /// <param name="action">对每一个节点调用的方法。方法如何返回 true，则表示停止循环，返回该节点。</param>
            /// <returns>第一个被调用 action 后返回 true 的节点。</returns>
            public Entity EachNode(Func<Entity, bool> action)
            {
                if (_nodes != null)
                {
                    for (int i = 0, c = _nodes.Count; i < c; i++)
                    {
                        var found = TravelDepthFirst(_nodes[i], action);
                        if (found != null) return found;
                    }
                }
                return null;
            }

            private static Entity TravelDepthFirst(Entity node, Func<Entity, bool> action)
            {
                if (node == null) throw new ArgumentNullException("node");
                if (action == null) throw new ArgumentNullException("action");

                var stack = new Stack<Entity>();
                stack.Push(node);

                while (stack.Count > 0)
                {
                    var currentNode = stack.Pop();

                    var found = action(currentNode);
                    if (found) { return currentNode; }

                    var children = currentNode.TreeChildren._nodes;
                    if (children != null)
                    {
                        for (int i = children.Count - 1; i >= 0; i--)
                        {
                            stack.Push(children[i]);
                        }
                    }
                }

                return null;
            }

            /// <summary>
            /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
            /// <returns>
            /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
            /// </returns>
            public int IndexOf(Entity item)
            {
                this.Load();
                if (_nodes == null) { return -1; }
                return _nodes.IndexOf(item);
            }

            /// <summary>
            /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
            /// </summary>
            /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
            /// </returns>
            public bool Contains(Entity item)
            {
                this.Load();
                if (_nodes == null) return false;
                return _nodes.Contains(item);
            }

            /// <summary>
            /// Copies to specific array.
            /// </summary>
            /// <param name="array">The array.</param>
            /// <param name="arrayIndex">Index of the array.</param>
            public void CopyTo(Entity[] array, int arrayIndex)
            {
                this.Load();
                if (_nodes == null) return;

                _nodes.CopyTo(array, arrayIndex);
            }

            /// <summary>
            /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.</returns>
            public int Count
            {
                get
                {
                    this.Load();
                    return _nodes != null ? _nodes.Count : 0;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
            /// </summary>
            /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only; otherwise, false.</returns>
            public bool IsReadOnly
            {
                get { return false; }
            }

            #endregion

            #region 添加节点

            /// <summary>
            /// Gets or sets the element at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
            public Entity this[int index]
            {
                get
                {
                    this.Load();
                    if (_nodes == null) throw new ArgumentOutOfRangeException("index");
                    return _nodes[index];
                }
                set
                {
                    this.SetItem(index, value);
                }
            }

            private void SetItem(int index, Entity value)
            {
                this.OnItemAdding(value);

                this.LoadAllNodes();

                if (_nodes == null) throw new ArgumentOutOfRangeException("index");

                var oldNode = _nodes[index];
                _nodes[index] = value;

                this.OnItemAdded(index, value);
                this.OnItemRemoved(index, oldNode);
            }

            /// <summary>
            /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            public void Add(Entity item)
            {
                this.OnItemAdding(item);

                this.Load();

                if (_nodes == null) { _nodes = new List<Entity>(); }
                _nodes.Add(item);

                this.OnItemAdded(_nodes.Count - 1, item);
            }

            /// <summary>
            /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
            /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
            public void Insert(int index, Entity item)
            {
                this.OnItemAdding(item);

                this.LoadAllNodes();

                if (_nodes == null) { _nodes = new List<Entity>(); }
                _nodes.Insert(index, item);

                this.OnItemAdded(index, item);
            }

            /// <summary>
            /// Called when [item adding].
            /// </summary>
            /// <param name="item">The item.</param>
            /// <exception cref="System.ArgumentNullException">item;添加的元素不能为 null。</exception>
            /// <exception cref="System.InvalidProgramException">该节点在本集合中只能出现一次。</exception>
            private void OnItemAdding(Entity item)
            {
                if (item == null) throw new ArgumentNullException("item", "添加的元素不能为 null。");

                if (item._treeParent == _owner)
                {
                    //如果节点不在删除列表中，则说明它已经在本集合的 _nodes 中了。
                    if (_deleted == null || !_deleted.Remove(item))
                    {
                        throw new InvalidProgramException(string.Format(
                            "节点 {0} 在父节点的树子集合中只能出现一次。", item
                            ));
                    }
                }

                item.RemoveFromParentList(false);

                if (!_owner.IsDeleted && item.IsDeleted)
                {
                    (item as ITreeComponent).EachNode(e =>
                    {
                        (e as IEntityWithStatus).RevertDeletedStatus();
                        return false;
                    });
                }
            }

            /// <summary>
            /// 当集合中新加入一个全新的节点时，调用此方法。
            /// </summary>
            /// <param name="index">The index.</param>
            /// <param name="item">The item.</param>
            private void OnItemAdded(int index, Entity item)
            {
                //一个节点在树中只能存在一次，所以此处会断开此节点与原有父节点之间的关系。
                if (item._treeParent != _owner)
                {
                    DisconnectFromTreeParent(item);
                }

                item.TreeParentData = _owner;
                _owner.IsTreeLeafSure = false;

                this.ResetTreeIndexSimple(index);
            }

            /// <summary>
            /// 断开此节点与原有父节点之间的关系。
            /// </summary>
            /// <param name="child"></param>
            internal static void DisconnectFromTreeParent(Entity child)
            {
                var oldParent = child._treeParent;
                if (oldParent != null)
                {
                    var oldTreeChildren = oldParent._treeChildren;
                    if (oldTreeChildren != null)
                    {
                        //从 _nodes 中删除。
                        var nodes = oldTreeChildren._nodes;
                        if (nodes != null)
                        {
                            var index  = nodes.IndexOf(child);
                            if (index >= 0)
                            {
                                nodes.RemoveAt(index);
                                oldTreeChildren.OnItemRemoved(index, child, false);
                            }
                        }

                        //从 _deleted 中删除。
                        var oldDeleted = oldTreeChildren._deleted;
                        if (oldDeleted != null)
                        {
                            oldDeleted.Remove(child);
                            if (oldDeleted.Count == 0)
                            {
                                oldTreeChildren._deleted = null;
                            }
                        }
                    }

                    //断开父子关系。
                    child._treeParent = null;
                }
            }

            #endregion

            #region 移除节点

            /// <summary>
            /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the item to remove.</param>
            /// <exception cref="System.ArgumentOutOfRangeException">index</exception>
            public void RemoveAt(int index)
            {
                this.Load();

                if (_nodes == null) throw new ArgumentOutOfRangeException("index");

                var oldItem = _nodes[index];

                _nodes.RemoveAt(index);

                this.OnItemRemoved(index, oldItem);
            }

            /// <summary>
            /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
            /// <returns>
            /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </returns>
            public bool Remove(Entity item)
            {
                this.Load();

                if (_nodes != null)
                {
                    int num = _nodes.IndexOf(item);
                    if (num >= 0)
                    {
                        this.RemoveAt(num);
                        return true;
                    }
                }

                return false;
            }

            private void OnItemRemoved(int oldIndex, Entity item, bool addToDeletedList = true)
            {
                //是否把刚删除的这个节点，添加到待删除列表中。（internal 删除，可以不加入列表中。）
                if (addToDeletedList && !item.IsNew)
                {
                    if (_deleted == null)
                    {
                        _deleted = new List<Entity>();
                    }
                    else
                    {
                        if (_deleted.Contains(item))
                        {
                            return;
                        }
                    }

                    _deleted.Add(item);
                    (item as ITreeComponent).EachNode(e =>
                    {
                        e.PersistenceStatus = PersistenceStatus.Deleted;
                        return false;
                    });
                }

                /*********************** 代码块解释 *********************************
                 * 从子集合中删除子节点时，不应该清空子节点的 TreeParent，TreePId 字段，
                 * 否则在保存时，无法得知删除列表中的节点是否还是当前集合的子节点。
                 * 
                 * 子集合中删除节点 a 后，a 的状态是，TreeParent，TreePId不变，isDeleted 属性为真。
                 * 表示这个节点还属于原来的父节点，但是是被删除的状态。
                 * 
                 * 以下逻辑不应该应用到新添加的对象上。
                **********************************************************************/
                if (item.IsNew)
                {
                    item.TreeParentData = null;
                }

                if (_nodes.Count == 0)
                {
                    _nodes = null;
                    _owner.IsTreeLeafSure = true;
                }
                else
                {
                    this.TryAutoIndex(oldIndex);
                }
            }

            /// <summary>
            /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
            /// </summary>
            public void Clear()
            {
                this.Load();

                if (_nodes != null)
                {
                    //把清空的节点都加入到 _deleted 列表中。
                    if (_deleted == null)
                    {
                        _deleted = _nodes;
                    }
                    else
                    {
                        _deleted.AddRange(_nodes);
                    }

                    _nodes = null;
                }

                _owner.IsTreeLeafSure = true;
            }

            #endregion

            #region 其它接口实现

            /// <summary>
            /// Returns an enumerator that iterates through the collection.
            /// </summary>
            /// <returns>
            /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
            /// </returns>
            public IEnumerator<Entity> GetEnumerator()
            {
                this.Load();

                return _nodes != null ?
                    _nodes.GetEnumerator() :
                    Enumerable.Empty<Entity>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            int IList<ITreeEntity>.IndexOf(ITreeEntity item)
            {
                return this.IndexOf(item as Entity);
            }

            void IList<ITreeEntity>.Insert(int index, ITreeEntity item)
            {
                this.Insert(index, item as Entity);
            }

            void ICollection<ITreeEntity>.Add(ITreeEntity item)
            {
                this.Add(item as Entity);
            }

            bool ICollection<ITreeEntity>.Contains(ITreeEntity item)
            {
                return this.Contains(item as Entity);
            }

            void ICollection<ITreeEntity>.CopyTo(ITreeEntity[] array, int arrayIndex)
            {
                this.Load();
                if (_nodes == null) return;

                for (int i = 0, c = _nodes.Count, c2 = array.Length; i < c && arrayIndex < c2; i++, arrayIndex++)
                {
                    array[arrayIndex] = _nodes[i];
                }
            }

            bool ICollection<ITreeEntity>.Remove(ITreeEntity item)
            {
                return this.Remove(item as Entity);
            }

            IEnumerator<ITreeEntity> IEnumerable<ITreeEntity>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            ITreeEntity IList<ITreeEntity>.this[int index]
            {
                get
                {
                    return this[index];
                }

                set
                {
                    this[index] = value as Entity;
                }
            }

            ITreeComponent ITreeComponent.TreeComponentParent
            {
                get { return _owner; }
            }

            TreeComponentType ITreeComponent.ComponentType
            {
                get { return TreeComponentType.TreeChildren; }
            }

            private string DebuggerDisplay
            {
                get
                {
                    if (_loaded)
                    {
                        return string.Format("Loaded！  Count:{0}  Owner:{1}", this.Count, _owner);
                    }
                    return string.Format("Unloaded！  Owner:{0}", _owner);
                }
            }

            #endregion

            //public void Move(int index, MoveDirection dir)
            //{
            //    var i = this[index];
            //    var nextIndex = index + (dir == MoveDirection.Up ? 1 : -1);
            //    if (nextIndex >= 0 && nextIndex < this.Count)
            //    {
            //        var j = this[nextIndex];
            //        this[nextIndex] = i;
            //        this[index] = j;
            //    }
            //}
            //public enum MoveDirection { Up, Down }
        }
    }
}