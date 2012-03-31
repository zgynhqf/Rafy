using System;
using System.ComponentModel;
using SimpleCsla.Wpf;
using OEA.Library;
using OEA.Module.WPF;

namespace OEA.Module.WPF
{
    /// <summary>
    /// ObjectView的数据加载器
    /// </summary>
    internal class ViewDataLoader : IAsyncDataLoader
    {
        private OEADataProvider _dataProvider;

        private WPFObjectView _view;

        public ViewDataLoader(WPFObjectView view)
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

        #region DataProvider

        public OEADataProvider DataProvider
        {
            get { return this._dataProvider; }
        }

        /// <summary>
        /// 初始化数据提供组件。
        /// </summary>
        private void InitDataProvider()
        {
            //生成列表界面
            this._dataProvider = new OEADataProvider();
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

        ObjectView IAsyncDataLoader.View
        {
            get { return this._view; }
        }

        public void LoadDataAsync()
        {
            this.LoadDataAsync(null, null);
        }

        public void LoadDataAsync(Action changedCallback)
        {
            this.LoadDataAsync(null, changedCallback);
        }

        public void LoadDataAsync(Func<object> dataProvider)
        {
            this.LoadDataAsync(dataProvider, null);
        }

        public void LoadDataAsync(Func<object> dataProvider, Action changedCallback)
        {
            if (dataProvider == null)
            {
                dataProvider = () => RF.Create(this._view.EntityType).GetAll();
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

        public void ReloadData()
        {
            if (this._dataProvider.DataProducer == null)
            {
                this.LoadDataAsync();
            }
            else
            {
                this.LoadDataAsync(this._dataProvider.DataProducer, null);
            }
        }

        public void CancelLoading()
        {
            this._dataProvider.CancelAsync();
        }

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
                var asyncData = this._dataProvider.Data;

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
    }
}
