using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.ObjectModel;

namespace Rafy.Threading
{
    public class ParallelActions : Collection<Action>, IParallelActions
    {
        private int _maxCount = 64;

        private bool _isRunning;

        #region IParallelActions Members

        public int MaxThreadCount
        {
            get
            {
                return this._maxCount;
            }
            set
            {
                this._maxCount = value;
            }
        }

        public void Prepare(Action action)
        {
            this.Add(action);
        }

        public void RunAll()
        {
            try
            {
                this._isRunning = true;

                Run(this, this._maxCount);
            }
            finally
            {
                this._isRunning = false;
            }
        }

        #endregion

        protected override void ClearItems()
        {
            this.CheckNotRunning();
            base.ClearItems();
        }

        protected override void InsertItem(int index, Action item)
        {
            this.CheckNotRunning();
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, Action item)
        {
            this.CheckNotRunning();
            base.SetItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            this.CheckNotRunning();
            base.RemoveItem(index);
        }

        private void CheckNotRunning()
        {
            if (this._isRunning)
            {
                throw new InvalidOperationException("正在执行中，集合不可更改。");
            }
        }

        private static void Run(IList<Action> actions, int maxThreadCount)
        {
            if (actions.Count <= 0)
            {
                return;
            }

            #region 任务太多，分批做。

            if (actions.Count > maxThreadCount)
            {
                var count = actions.Count / maxThreadCount;
                var yu = actions.Count % maxThreadCount;
                if (yu > 0)
                {
                    count = count + 1;
                }
                List<Action> tempActions = new List<Action>(maxThreadCount);
                for (int i = 0; i < count; i++)
                {
                    tempActions.Clear();
                    int baseIndex = i * maxThreadCount;
                    for (int j = baseIndex; j < baseIndex + maxThreadCount && j < actions.Count; j++)
                    {
                        tempActions.Add(actions[j]);
                    }

                    Run(tempActions, maxThreadCount);
                }

                return;
            }

            #endregion

            AutoResetEvent[] events = new AutoResetEvent[actions.Count - 1];

            //每个action都开个线程
            //第一个不需要放在线程池，由当前线程直接执行
            for (int i = 1, c = actions.Count; i < c; i++)
            {
                var action = actions[i].WrapByCurrentPrinciple();
                var autoResetEvent = new AutoResetEvent(false);
                events[i - 1] = autoResetEvent;

                ThreadPool.QueueUserWorkItem(e =>
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        autoResetEvent.Set();
                    }
                });
            }

            //先把第一个操作做完
            actions[0]();

            //等等其它的所有线程
            foreach (var item in events)
            {
                item.WaitOne();
            }
        }
    }
}
