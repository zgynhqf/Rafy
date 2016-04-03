using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Rafy.Threading
{
    internal class AsyncMultiActions : IObservableActions
    {
        private Queue<AutoResetEvent> _events = new Queue<AutoResetEvent>();
        private bool _started;

        public event EventHandler FirstActionStarted;
        public event EventHandler LastActionEnded;

        public void Execute(Action action)
        {
            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            lock (this)
            {
                this._events.Enqueue(autoResetEvent);
            }

            action = action.WrapByCurrentPrinciple();

            //在后台线程中，执行这个操作
            ThreadPool.QueueUserWorkItem(o =>
            {
                //执行Action
                action();

                autoResetEvent.Set();
            });

            this.Start();
        }

        private void Start()
        {
            bool raiseStarted = false;
            lock (this)
            {
                if (this._started == false)
                {
                    this._started = true;
                    raiseStarted = true;

                    this.CreateEnder();
                }
            }
            if (raiseStarted)
            {
                if (this.FirstActionStarted != null)
                {
                    this.FirstActionStarted(this, EventArgs.Empty);
                }
            }
        }
        private void CreateEnder()
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                while (true)
                {
                    int count = 0;
                    lock (this)
                    {
                        count = this._events.Count;
                    }
                    if (count <= 0)
                    {
                        break;
                    }

                    AutoResetEvent autoResetEvent = null;
                    lock (this)
                    {
                        autoResetEvent = this._events.Dequeue();
                    }
                    using (autoResetEvent)
                    {
                        autoResetEvent.WaitOne();
                    }
                }

                this.End();
            });
        }
        private void End()
        {
            bool raise = false;
            lock (this)
            {
                if (this._started)
                {
                    this._started = false;
                    raise = true;
                }
            }
            if (raise)
            {
                if (this.LastActionEnded != null)
                {
                    this.LastActionEnded(this, EventArgs.Empty);
                }
            }
        }
    }
}
