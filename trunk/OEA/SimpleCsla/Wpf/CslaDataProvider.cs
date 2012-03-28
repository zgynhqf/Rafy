using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Reflection;
using SimpleCsla.Reflection;
using SimpleCsla.Properties;
using System.Diagnostics;
using System.Collections.Specialized;
using SimpleCsla.Core;

namespace SimpleCsla.Wpf
{
    /// <summary>
    /// Wraps and creates a CSLA .NET-style object 
    /// that you can use as a binding source.
    /// </summary>
    public class CslaDataProvider : DataSourceProvider
    {
        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public CslaDataProvider()
        {
            _factoryParameters = new ObservableCollection<object>();
            _factoryParameters.CollectionChanged += new NotifyCollectionChangedEventHandler(_factoryParameters_CollectionChanged);
        }

        public event EventHandler<SavedEventArgs> Saved;

        protected void OnSaved(object newObject, Exception error, object userState)
        {
            if (Saved != null)
                Saved(this, new SimpleCsla.Core.SavedEventArgs(newObject, error, userState));
        }

        void _factoryParameters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            BeginQuery();
        }

        #region Properties

        private Type _objectType = null;
        private string _factoryMethod = string.Empty;
        private ObservableCollection<object> _factoryParameters;
        private bool _isAsynchronous;
        private bool _isBusy;

        /// <summary>
        /// Gets or sets the type of object 
        /// to create an instance of.
        /// </summary>
        public Type ObjectType
        {
            get
            {
                return _objectType;
            }
            set
            {
                _objectType = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ObjectType"));
            }
        }

        /// <summary>
        /// Gets or sets the name of the static
        /// (Shared in Visual Basic) factory method
        /// that should be called to create the
        /// object instance.
        /// </summary>
        public string FactoryMethod
        {
            get
            {
                return _factoryMethod;
            }
            set
            {
                _factoryMethod = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FactoryMethod"));
            }
        }

        /// <summary>
        /// Get the list of parameters to pass
        /// to the factory method.
        /// </summary>
        public IList FactoryParameters
        {
            get
            {
                return _factoryParameters;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates 
        /// whether to perform object creation in 
        /// a worker thread or in the active context.
        /// </summary>
        public bool IsAsynchronous
        {
            get { return _isAsynchronous; }
            set { _isAsynchronous = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the data
        /// object.
        /// </summary>
        public object ObjectInstance
        {
            get { return Data; }
            set
            {
                OnQueryFinished(value, null, null, null);
                OnPropertyChanged(new PropertyChangedEventArgs("ObjectInstance"));
            }
        }

        /// <summary>
        /// Gets a value indicating if this object is busy.
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set
            {
                _isBusy = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsBusy"));
            }
        }

        /// <summary>
        /// Triggers WPF data binding to rebind to the
        /// data object.
        /// </summary>
        public void Rebind()
        {
            object tmp = ObjectInstance;
            ObjectInstance = null;
            ObjectInstance = tmp;
        }

        #endregion

        #region Query

        private bool _firstRun = true;
        private bool _init = true;
        private bool _endInitCompete = false;
        private bool _endInitError = false;

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

        /// <summary>
        /// Overridden. Starts to create the requested object, 
        /// either immediately or on a background thread, 
        /// based on the value of the IsAsynchronous property.
        /// </summary>
        protected override void BeginQuery()
        {
            if (_init)
                return;

            if (_firstRun)
            {
                _firstRun = false;
                if (!IsInitialLoadEnabled)
                    return;
            }

            if (_endInitError)
            {
                // this handles a case where the WPF form initilizer
                // invokes the data provider twice when an exception
                // occurs - we really don't want to try the query twice
                // or report the error twice
                _endInitError = false;
                OnQueryFinished(null);
                return;
            }

            if (this.IsRefreshDeferred)
                return;

            var request = this.CreateRequest();
            request.Version = this._queryVersion;//huqf

            IsBusy = true;

            if (IsAsynchronous)
                System.Threading.ThreadPool.QueueUserWorkItem(DoQuery, request);
            else
                DoQuery(request);
        }

        protected virtual QueryRequest CreateRequest()
        {
            return new CslaQueryRequest()
            {
                ObjectType = _objectType,
                FactoryMethod = _factoryMethod,
                FactoryParameters = _factoryParameters
            };
        }

        /// <summary>
        /// 查询的版本号。
        /// 
        /// 每取消一次，递增一。
        /// </summary>
        private int _queryVersion;

        /// <summary>
        /// 每取消一次，递增一。
        /// 这样，上次的异步调用在执行完成后，也不会返回结果给基类。
        /// </summary>
        public void CancelAsync()
        {
            this._queryVersion++;
        }

        private void DoQuery(object state)
        {
            var request = state as QueryRequest;
            var result = DoQueryCore(request);

            //if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            //  System.Windows.Application.Current.Dispatcher.Invoke(
            //    new Action(() => { IsBusy = false; }), 
            //    new object[] { });

            if (!_endInitCompete && result.Exception != null) _endInitError = true;

            //如果没有取消，则继续通知更改。
            if (request.Version == this._queryVersion)
            {
                // return result to base class
                this.OnQueryFinished(result);
            }
        }

        protected virtual QueryResult DoQueryCore(QueryRequest request)
        {
            var cslaRequest = request as CslaQueryRequest;
            if (cslaRequest == null) throw new InvalidOperationException("请重写 DoQueryCore 方法来支持非 SimpleCsla 的自定义查询方案。");

            return DoCslaQuery(cslaRequest);
        }

        private static QueryResult DoCslaQuery(CslaQueryRequest request)
        {
            var result = new QueryResult();

            object[] parameters = new List<object>(request.FactoryParameters).ToArray();

            try
            {
                // get factory method info
                BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy;
                MethodInfo factory = request.ObjectType.GetMethod(
                  request.FactoryMethod, flags, null,
                  MethodCaller.GetParameterTypes(parameters), null);

                if (factory == null)
                {
                    // strongly typed factory couldn't be found
                    // so find one with the correct number of
                    // parameters 
                    int parameterCount = parameters.Length;
                    MethodInfo[] methods = request.ObjectType.GetMethods(flags);
                    foreach (MethodInfo method in methods)
                        if (method.Name == request.FactoryMethod && method.GetParameters().Length == parameterCount)
                        {
                            factory = method;
                            break;
                        }
                }

                if (factory == null)
                {
                    // no matching factory could be found
                    // so throw exception
                    throw new InvalidOperationException(
                      string.Format("No such factory method:{0}", request.FactoryMethod));
                }

                // invoke factory method
                try
                {
                    result.Data = factory.Invoke(null, parameters);
                }
                catch (SimpleCsla.DataPortalException ex)
                {
                    result.Exception = ex.BusinessException;
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        result.Exception = ex.InnerException;
                        var dpe = result.Exception as SimpleCsla.DataPortalException;
                        if (dpe != null && dpe.BusinessException != null)
                            result.Exception = dpe.BusinessException;
                    }
                    else
                        result.Exception = ex;
                }
                catch (Exception ex)
                {
                    result.Exception = ex;
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }

            //if (request.ManageObjectLifetime && result != null)
            //{
            //    SimpleCsla.Core.ISupportUndo undo = result as SimpleCsla.Core.ISupportUndo;
            //    if (undo != null)
            //        undo.BeginEdit();
            //}

            return result;
        }

        #region QueryResult

        protected class QueryResult
        {
            public object Data { get; set; }
            public Exception Exception { get; set; }
        }

        protected void OnQueryFinished(QueryResult result)
        {
            this.OnQueryFinished(result.Data, result.Exception, (o) => { IsBusy = false; return null; }, null);
        }

        #endregion

        #region QueryRequest

        protected abstract class QueryRequest
        {
            /// <summary>
            /// 查询的版本号。
            /// 
            /// 主要用于异步查询。
            /// 如果Version和当前的DataProvider对象的Version相等，表示当前这次异步查询没有被CancelAsync方法取消掉。
            /// </summary>
            internal int Version;
        }

        private class CslaQueryRequest : QueryRequest
        {
            public Type ObjectType { get; set; }

            public string FactoryMethod { get; set; }

            private ObservableCollection<object> _factoryParameters;

            public ObservableCollection<object> FactoryParameters
            {
                get { return _factoryParameters; }
                set
                {
                    _factoryParameters = new ObservableCollection<object>(new List<object>(value));
                }
            }
        }

        #endregion

        #endregion

        //#region Cancel/Update/New/Remove

        ///// <summary>
        ///// Cancels changes to the business object, returning
        ///// it to its previous state.
        ///// </summary>
        ///// <remarks>
        ///// This metod does nothing unless ManageLifetime is
        ///// set to true and the object supports n-level undo.
        ///// </remarks>
        //public void Cancel()
        //{
        //    SimpleCsla.Core.ISupportUndo undo = this.Data as SimpleCsla.Core.ISupportUndo;
        //    if (undo != null && _manageLifetime)
        //    {
        //        IsBusy = true;
        //        undo.CancelEdit();
        //        undo.BeginEdit();
        //        IsBusy = false;
        //    }
        //}

        ///// <summary>
        ///// Accepts changes to the business object, and
        ///// commits them by calling the object's Save()
        ///// method.
        ///// </summary>
        ///// <remarks>
        ///// <para>
        ///// This method does nothing unless the object
        ///// implements SimpleCsla.Core.ISavable.
        ///// </para><para>
        ///// If the object implements IClonable, it
        ///// will be cloned, and the clone will be
        ///// saved.
        ///// </para><para>
        ///// If the object supports n-level undo and
        ///// ManageLifetime is true, then this method
        ///// will automatically call ApplyEdit() and
        ///// BeginEdit() appropriately.
        ///// </para>
        ///// </remarks>
        //public void Save()
        //{
        //    // only do something if the object implements
        //    // ISavable
        //    SimpleCsla.Core.ISavable savable = this.Data as SimpleCsla.Core.ISavable;
        //    if (savable != null)
        //    {
        //        object result = savable;
        //        Exception exceptionResult = null;
        //        try
        //        {
        //            IsBusy = true;

        //            // clone the object if possible
        //            ICloneable clonable = savable as ICloneable;
        //            if (clonable != null)
        //                savable = (SimpleCsla.Core.ISavable)clonable.Clone();

        //            // apply edits in memory
        //            SimpleCsla.Core.ISupportUndo undo = savable as SimpleCsla.Core.ISupportUndo;
        //            if (undo != null && _manageLifetime)
        //                undo.ApplyEdit();


        //            // save the clone
        //            result = savable.Save();

        //            if (!ReferenceEquals(savable, this.Data) && !SimpleCsla.ApplicationContext.AutoCloneOnUpdate)
        //            {
        //                // raise Saved event from original object
        //                Core.ISavable original = this.Data as Core.ISavable;
        //                if (original != null)
        //                    original.SaveComplete(result);
        //            }

        //            // start editing the resulting object
        //            undo = result as SimpleCsla.Core.ISupportUndo;
        //            if (undo != null && _manageLifetime)
        //                undo.BeginEdit();
        //        }
        //        catch (Exception ex)
        //        {
        //            exceptionResult = ex;
        //        }
        //        // clear previous object
        //        OnQueryFinished(null, exceptionResult, null, null);
        //        // return result to base class
        //        OnQueryFinished(result, null, null, null);
        //        IsBusy = false;
        //        OnSaved(result, exceptionResult, null);
        //    }
        //}

        ///// <summary>
        ///// Adds a new item to the object if the object
        ///// implements IBindingList and AllowNew is true.
        ///// </summary>
        //public object AddNew()
        //{
        //    // only do something if the object implements
        //    // IBindingList
        //    IBindingList list = this.Data as IBindingList;
        //    if (list != null && list.AllowNew)
        //        return list.AddNew();
        //    else
        //        return null;
        //}

        ///// <summary>
        ///// Removes an item from the list if the object
        ///// implements IBindingList and AllowRemove is true.
        ///// </summary>
        ///// <param name="item">
        ///// The item to be removed from the list.
        ///// </param>
        //public void RemoveItem(object item)
        //{
        //    // only do something if the object implements
        //    // IBindingList
        //    IBindingList list;
        //    SimpleCsla.Core.BusinessBase bb = item as SimpleCsla.Core.BusinessBase;
        //    if (bb != null)
        //        list = bb.Parent as IBindingList;
        //    else
        //        list = this.Data as IBindingList;
        //    if (list != null && list.AllowRemove)
        //        list.Remove(item);
        //}

        //#endregion
    }
}