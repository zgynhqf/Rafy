/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using System.Diagnostics;
using System.Threading;

namespace Rafy.Threading
{
    /// <summary>
    /// 预加载的实现类。
    /// 
    /// 实现某个action的预加载。
    /// </summary>
    public class ForeAsyncLoader
    {
        /// <summary>
        /// 同步两个线程的信号
        /// </summary>
        private EventWaitHandle _signal = new ManualResetEvent(false);

        private LoaderStatus _status = LoaderStatus.NotStarted;

        /// <summary>
        /// _status的“版本号”
        /// </summary>
        private int _version = 0;

        /// <summary>
        /// 真正执行的耗时的操作
        /// </summary>
        private Action _action;

        /// <summary>
        /// 构造一个对应指定方法的预加载器。
        /// </summary>
        /// <param name="loadAction">
        /// 真正的加载方法，比较耗时的操作。
        /// </param>
        public ForeAsyncLoader(Action loadAction)
        {
            Debug.Assert(loadAction != null, "loadAction != null");
            this._action = loadAction.WrapByCurrentPrinciple();
        }

        /// <summary>
        /// 是否已经加载完毕。
        /// </summary>
        /// <returns></returns>
        public bool IsRunOver()
        {
            return this._status == LoaderStatus.Completed ||
                this._status == LoaderStatus.Failed;
        }

        /// <summary>
        /// 加载器状态
        /// </summary>
        public LoaderStatus Status
        {
            get
            {
                return this._status;
            }
            private set
            {
                this._version++;
                this._status = value;
            }
        }

        /// <summary>
        /// 执行成功的事件。
        /// </summary>
        public event EventHandler ActionSucceeded;

        /// <summary>
        /// 开始异步进行预加载。
        /// 
        /// 如果在进行执行Reset操作前，调用本方法多次，也只会执行一次loadAction。
        /// </summary>
        public void BeginLoading()
        {
            if (this.Status == LoaderStatus.NotStarted)
            {
                this.Status = LoaderStatus.Running;
                var version = this._version;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    LoaderStatus status = LoaderStatus.NotStarted;
                    try
                    {
                        this._action();
                        status = LoaderStatus.Completed;

                        //触发事件。
                        if (this.ActionSucceeded != null)
                        {
                            this.ActionSucceeded(this, EventArgs.Empty);
                        }
                    }
                    catch
                    {
                        status = LoaderStatus.Failed;
                        throw;
                    }
                    finally
                    {
                        this._signal.Set();

                        //有可能外界已经使用Reset重设了状态，那么这次执行就算被“丢弃”了，不再设置值。
                        if (this._version == version)
                        {
                            this.Status = status;
                        }
                    }
                });
            }
        }

        /// <summary>
        /// 等待异步加载完成。
        /// 
        /// （注意，如果在这个方法之前没有调用Begin，则这里也会先调用Begin。）
        /// </summary>
        public void WaitForLoading()
        {
            this.BeginLoading();
            if (this._status == LoaderStatus.Running)
            {
                this._signal.WaitOne();
            }
        }

        /// <summary>
        /// 重设本加载器，使得BeginLoading可以再次起作用。
        /// </summary>
        public void Reset()
        {
            this.Status = LoaderStatus.NotStarted;
            this._signal.Reset();
        }
    }

    /// <summary>
    /// 预加载器状态。
    /// </summary>
    public enum LoaderStatus
    {
        /// <summary>
        /// 还未启动
        /// </summary>
        NotStarted,
        /// <summary>
        /// 正在加载中。
        /// </summary>
        Running,
        /// <summary>
        /// 表示是否已经运行过。
        /// </summary>
        Completed,
        /// <summary>
        /// 加载失败。
        /// </summary>
        Failed
    }
}