/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：树型实体类的基类
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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using OEA.Serialization;
using OEA.Serialization.Mobile;

namespace OEA.Library
{
    /// <summary>
    /// 树型实体类的基类
    /// </summary>
    public partial class Entity
    {
        #region 公有配置项

        public bool SupportTree
        {
            get { return this.GetRepository().SupportTree; }
        }

        public static readonly Property<string> TreeCodeProperty = P<Entity>.Register(e => e.TreeCode);
        /// <summary>
        /// 树型实体的树型编码
        /// 这个属性是实现树型实体的关键所在！
        /// </summary>
        public virtual string TreeCode
        {
            get { return this.GetProperty(TreeCodeProperty); }
            set { this.SetProperty(TreeCodeProperty, value); }
        }

        public static readonly Property<int?> TreePIdProperty = P<Entity>.Register(e => e.TreePId);
        /// <summary>
        /// 树型父对象的 Id
        /// 
        /// 默认使用存储于数据库中的字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        public virtual int? TreePId
        {
            get { return this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }

        /// <summary>
        /// 在树中的 Id
        /// 
        /// 默认使用存储于数据库中的 Id 字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        public virtual int TreeId
        {
            get { return this.Id; }
        }

        #endregion

        [NonSerialized]
        private Entity _treeParent;

        [NonSerialized]
        private TreeEntityChildren _treeChildren;

        private void OnTreeIdLoaded()
        {
            if (this.IsDataAccessing && this.SupportTree && this._treeChildren != null)
            {
                foreach (var treeChild in this._treeChildren)
                {
                    treeChild.TreePId = this.TreeId;
                }
            }
        }

        /// <summary>
        /// 封装 _treeParent 与 TreePId 之间的关系
        /// </summary>
        private Entity TreeParentData
        {
            get { return this._treeParent; }
            set
            {
                if (this._treeParent != value)
                {
                    if (value != null)
                    {
                        this.TreePId = value.TreeId;
                    }
                    else
                    {
                        this.TreePId = null;
                    }

                    this._treeParent = value;
                }
            }
        }

        public Entity TreeParent
        {
            get { return this.TreeParentData; }
            set
            {
                if (this.TreeParentData != value)
                {
                    var oldParent = this.TreeParentData;

                    if (value != null)
                    {
                        value.TreeChildren.Add(this);
                    }
                    else
                    {
                        //设置 value 为 null 时执行以下操作，同时，该节点会从整个树中删除。
                        if (oldParent != null) oldParent.TreeChildren.Remove(this);
                    }

                    this.TreeParentData = value;
                }
            }
        }

        public TreeEntityChildren TreeChildren
        {
            [DebuggerStepThrough]
            get
            {
                if (this._treeChildren == null)
                {
                    this._treeChildren = new TreeEntityChildren(this);
                }

                return this._treeChildren; ;
            }
        }

        public IEnumerable<Entity> GetTreeChildrenRecur()
        {
            foreach (var item in this.TreeChildren)
            {
                yield return item;

                foreach (var child in item.GetTreeChildrenRecur()) { yield return item; }
            }
        }

        private void OnTreeItemCloned(Entity target, CloneOptions options)
        {
            if (options.HasAction(CloneActions.ParentRefEntity))
            {
                this.TreeParentData = target.TreeParentData;
            }

            if (options.HasAction(CloneActions.GrabChildren))
            {
                this._treeChildren = target._treeChildren;
            }
        }

        //public override void MarkDeleted()
        //{
        //    base.MarkDeleted();

        //    if (this.SupportTree) { this.TreeParent = null; }
        //}

        private void EnsureSupportTree()
        {
            if (!this.SupportTree) throw new NotSupportedException("此操作需要本类支持树型操作，请重写：SupportTree、TreePId、OrderNo。");
        }

        /// <summary>
        /// 防止重入、设置父子关系
        /// </summary>
        public sealed class TreeEntityChildren : Collection<Entity>
        {
            private Entity _owner;

            public TreeEntityChildren(Entity owner)
            {
                this._owner = owner;
            }

            /// <summary>
            /// 是否自动在集体变更时计算 TreeCode，默认：null。表示从上层列表中取值。
            /// </summary>
            public bool? AutoTreeCodeEnabled { get; set; }

            /// <summary>
            /// 根据当前对象的 TreeCode 重设当前对象的整个树型子列表的 TreeCode
            /// </summary>
            /// <param name="recur"></param>
            public void ResetTreeCode()
            {
                if (this.Count > 0)
                {
                    var option = this._owner.GetRepository().TreeCodeOption;

                    var pCode = this._owner.TreeCode;
                    for (int i = 0, c = this.Count; i < c; i++)
                    {
                        var child = this[i];

                        child.TreeCode = option.CalculateCode(pCode, i);

                        //不管父对象的 TreeCode 是否改变，子对象集合的 TreeCode 都需要重新计算一遍。
                        //这是因为有可能子集合发生了改变。
                        child.TreeChildren.ResetTreeCode();
                    }
                }
            }

            protected override void InsertItem(int index, Entity item)
            {
                //防止多次添加同一个子对象
                if (item.TreeParentData == this._owner) { return; }

                base.InsertItem(index, item);

                this.OnNewItemAdded(index, item);

                this.TryAutoCode();
            }

            protected override void RemoveItem(int index)
            {
                var item = this[index];

                base.RemoveItem(index);

                //直接自动从其所在的列表中删除。
                var list = this._owner.ParentList;
                if (list != null && list.TreeRelationLoaded) { list.Remove(item); }

                item.TreeParentData = null;

                this.TryAutoCode();
            }

            protected override void SetItem(int index, Entity item)
            {
                var old = this[index];
                old.TreeParentData = null;

                base.SetItem(index, item);

                //如果这个项还没有在这个集合中，则表示一个新的元素。
                if (item.TreeParentData != this._owner) { this.OnNewItemAdded(index, item); }

                this.TryAutoCode();
            }

            protected override void ClearItems()
            {
                var list = this._owner.ParentList;
                if (list != null && list.TreeRelationLoaded)
                {
                    for (int i = this.Count - 1; i >= 0; i--)
                    {
                        var item = this[i];
                        list.Remove(item);
                        item.TreeParentData = null;
                    }
                }
                else
                {
                    foreach (var item in this)
                    {
                        item.TreeParentData = null;
                    }
                }

                base.ClearItems();
            }

            private void OnNewItemAdded(int index, Entity item)
            {
                var list = this._owner.ParentList;
                var needRebuildRelation = list != null && list.TreeRelationLoaded;
                if (needRebuildRelation)
                {
                    //断开此实体与原有父对象、原有父列表之间的关系
                    var oldParent = item.TreeParentData;
                    if (oldParent != null) { oldParent.TreeChildren.Remove(item); }
                    else
                    {
                        var itemList = item.ParentList;
                        if (itemList != null)
                        {
                            itemList.Remove(item);
                        }
                    }
                }

                item.TreeParentData = this._owner;

                //直接自动添加到其所在的列表中。
                if (needRebuildRelation)
                {
                    var previous = index > 0 ? this[index - 1] : this._owner;
                    list.Insert(list.IndexOf(previous) + 1, item);
                }
            }

            private void TryAutoCode()
            {
                if (this.IsAutoTreeCodeEnabled()) { this.ResetTreeCode(); }
            }

            private bool IsAutoTreeCodeEnabled()
            {
                if (!this.AutoTreeCodeEnabled.HasValue)
                {
                    var list = this._owner.ParentList;
                    if (list != null) { return list.AutoTreeCodeEnabled; }
                }

                return this.AutoTreeCodeEnabled.GetValueOrDefault(true);
            }

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