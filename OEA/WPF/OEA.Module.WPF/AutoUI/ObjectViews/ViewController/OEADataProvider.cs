/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110713
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110713
 * 不再从 CslaDataProvider 类继承。 胡庆访 20120329
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.ComponentModel;
using SimpleCsla.Wpf;
using System.Windows.Threading;
using OEA.Module.WPF;
using System.Windows;
using OEA.Threading;

namespace OEA
{
    /// <summary>
    /// 在 CslaDataProvider 的基础上增加了一个更加通用的数据获取方案器。
    /// </summary>
    public class OEADataProvider : DataSourceProvider
    {
        private bool _firstRun = true;
        private bool _endInitCompete = false;
        private bool _endInitError = false;
        private bool _isBusy = false;

        /// <summary>
        /// 查询的版本号。
        /// 
        /// 每取消一次，递增一。
        /// </summary>
        private int _queryVersion;

        /// <summary>
        /// 数据获取器
        /// </summary>
        public Func<object> DataProducer { get; set; }

        /// <summary>
        /// Gets a value indicating if this object is busy.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set
            {
                _isBusy = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsBusy"));
            }
        }

        #region Init

        private bool _init = true;

        /// <summary>
        /// Indicates that the control is about to initialize.
        /// </summary>
        protected override void BeginInit()
        {
            _init = true;

            base.BeginInit();
        }

        /// <summary>
        /// Indicates that the control has initialized.
        /// </summary>
        protected override void EndInit()
        {
            _init = false;

            base.EndInit();

            _endInitCompete = true;
        }

        #endregion

        /// <summary>
        /// Overridden. Starts to create the requested object, 
        /// either immediately or on a background thread, 
        /// based on the value of the IsAsynchronous property.
        /// </summary>
        protected override void BeginQuery()
        {
            if (this._init) return;

            if (this._firstRun)
            {
                this._firstRun = false;
                if (!IsInitialLoadEnabled) return;
            }

            if (this._endInitError)
            {
                // this handles a case where the WPF form initilizer
                // invokes the data provider twice when an exception
                // occurs - we really don't want to try the query twice
                // or report the error twice

                this._endInitError = false;
                this.OnQueryFinished(null, null, null, null);
                return;
            }

            if (this.IsRefreshDeferred) return;

            var request = new QueryRequest()
            {
                DataProducer = this.DataProducer,
                Version = this._queryVersion
            };

            this.IsBusy = true;

            ThreadHelper.AsyncInvoke(() => DoQuery(request));
        }

        private void DoQuery(QueryRequest request)
        {
            object data = null;
            Exception exception = null;

            try
            {
                data = request.DataProducer();
            }
            catch (Exception e)
            {
                exception = e;
            }

            //if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            //  System.Windows.Application.Current.Dispatcher.Invoke(
            //    new Action(() => { IsBusy = false; }), 
            //    new object[] { });

            if (!this._endInitCompete && exception != null) this._endInitError = true;

            //如果没有取消，则继续通知更改。
            if (request.Version == this._queryVersion)
            {
                //把结果返回到基类中。
                this.OnQueryFinished(data, exception, o => { this.IsBusy = false; return null; }, null);
            }
        }

        /// <summary>
        /// 在CslaDataProvider的基础上增加了： 错误处理。
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="error"></param>
        /// <param name="completionWork"></param>
        /// <param name="callbackArguments"></param>
        protected override void OnQueryFinished(object newData, Exception error, DispatcherOperationCallback completionWork, object callbackArguments)
        {
            base.OnQueryFinished(newData, error, completionWork, callbackArguments);

            if (error != null)
            {
                Logger.LogError("CSLADataProvider获取数据报错", error);

                Action<Exception> action = e => e.Alert();
                Application.Current.Dispatcher.Invoke(action, error);
            }
        }

        /// <summary>
        /// 每取消一次，递增一。
        /// 这样，上次的异步调用在执行完成后，也不会返回结果给基类。
        /// </summary>
        public void CancelAsync()
        {
            this._queryVersion++;
        }

        #region private class QueryRequest

        private class QueryRequest
        {
            /// <summary>
            /// 查询的版本号。
            /// 
            /// 主要用于异步查询。
            /// 如果Version和当前的DataProvider对象的Version相等，表示当前这次异步查询没有被CancelAsync方法取消掉。
            /// </summary>
            public int Version;

            public Func<object> DataProducer;
        }

        #endregion
    }
}