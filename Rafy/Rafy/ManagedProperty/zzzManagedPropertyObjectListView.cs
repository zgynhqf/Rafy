//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.ComponentModel;

//namespace Rafy.ManagedProperty
//{
//    public class ManagedPropertyObjectListView : BindingList<ManagedPropertyLogicalView>
//    {
//        /// <summary>
//        /// 缓存变化前的列表，这样才可以通过这个集合来使用变化前的索引。
//        /// </summary>
//        private ManagedPropertyObject[] _cacheOldList;

//        public ManagedPropertyObjectListView(IBindingList coreList)
//        {
//            coreList.ListChanged += OnCoreList_ListChanged;

//            this.ResetCache(coreList);
//        }

//        private void OnCoreList_ListChanged(object sender, ListChangedEventArgs e)
//        {
//            var listChangedType = e.ListChangedType;

//            switch (listChangedType)
//            {
//                case ListChangedType.PropertyDescriptorAdded:
//                case ListChangedType.PropertyDescriptorChanged:
//                case ListChangedType.PropertyDescriptorDeleted:
//                case ListChangedType.Reset:
//                    this.OnListChanged(e);
//                    return;
//                case ListChangedType.ItemMoved:
//                    var newViewIndex = this.SyncIndex(e.NewIndex);
//                    var oldViewIndex = this.SyncIndex(e.OldIndex);
//                    if (newViewIndex != -1 && oldViewIndex != -1)
//                    {
//                        var newArgs = new ListChangedEventArgs(ListChangedType.ItemMoved, newViewIndex, oldViewIndex);
//                        this.OnListChanged(newArgs);
//                    }
//                    return;
//                case ListChangedType.ItemChanged:
//                    var viewIndex = this.SyncIndex(e.NewIndex);
//                    if (viewIndex != -1)
//                    {
//                        var newArgs = new ListChangedEventArgs(ListChangedType.ItemChanged, viewIndex, e.PropertyDescriptor);
//                        this.OnListChanged(newArgs);
//                    }
//                    return;
//            }

//            var args = e;

//            //更新视图模型
//            if (listChangedType == ListChangedType.ItemDeleted)
//            {
//                var model = this._cacheOldList[e.NewIndex];
//                var viewModelIndex = this.IndexOf(model);
//                args = new ListChangedEventArgs(ListChangedType.ItemDeleted, viewModelIndex);
//            }

//            var newList = sender as IBindingList;
//            this.ResetCache(newList);

//            //设置视图事件参数的Index
//            if (listChangedType == ListChangedType.ItemAdded)
//            {
//                var model = newList[e.NewIndex] as ManagedPropertyObject;
//                var viewModelIndex = this.IndexOf(model);
//                args = new ListChangedEventArgs(ListChangedType.ItemAdded, viewModelIndex);
//            }

//            this.OnListChanged(args);
//        }

//        private void ResetCache(IBindingList coreList)
//        {
//            this._cacheOldList = coreList.Cast<ManagedPropertyObject>().ToArray();

//            this.RaiseListChangedEvents = false;

//            this.ClearItems();

//            foreach (var item in this._cacheOldList) { this.Add(new ManagedPropertyLogicalView(item)); }

//            this.RaiseListChangedEvents = true;
//        }

//        /// <summary>
//        /// 返回对应旧的索引的新索引。
//        /// 旧索引：列表变化前的索引。
//        /// 新索引：列表变化后的索引。
//        /// </summary>
//        /// <param name="oldIndex"></param>
//        /// <returns></returns>
//        private int SyncIndex(int oldIndex)
//        {
//            var model = this._cacheOldList[oldIndex];

//            if (this.Count <= oldIndex || this[oldIndex].CoreObject != model)
//            {
//                return this.IndexOf(model);
//            }

//            return oldIndex;
//        }

//        private int IndexOf(ManagedPropertyObject model)
//        {
//            for (int i = 0, c = this.Count; i < c; i++)
//            {
//                var itemView = this[i];
//                if (itemView.CoreObject == model) return i;
//            }

//            return -1;
//        }
//    }
//}