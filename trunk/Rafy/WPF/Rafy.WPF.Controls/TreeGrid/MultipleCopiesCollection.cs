/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121017 18:08
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121017 18:08
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows.Data;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 此类拷贝自 MS System.Windows.Controls.MultipleCopiesCollection
    /// 主要用于为 TreeGridCellsPanel 提供数据源。
    /// </summary>
    internal class MultipleCopiesCollection : IList, ICollection, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string c_CountName = "Count";
        private const string c_IndexerName = "Item[]";
        private object _item;
        private int _count;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        internal object CopiedItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._item; }
            set
            {
                if (value == CollectionView.NewItemPlaceholder)
                {
                    throw new NotImplementedException();//huqf
                    //value = DataGrid.NewItemPlaceholder;
                }
                if (this._item != value)
                {
                    object item = this._item;
                    this._item = value;
                    this.OnPropertyChanged(c_IndexerName);
                    for (int i = 0; i < this._count; i++)
                    {
                        this.OnReplace(item, this._item, i);
                    }
                }
            }
        }
        private int RepeatCount
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._count;
            }
            set
            {
                if (this._count != value)
                {
                    this._count = value;
                    this.OnPropertyChanged(c_CountName);
                    this.OnPropertyChanged(c_IndexerName);
                }
            }
        }
        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }
        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }
        public object this[int index]
        {
            get
            {
                if (index >= 0 && index < this.RepeatCount)
                {
                    return this._item;
                }
                throw new ArgumentOutOfRangeException("index");
            }
            set
            {
                throw new InvalidOperationException();
            }
        }
        public int Count
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.RepeatCount;
            }
        }
        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }
        public object SyncRoot
        {
            get
            {
                return this;
            }
        }
        internal MultipleCopiesCollection(object item, int count)
        {
            this.CopiedItem = item;
            this._count = count;
        }
        internal void MirrorCollectionChange(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.Insert(e.NewStartingIndex);
                    return;
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveAt(e.OldStartingIndex);
                    return;
                case NotifyCollectionChangedAction.Replace:
                    this.OnReplace(this.CopiedItem, this.CopiedItem, e.NewStartingIndex);
                    return;
                case NotifyCollectionChangedAction.Move:
                    this.Move(e.OldStartingIndex, e.NewStartingIndex);
                    return;
                case NotifyCollectionChangedAction.Reset:
                    this.Reset();
                    return;
                default:
                    return;
            }
        }
        internal void SyncToCount(int newCount)
        {
            int repeatCount = this.RepeatCount;
            if (newCount != repeatCount)
            {
                if (newCount > repeatCount)
                {
                    this.InsertRange(repeatCount, newCount - repeatCount);
                    return;
                }
                int num = repeatCount - newCount;
                this.RemoveRange(repeatCount - num, num);
            }
        }
        public int Add(object value)
        {
            throw new NotSupportedException("TreeGrid_ReadonlyCellsItemsSource");
        }
        public void Clear()
        {
            throw new NotSupportedException("TreeGrid_ReadonlyCellsItemsSource");
        }
        public bool Contains(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return this._item == value;
        }
        public int IndexOf(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (this._item != value)
            {
                return -1;
            }
            return 0;
        }
        public void Insert(int index, object value)
        {
            throw new NotSupportedException("TreeGrid_ReadonlyCellsItemsSource");
        }
        public void Remove(object value)
        {
            throw new NotSupportedException("TreeGrid_ReadonlyCellsItemsSource");
        }
        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException("TreeGrid_ReadonlyCellsItemsSource");
        }
        public void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }
        public IEnumerator GetEnumerator()
        {
            return new MultipleCopiesCollection.MultipleCopiesCollectionEnumerator(this);
        }
        private void Insert(int index)
        {
            this.RepeatCount++;
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, this.CopiedItem, index);
        }
        private void InsertRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                this.Insert(index);
                index++;
            }
        }
        private void Move(int oldIndex, int newIndex)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this.CopiedItem, newIndex, oldIndex));
        }
        private void RemoveAt(int index)
        {
            this.RepeatCount--;
            this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, this.CopiedItem, index);
        }
        private void RemoveRange(int index, int count)
        {
            for (int i = 0; i < count; i++)
            {
                this.RemoveAt(index);
            }
        }
        private void OnReplace(object oldItem, object newItem, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }
        private void Reset()
        {
            this.RepeatCount = 0;
            this.OnCollectionReset();
        }
        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }
        private void OnCollectionReset()
        {
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (this.CollectionChanged != null)
            {
                this.CollectionChanged(this, e);
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, e);
            }
        }

        private class MultipleCopiesCollectionEnumerator : IEnumerator
        {
            private object _item;
            private int _count;
            private int _current;
            private MultipleCopiesCollection _collection;
            object IEnumerator.Current
            {
                get
                {
                    if (this._current < 0)
                    {
                        return null;
                    }
                    if (this._current < this._count)
                    {
                        return this._item;
                    }
                    throw new InvalidOperationException();
                }
            }
            private bool IsCollectionUnchanged
            {
                get
                {
                    return this._collection.RepeatCount == this._count && this._collection.CopiedItem == this._item;
                }
            }
            public MultipleCopiesCollectionEnumerator(MultipleCopiesCollection collection)
            {
                this._collection = collection;
                this._item = this._collection.CopiedItem;
                this._count = this._collection.RepeatCount;
                this._current = -1;
            }
            bool IEnumerator.MoveNext()
            {
                if (!this.IsCollectionUnchanged)
                {
                    throw new InvalidOperationException();
                }
                int num = this._current + 1;
                if (num < this._count)
                {
                    this._current = num;
                    return true;
                }
                return false;
            }
            void IEnumerator.Reset()
            {
                if (this.IsCollectionUnchanged)
                {
                    this._current = -1;
                    return;
                }
                throw new InvalidOperationException();
            }
        }
    }
}
