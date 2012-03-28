using System;
using System.ComponentModel;
using SimpleCsla.Wpf;
using OEA.Library;
using OEA.Module.WPF;

namespace OEA
{
    /// <summary>
    /// ObjectView的数据加载器
    /// </summary>
    public abstract class ViewDataLoaderBase : IViewDataLoader
    {
        private OEADataProvider _dataProvider;

        private WPFObjectView _view;

        private object[] _lastqueryparam = new object[0];

        public ViewDataLoaderBase(WPFObjectView view)
        {
            this._view = view;

            this.InitDataProvider();
        }

        public WPFObjectView View
        {
            get { return this._view; }
        }

        #region DataProvider

        /// <summary>
        /// 对应BOType的列表类型
        /// </summary>
        protected abstract Type GetQueryType();

        protected abstract string FactoryMethod { get; }

        public CslaDataProvider DataProvider
        {
            get { return this._dataProvider; }
        }

        /// <summary>
        /// 初始化组件。
        /// </summary>
        private void InitDataProvider()
        {
            //生成列表界面
            this._dataProvider = new OEADataProvider();
            (this._dataProvider as ISupportInitialize).BeginInit();
            this._dataProvider.IsAsynchronous = true;
            this._dataProvider.IsInitialLoadEnabled = false;
            this._dataProvider.DataChanged += new EventHandler(DataProvider_DataLoaded);
            (this._dataProvider as ISupportInitialize).EndInit();
        }

        #endregion

        #region IViewDataLoader Methods

        ObjectView IViewDataLoader.View
        {
            get { return this._view; }
        }

        public void GetObjectAsync(params object[] getListParam)
        {
            this.GetObjectAsync(FactoryMethod, getListParam);
            this._lastqueryparam = getListParam;
        }

        public virtual void GetObjectAsync(string getListMethod, params object[] getListParam)
        {
            if (string.IsNullOrWhiteSpace(getListMethod)) throw new ArgumentNullException("factoryMethod");

            using (this._dataProvider.DeferRefresh())
            {
                this._dataProvider.ObjectType = this.GetQueryType();
                this._dataProvider.FactoryMethod = getListMethod;
                var pList = this._dataProvider.FactoryParameters;
                pList.Clear();
                for (int i = 0, c = getListParam.Length; i < c; i++)
                {
                    var p = getListParam[i];
                    pList.Add(p);
                }
            }
        }

        public virtual void GetObjectAsync(Func<object> dataProvider)
        {
            using (this._dataProvider.DeferRefresh())
            {
                this._dataProvider.DataProducer = dataProvider;
            }
        }

        public void GetObject(params object[] getListParam)
        {
            this.GetObject(this.FactoryMethod, getListParam);
            this._lastqueryparam = getListParam;
        }

        public virtual void GetObject(string getListMethod, params object[] getListParam)
        {
            if (string.IsNullOrWhiteSpace(getListMethod)) throw new ArgumentNullException("factoryMethod");

            ////设置属性
            //this._dataProvider.ObjectType = this.GetQueryType();
            //this._dataProvider.FactoryMethod = getListMethod;
            //var pList = this._dataProvider.FactoryParameters;
            //pList.Clear();
            //foreach (var p in getListParam) pList.Add(p);

            //this._dataProvider.Refresh();

            this._dataProvider.IsAsynchronous = false;

            using (this._dataProvider.DeferRefresh())
            {
                this._dataProvider.ObjectType = this.GetQueryType();
                this._dataProvider.FactoryMethod = getListMethod;
                var pList = this._dataProvider.FactoryParameters;
                pList.Clear();
                foreach (var p in getListParam) pList.Add(p);
            }

            this._dataProvider.IsAsynchronous = true;
        }

        #endregion

        #region Loading Data

        /// <summary>
        /// 表示当前是否正自在异步加载的状态
        /// </summary>
        public bool IsLoadingData
        {
            get
            {
                return this._dataProvider.IsBusy;
            }
        }

        public object[] LastQueryParam
        {
            get
            {
                return this._lastqueryparam;
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

        public event EventHandler<DataLoadedEventArgs> DataLoaded;

        /// <summary>
        /// 数据加载完成时，发生此事件。
        /// </summary>
        protected virtual void OnDataLoaded(DataLoadedEventArgs e)
        {
            if (this.DataLoaded != null)
            {
                this.DataLoaded(this, e);
            }
        }

        public event EventHandler DataChanged;

        /// <summary>
        /// 数据加载完成时，发生此事件。
        /// </summary>
        protected virtual void OnDataChanged(EventArgs e)
        {
            if (this.DataChanged != null)
            {
                this.DataChanged(this, e);
            }
        }

        #endregion
    }
}
