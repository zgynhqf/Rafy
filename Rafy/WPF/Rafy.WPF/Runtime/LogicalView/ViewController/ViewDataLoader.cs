using System;
using System.ComponentModel;

using Rafy.Domain;
using Rafy.WPF;

namespace Rafy.WPF
{
    /// <summary>
    /// LogicalView的数据加载器
    /// </summary>
    internal class ViewDataLoader : IAsyncDataLoader, IDisposable
    {
        private RafyDataSourceProvider _dataProvider;

        private LogicalView _view;

        public ViewDataLoader(LogicalView view)
        {
            this._view = view;

            this.InitDataProvider();
        }

        /// <summary>
        /// 表示当前是否正自在异步加载的状态
        /// </summary>
        public bool IsLoadingData
        {
            get { return this._dataProvider.IsBusy; }
        }

        /// <summary>
        /// 是否已经使用 DataLoader 查询过至少一次数据了。
        /// </summary>
        public bool AnyLoaded
        {
            get { return this._dataProvider.DataProducer != null; }
        }

        #region DataProvider

        public RafyDataSourceProvider DataProvider
        {
            get { return this._dataProvider; }
        }

        /// <summary>
        /// 初始化数据提供组件。
        /// </summary>
        private void InitDataProvider()
        {
            //生成列表界面
            this._dataProvider = new RafyDataSourceProvider();
            (this._dataProvider as ISupportInitialize).BeginInit();
            this._dataProvider.IsInitialLoadEnabled = false;
            this._dataProvider.DataChanged += new EventHandler(DataProvider_DataLoaded);
            (this._dataProvider as ISupportInitialize).EndInit();
        }

        #endregion

        #region 事件

        public event EventHandler<DataLoadedEventArgs> DataLoaded;

        /// <summary>
        /// 数据加载完成时，发生此事件。
        /// </summary>
        protected virtual void OnDataLoaded(DataLoadedEventArgs e)
        {
            var h = this.DataLoaded;
            if (h != null) { h(this, e); }
        }

        public event EventHandler DataChanged;

        /// <summary>
        /// 数据加载完成时，发生此事件。
        /// </summary>
        protected virtual void OnDataChanged(EventArgs e)
        {
            var h = this.DataChanged;
            if (h != null) { h(this, e); }
        }

        #endregion

        #region 数据加载接口

        /// <summary>
        /// 异步加载该视图对应实体类型的所有实体对象
        /// </summary>
        public void LoadDataAsync()
        {
            this.LoadDataAsync(null, null);
        }

        /// <summary>
        /// 异步加载该视图对应实体类型的所有实体对象
        /// </summary>
        /// <param name="changedCallback">数据加载返回后的回调函数</param>
        public void LoadDataAsync(Action changedCallback)
        {
            this.LoadDataAsync(null, changedCallback);
        }

        /// <summary>
        /// 通过指定的数据获取方法，异步加载该视图对应实体类型的指定实体对象
        /// </summary>
        /// <param name="dataProvider">自定义数据提供程序。可以返回 Entity，也可以返回 EntityList。</param>
        public void LoadDataAsync(Func<IDomainComponent> dataProvider)
        {
            this.LoadDataAsync(dataProvider, null);
        }

        /// <summary>
        /// 通过指定的数据获取方法，异步加载该视图对应实体类型的指定实体对象
        /// </summary>
        /// <param name="dataProvider">自定义数据提供程序。可以返回 Entity，也可以返回 EntityList。</param>
        /// <param name="changedCallback">数据加载返回后的回调函数</param>
        public void LoadDataAsync(Func<IDomainComponent> dataProvider, Action changedCallback)
        {
            if (dataProvider == null)
            {
                dataProvider = () => RF.Find(this._view.EntityType).CacheAll();
            }

            using (this._dataProvider.DeferRefresh())
            {
                this._dataProvider.DataProducer = dataProvider;
            }

            if (changedCallback != null)
            {
                this.ListenDataChangedOnce(changedCallback);
            }
        }

        /// <summary>
        /// 使用上一次使用过的数据提供程序重新加载数据。
        /// 
        /// 如果还没有进行过任何加载，则抛出异常。
        /// </summary>
        public void ReloadDataAsync()
        {
            this.ReloadDataAsync(null);
        }

        /// <summary>
        /// 使用上一次使用过的数据提供程序重新加载数据。
        /// 
        /// 如果还没有进行过任何加载，则抛出异常。
        /// </summary>
        /// <param name="changedCallback">数据加载返回后的回调函数</param>
        public void ReloadDataAsync(Action changedCallback)
        {
            if (!this.AnyLoaded)
            {
                throw new InvalidProgramException("还没有进行过任何一次加载，不能使用 ReloadDataAsync 方法，请使用 AnyLoaded 属性检测。");
            }

            this.LoadDataAsync(this._dataProvider.DataProducer as Func<IDomainComponent>, changedCallback);
        }

        /// <summary>
        /// 如果当前正在数据加载中，则可以使用本方法来取消数据加载过程。
        /// </summary>
        public void CancelLoading()
        {
            this._dataProvider.CancelAsync();
        }

        #endregion

        /// <summary>
        /// 重新绑定View的数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataProvider_DataLoaded(object sender, EventArgs e)
        {
            if (this._dataProvider.Error == null)
            {
                //重新绑定数据
                var asyncData = this._dataProvider.Data as IDomainComponent;

                var args = new DataLoadedEventArgs(asyncData);

                //这个事件的处理者，可能会对这个数据进行一些更改。
                this.OnDataLoaded(args);

                var oldData = this._view.Data;
                if (oldData != args.Data)
                {
                    this._view.Data = args.Data;
                }
                else
                {
                    //由于经过OnDataLoaded的处理，所有上一步可能是同一对象，
                    //这样的话，设置View的Data就会没有任何效果，这时需要加上以下这步。
                    this._view.RefreshCurrentEntity();
                }

                this.OnDataChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// 内存泄漏，尽量断开连接。
        /// </summary>
        public void Dispose()
        {
            if (_dataProvider != null)
            {
                _dataProvider.Dispose();
                _dataProvider = null;
            }
        }


        LogicalView IAsyncDataLoader.View
        {
            get { return this._view; }
        }
    }
}
