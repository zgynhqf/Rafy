///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20100720
// * 说明：封装DataGrid的ICollectionView的生成
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100720
// * 
//*******************************************************/

//using System;
//using System.Collections;
//using System.ComponentModel;
//using System.Diagnostics;
//using System.Linq;
//using System.Windows.Data;
//

//namespace OEA.Module.WPF
//{
//    /// <summary>
//    /// SortableList 存在多次注册事件的问题。
//    /// 
//    /// 此类用于给指定的IBindingList作可排序的视图使用。
//    /// 
//    /// 实现方式：
//    /// 组合了entityList 和 viewList
//    /// 
//    /// 复用SortedBindingList(T)的排序功能，用它作为缓存的视图模型。
//    /// 本SortableList对象的所有视图属性都直接代理到这个_viewList字段上。
//    /// （无法使用继承方式，因为SortedBindingList的构造函数只支持泛型列表，而这里无法把entityList直接转换为泛型。）
//    /// 
//    /// 所有数据的操作都代理到_models这个实体列表字段上，并同步刷新视图的缓存列表_viewList。
//    /// </summary>
//    internal class SortableList : IBindingList
//    {
//        public static ICollectionView CreateViewForEntityList(IList entityList, bool supportGrouping)
//        {
//            return CreateViewForEntityList(entityList, null, supportGrouping);
//        }

//        public static ICollectionView CreateViewForEntityList(IList entityList, Predicate<object> filter, bool supportGrouping)
//        {
//            //Create a cached BindingList
//            if (entityList is IBindingList)
//            {
//                if (!supportGrouping)
//                {
//                    var viewList = new SortableList(entityList as IBindingList, filter);
//                    return new BindingListCollectionView(viewList);
//                }
//                if (filter != null)
//                {
//                    var filterView = new ListCollectionView(entityList as IList);
//                    filterView.Filter = filter;
//                    return filterView;
//                }
//                //如果需要支持分组，则不再支持排序，同时也不支持过滤。
//                return new BindingListCollectionView(entityList as IBindingList);
//            }

//            var view = new ListCollectionView(entityList as IList);
//            if (filter != null)
//            {
//                view.Filter = filter;
//            }
//            return view;
//        }

//        private IBindingList _models;

//        /// <summary>
//        /// 用于排序的缓存的实体列表
//        /// </summary>
//        private SortedBindingList<object> _viewList;

//        private object[] _cacheOldModels;

//        private SortableList(IBindingList list) : this(list, null) { }

//        private SortableList(IBindingList list, Predicate<object> filter)
//        {
//            Debug.Assert(list != null, "list != null");

//            this._models = list;

//            this.Filter = filter;

//            this.CreateNewCache();

//            this._models.ListChanged += new ListChangedEventHandler(_models_ListChanged);
//        }

//        public Predicate<object> Filter { get; set; }

//        private void CreateNewCache()
//        {
//            _cacheOldModels = this._models.OfType<object>().ToArray();
//            if (this.Filter != null)
//            {
//                var cacheModels = _cacheOldModels.Where(o => this.Filter(o));
//                this._viewList = new SortedBindingList<object>(cacheModels.ToArray());
//            }
//            else
//            {
//                this._viewList = new SortedBindingList<object>(_cacheOldModels.ToArray());
//            }
//        }

//        /// <summary>
//        /// 把实体的更改同步到缓存上
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void _models_ListChanged(object sender, ListChangedEventArgs e)
//        {
//            //以下更新不作额外处理，直接以Reset返回。
//            //以下两种模式可能会因为viewModelIndex和modelIndex不同而引起BUG。
//            switch (e.ListChangedType)
//            {
//                case ListChangedType.PropertyDescriptorAdded:
//                case ListChangedType.PropertyDescriptorChanged:
//                case ListChangedType.PropertyDescriptorDeleted:
//                    this.OnListChanged(e);
//                    return;
//                case ListChangedType.Reset:
//                    if (this._models.Count == 0)
//                    {
//                        this.CreateNewCache();
//                    }
//                    this.OnListChanged(e);
//                    return;
//                case ListChangedType.ItemMoved:
//                    var newViewIndex = SyncIndex(e.NewIndex);
//                    var oldViewIndex = SyncIndex(e.OldIndex);
//                    if (newViewIndex != -1 && oldViewIndex != -1)
//                    {
//                        var newArgs = new ListChangedEventArgs(e.ListChangedType, newViewIndex, oldViewIndex);
//                        this.OnListChanged(newArgs);
//                    }
//                    return;
//                case ListChangedType.ItemChanged:
//                    if (e.PropertyDescriptor == null)
//                    {
//                        var viewIndex = SyncIndex(e.NewIndex);
//                        if (viewIndex != -1)
//                        {
//                            var newArgs = new ListChangedEventArgs(ListChangedType.ItemChanged, viewIndex);
//                            this.OnListChanged(newArgs);
//                        }
//                    }
//                    else
//                    {
//                        //暂时不做任何事
//                    }
//                    return;
//            }

//            //如果数据被排序了，或者被过滤了，则需要进行同步。
//            var needSync = this._viewList.IsSorted ||
//                this._cacheOldModels.Length != this._viewList.Count;

//            ListChangedEventArgs args = e;

//            //更新视图模型
//            if (needSync && e.ListChangedType == ListChangedType.ItemDeleted)
//            {
//                var model = this._cacheOldModels[e.NewIndex];
//                var viewModelIndex = this._viewList.IndexOf(model);
//                args = new ListChangedEventArgs(ListChangedType.ItemDeleted, viewModelIndex);
//            }

//            this.CreateNewCache();

//            //设置视图事件参数的Index
//            if (needSync && e.ListChangedType == ListChangedType.ItemAdded)
//            {
//                var model = this._models[e.NewIndex];
//                var viewModelIndex = this._viewList.IndexOf(model);
//                args = new ListChangedEventArgs(ListChangedType.ItemAdded, viewModelIndex);
//            }

//            ////由于要更新视图模型，这里需要把已经排序的规则保存下来。
//            //var oldDirection = this._viewList.SortDirection;
//            //var oldProperty = this._viewList.SortProperty;

//            ////更新视图模型
//            //if (needSync && e.ListChangedType == ListChangedType.ItemDeleted)
//            //{
//            //    //移除排序并根据index找到该元素
//            //    this._viewList.RemoveSort();
//            //    var model = this._viewList[e.NewIndex];
//            //    //找到该元素在排序后的位置。
//            //    this._viewList.ApplySort(oldProperty, oldDirection);
//            //    var viewModelIndex = this._viewList.IndexOf(model);
//            //    args = new ListChangedEventArgs(ListChangedType.ItemDeleted, viewModelIndex);
//            //}

//            //this.CreateNewCache();

//            //if (needSync)
//            //{
//            //    this._viewList.ApplySort(oldProperty, oldDirection);

//            //    //设置视图事件参数的Index
//            //    if (e.ListChangedType == ListChangedType.ItemAdded)
//            //    {
//            //        var model = this._models[e.NewIndex];
//            //        var viewModelIndex = this._viewList.IndexOf(model);
//            //        args = new ListChangedEventArgs(ListChangedType.ItemAdded, viewModelIndex);
//            //    }
//            //}

//            ////更新视图模型
//            //switch (e.ListChangedType)
//            //{
//            //    case ListChangedType.ItemAdded:
//            //        var model = this._models[e.NewIndex];
//            //        this._viewList.Insert(e.NewIndex, model);
//            //        break;
//            //    case ListChangedType.ItemDeleted:
//            //        this._viewList.RemoveAt(e.NewIndex);
//            //        break;
//            //}

//            this.OnListChanged(args);
//        }

//        private int SyncIndex(int oldIndex)
//        {
//            var model = this._cacheOldModels[oldIndex];
//            if (this._viewList.Count <= oldIndex)
//            {
//                return this._viewList.IndexOf(model);
//            }
//            else
//            {
//                if (this._viewList[oldIndex] != model)
//                {
//                    return this._viewList.IndexOf(model);
//                }
//            }
//            return oldIndex;
//        }

//        private void OnListChanged(ListChangedEventArgs e)
//        {
//            if (this.ListChanged != null)
//            {
//                this.ListChanged(this, e);
//            }
//        }

//        #region IBindingList

//        public void AddIndex(PropertyDescriptor property)
//        {
//            this._viewList.AddIndex(property);
//        }

//        public object AddNew()
//        {
//            return this._models.AddNew();
//        }

//        public bool AllowEdit
//        {
//            get
//            {
//                return this._models.AllowEdit;
//            }
//        }

//        public bool AllowNew
//        {
//            get
//            {
//                return this._models.AllowNew;
//            }
//        }

//        public bool AllowRemove
//        {
//            get
//            {
//                return this._models.AllowRemove;
//            }
//        }

//        public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
//        {
//            this._viewList.ApplySort(property, direction);
//        }

//        public int Find(PropertyDescriptor property, object key)
//        {
//            return this._viewList.Find(property, key);
//        }

//        public bool IsSorted
//        {
//            get
//            {
//                return this._viewList.IsSorted;
//            }
//        }

//        public event ListChangedEventHandler ListChanged;

//        public void RemoveIndex(PropertyDescriptor property)
//        {
//            this._viewList.RemoveIndex(property);
//        }

//        public void RemoveSort()
//        {
//            this._viewList.RemoveSort();
//        }

//        public ListSortDirection SortDirection
//        {
//            get
//            {
//                return this._viewList.SortDirection;
//            }
//        }

//        public PropertyDescriptor SortProperty
//        {
//            get
//            {
//                return this._viewList.SortProperty;
//            }
//        }

//        public bool SupportsChangeNotification
//        {
//            get
//            {
//                return this._models.SupportsChangeNotification;
//            }
//        }

//        public bool SupportsSearching
//        {
//            get
//            {
//                return this._viewList.SupportsSearching;
//            }
//        }

//        public bool SupportsSorting
//        {
//            get
//            {
//                return this._viewList.SupportsSorting;
//            }
//        }

//        public int Add(object value)
//        {
//            return this._models.Add(value);
//        }

//        public void Clear()
//        {
//            this._models.Clear();
//        }

//        public bool Contains(object value)
//        {
//            return this._viewList.Contains(value);
//        }

//        public int IndexOf(object value)
//        {
//            return this._viewList.IndexOf(value);
//        }

//        public void Insert(int index, object value)
//        {
//            throw new NotImplementedException();
//        }

//        public bool IsFixedSize
//        {
//            get
//            {
//                return this._models.IsFixedSize;
//            }
//        }

//        public bool IsReadOnly
//        {
//            get
//            {
//                return this._models.IsReadOnly;
//            }
//        }

//        public void Remove(object value)
//        {
//            this._models.Remove(value);
//        }

//        public void RemoveAt(int index)
//        {
//            //这里没有任何操作，可能会引起BUG。
//            //throw new NotImplementedException();
//            //this._models.Remove(value);
//        }

//        public object this[int index]
//        {
//            get
//            {
//                return this._viewList[index];
//            }
//            set
//            {
//                this._viewList[index] = value;
//            }
//        }

//        public void CopyTo(Array array, int index)
//        {
//            this._viewList.ToArray().CopyTo(array, index);
//        }

//        public int Count
//        {
//            get
//            {
//                return this._viewList.Count;
//            }
//        }

//        public bool IsSynchronized
//        {
//            get
//            {
//                return this._models.IsSynchronized;
//            }
//        }

//        public object SyncRoot
//        {
//            get
//            {
//                return this._models.SyncRoot;
//            }
//        }

//        public IEnumerator GetEnumerator()
//        {
//            return this._viewList.GetEnumerator();
//        }

//        #endregion
//    }
//}
