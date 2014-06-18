/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Runtime;
using System.Windows;
using System.ComponentModel;

namespace Rafy.WPF.Controls
{
    [Serializable]
    public class TreeGridColumnCollection : ObservableCollection<TreeGridColumn>
    {
        [NonSerialized]
        private DependencyObject _owner;

        /// <summary>
        /// 一般情况下，此属性表示当前所在的 TreeGrid。
        /// </summary>
        internal DependencyObject Owner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._owner; }
            set
            {
                if (this._owner != value)
                {
                    this.HookContext(this._owner, value);

                    this._owner = value;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.OnInternalCollectionChanged();

            base.OnCollectionChanged(e);

            this.ResetVisibility(e);
        }

        #region UnvisibleColumns 可见性

        private List<TreeGridColumn> _unvisibleColumns = new List<TreeGridColumn>();

        /// <summary>
        /// 所有被设置为不可见的列。
        /// </summary>
        public IList<TreeGridColumn> UnvisibleColumns
        {
            get { return this._unvisibleColumns.AsReadOnly(); }
        }

        private void ResetVisibility(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add || e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (TreeGridColumn column in e.NewItems)
                {
                    column.TreeGrid = this._owner as TreeGrid;

                    column._ownerCollection = this;

                    //如果该列不可见，就在设置完 _ownerCollection 后删除它。
                    if (!column.IsVisible)
                    {
                        if (!this._unvisibleColumns.Contains(column))
                        {
                            this.SetVisibility(column, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 设置某列在整个集合中的可见性。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="isVisible"></param>
        internal void SetVisibility(TreeGridColumn column, bool isVisible)
        {
            if (isVisible)
            {
                if (column._oldIndexAsUnvisible >= 0 && column._oldIndexAsUnvisible < this.Count)
                {
                    this.Insert(column._oldIndexAsUnvisible, column);
                }
                else
                {
                    this.Add(column);
                }

                //最后再移除，OnCollectionChanged 事件中会使用此变量防止重入
                this._unvisibleColumns.Remove(column);
            }
            else
            {
                column._oldIndexAsUnvisible = this.IndexOf(column);

                this.RemoveAt(column._oldIndexAsUnvisible);

                this._unvisibleColumns.Add(column);
            }
        }

        #endregion

        #region StableColumns

        /*********************** 代码块解释 *********************************
         * 
         * “可能是为了更好地支持列排序功能”，虽然 TreeGridColumnCollection 本身是一个集合，
         * 但是这里并不使用本集合中的项（this[i]），而是单独声明了两个列表 _stableColumns 及 _stableIndices。
         * 
         * _stableColumns 用于存储所有的列，这样，就可以保留本对象公共接口操作前的所有数据，方便查找。
         * 
         * _stableIndices 则存储 this 中每一项对应在 _stableColumns 中的索引。
         * 例如，当 this 中的列对应 c1,c3,c2，而 _columns 中存储的是 c1,c2,c3 时，则 _stableIndices 中存储的是：0,2,1，
         * 即 _stableIndices[index in this] = "index in _stableColumns"。
         * 
        **********************************************************************/

        /// <summary>
        /// 位置不会变化的列的缓存列表
        /// </summary>
        private List<TreeGridColumn> _stableColumns = new List<TreeGridColumn>();

        /// <summary>
        /// _stableIndices[index in this] = "index in _stableColumns"
        /// </summary>
        private List<int> _stableIndices = new List<int>();

        internal List<TreeGridColumn> StableColumns
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._stableColumns; }
        }

        protected override void ClearItems()
        {
            this.VerifyAccess();
            this._internalEventArg = this.ClearPreprocess();
            base.ClearItems();
        }

        protected override void InsertItem(int index, TreeGridColumn column)
        {
            this.VerifyAccess();
            this._internalEventArg = this.InsertPreprocess(index, column);
            base.InsertItem(index, column);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            if (oldIndex != newIndex)
            {
                this.VerifyAccess();
                this._internalEventArg = this.MovePreprocess(oldIndex, newIndex);
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void RemoveItem(int index)
        {
            this.VerifyAccess();
            this._internalEventArg = this.RemoveAtPreprocess(index);
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, TreeGridColumn column)
        {
            this.VerifyAccess();
            this._internalEventArg = this.SetPreprocess(index, column);
            if (this._internalEventArg != null)
            {
                base.SetItem(index, column);
            }
        }

        private TreeGridColumnCollectionChangedEventArgs ClearPreprocess()
        {
            var array = new TreeGridColumn[base.Count];
            if (base.Count > 0)
            {
                base.CopyTo(array, 0);
            }

            foreach (var column in this._stableColumns)
            {
                column.ResetPrivateData();
                column.PropertyChanged -= this.ColumnPropertyChanged;
                InheritanceContextHelper.RemoveContextFromObject(this._owner, column);
            }

            this._stableColumns.Clear();
            this._stableIndices.Clear();

            return new TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, array);
        }

        private TreeGridColumnCollectionChangedEventArgs InsertPreprocess(int index, TreeGridColumn column)
        {
            int count = this._stableColumns.Count;
            if (index < 0 || index > count) { throw new ArgumentOutOfRangeException("index"); }
            if (column == null) { throw new ArgumentNullException("column"); }
            if (column.StableIndex >= 0) { throw new InvalidOperationException("NotAllowShareColumnToTwoColumnCollection"); }

            column.StableIndex = count;
            this._stableColumns.Add(column);
            this._stableIndices.Insert(index, column.StableIndex);

            InheritanceContextHelper.ProvideContextForObject(this._owner, column);

            column.PropertyChanged += this.ColumnPropertyChanged;

            return new TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, column, index, count);
        }

        private TreeGridColumnCollectionChangedEventArgs MovePreprocess(int oldIndex, int newIndex)
        {
            this.VerifyIndexInRange(oldIndex, "oldIndex");
            this.VerifyIndexInRange(newIndex, "newIndex");

            int actualIndex = this._stableIndices[oldIndex];
            if (oldIndex < newIndex)
            {
                for (int i = oldIndex; i < newIndex; i++)
                {
                    this._stableIndices[i] = this._stableIndices[i + 1];
                }
            }
            else
            {
                for (int j = oldIndex; j > newIndex; j--)
                {
                    this._stableIndices[j] = this._stableIndices[j - 1];
                }
            }
            this._stableIndices[newIndex] = actualIndex;

            return new TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this._stableColumns[actualIndex], newIndex, oldIndex, actualIndex);
        }

        private TreeGridColumnCollectionChangedEventArgs RemoveAtPreprocess(int index)
        {
            this.VerifyIndexInRange(index, "index");

            int actualIndex = this._stableIndices[index];
            var column = this._stableColumns[actualIndex];

            column.ResetPrivateData();
            column.PropertyChanged -= this.ColumnPropertyChanged;

            this._stableColumns.RemoveAt(actualIndex);

            this.UpdateIndicesOnRemoved(actualIndex, index);

            InheritanceContextHelper.RemoveContextFromObject(this._owner, column);

            return new TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, column, index, actualIndex);
        }

        private TreeGridColumnCollectionChangedEventArgs SetPreprocess(int index, TreeGridColumn newColumn)
        {
            this.VerifyIndexInRange(index, "index");

            var column = base[index];
            if (column != newColumn)
            {
                int actualIndex = this._stableIndices[index];

                this.RemoveAtPreprocess(index);

                this.InsertPreprocess(index, newColumn);

                return new TreeGridColumnCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newColumn, column, index, actualIndex);
            }

            return null;
        }

        private void UpdateIndicesOnRemoved(int actualIndex, int removeIndex)
        {
            for (int i = 0; i < removeIndex; i++)
            {
                int ai = this._stableIndices[i];
                if (ai > actualIndex)
                {
                    this._stableIndices[i] = ai - 1;
                }
            }

            for (int j = removeIndex + 1; j < this._stableIndices.Count; j++)
            {
                int ai = this._stableIndices[j];
                if (ai < actualIndex)
                {
                    this._stableIndices[j - 1] = ai;
                }
                else
                {
                    if (ai > actualIndex)
                    {
                        this._stableIndices[j - 1] = ai - 1;
                    }
                }
            }

            this._stableIndices.RemoveAt(this._stableIndices.Count - 1);

            //更新 ActualIndex
            for (int i = actualIndex; i < this._stableColumns.Count; i++)
            {
                this._stableColumns[i].StableIndex = i;
            }
        }

        private void VerifyIndexInRange(int index, string indexName)
        {
            if (index < 0 || index >= this._stableIndices.Count)
            {
                throw new ArgumentOutOfRangeException(indexName);
            }
        }

        private void HookContext(DependencyObject oldOwner, DependencyObject newOwner)
        {
            if (newOwner != null)
            {
                foreach (var column in this._stableColumns)
                {
                    InheritanceContextHelper.ProvideContextForObject(newOwner, column);
                }
            }
            else
            {
                foreach (var column in this._stableColumns)
                {
                    InheritanceContextHelper.RemoveContextFromObject(oldOwner, column);
                }
            }
        }

        #endregion

        #region InternalCollectionChanged

        [NonSerialized]
        private TreeGridColumnCollectionChangedEventArgs _internalEventArg;

        internal event NotifyCollectionChangedEventHandler InternalCollectionChanged;

        private void ColumnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var gridViewColumn = sender as TreeGridColumn;
            if (this.InternalCollectionChanged != null && gridViewColumn != null)
            {
                this.InternalCollectionChanged(this, new TreeGridColumnCollectionChangedEventArgs(gridViewColumn, e.PropertyName));
            }
        }

        private void OnInternalCollectionChanged()
        {
            if (this.InternalCollectionChanged != null && this._internalEventArg != null)
            {
                this.InternalCollectionChanged(this, this._internalEventArg);
                this._internalEventArg = null;
            }
        }

        #endregion

        #region IsImmutable

        private bool _isImmutable;

        internal void BlockWrite()
        {
            this._isImmutable = true;
        }

        internal void UnblockWrite()
        {
            this._isImmutable = false;
        }

        private void VerifyAccess()
        {
            if (this._isImmutable)
            {
                throw new InvalidOperationException("TreeGridColumnCollectionIsReadOnly");
            }
            base.CheckReentrancy();
        }

        #endregion

        #region For Automation

        //internal List<int> ActualIndices
        //{
        //    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        //    get { return this._actualIndices; }
        //}

        #endregion
    }
}