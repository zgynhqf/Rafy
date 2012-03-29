using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using OEA;
using System.Threading.Tasks;
using System.Diagnostics;
using OEA.Threading;

namespace OEA.Module.WPF
{
    public static class AsyncAction
    {
        #region API多种使用方法

        #region 第一种

        //AsyncAction.Execute(false, e =>
        //{
        //    e.ProgressValue = new ProgressValue()
        //    {
        //        Percent = 10,
        //        Text = "开始计算..."
        //    };
        //    Thread.Sleep(3000);
        //    e.ProgressValue = new ProgressValue()
        //    {
        //        Percent = 30,
        //        Text = "还在计算..."
        //    };
        //    Thread.Sleep(3000);
        //    e.ProgressValue = new ProgressValue()
        //    {
        //        Percent = 60,
        //        Text = "还没算完..."
        //    };
        //    Thread.Sleep(3000);
        //    e.ProgressValue = new ProgressValue()
        //    {
        //        Percent = 90,
        //        Text = "进入倒计时！"
        //    };
        //    Thread.Sleep(1000);
        //});  

        #endregion

        #region 第二种

        //AsyncAction.EstimateExecute(() =>
        //{
        //    Thread.Sleep(11000);
        //}, new TimedProgressValue(2, 10, "开始计算...")
        //, new TimedProgressValue(2, 30, "还在计算中...")
        //, new TimedProgressValue(2, 60, "主要结果已经计算完毕...")
        //, TimedProgressValue.End(2)); 

        #endregion

        #endregion

        public static void EstimateExecute(Action action, params TimedProgressValue[] estimateTimes)
        {
            EstimateExecute(action, null, estimateTimes);
        }

        public static void EstimateExecute(Action action, Action endAsyncCallBack, params TimedProgressValue[] estimateTimes)
        {
            Execute(false, e =>
            {
                var start = TimedProgressValue.New(TimeSpan.FromMilliseconds(100), 1, null);

                List<TimedProgressValue> values = new List<TimedProgressValue>(estimateTimes.Length + 2);
                values.Add(start);

                TimeSpan timeNeed = TimeSpan.FromSeconds(0.5);
                double percent = 1;
                for (int i = 0, c = estimateTimes.Length; i < c; i++)
                {
                    var item = estimateTimes[i];

                    var itemTime = item.TimeNeed;
                    item.TimeNeed = timeNeed;
                    timeNeed += itemTime;

                    var tmp = item.Percent;
                    item.Percent = percent;
                    percent = tmp;

                    values.Add(item);
                }

                var end = TimedProgressValue.New(timeNeed, 99, "即将完成...");
                values.Add(end);

                //start all timer
                for (int i = 0, c = values.Count; i < c; i++)
                {
                    var item = values[i];

                    var timer = new System.Timers.Timer();
                    timer.Interval = item.TimeNeed.TotalMilliseconds;
                    timer.Elapsed += (o, te) =>
                    {
                        e.ProgressValue = item;
                        timer.Stop();
                    };
                    timer.Start();
                }

                action();
            }, endAsyncCallBack);
        }

        /// <summary>
        /// 使用异步线程执行某个Action。
        /// 默认不使用ActionParam来报告进度
        /// </summary>
        /// <param name="action"></param>
        public static void Execute(Action action)
        {
            Execute(true, p => action());
        }

        public static void Execute(Action action, Action endAsyncCallBack)
        {
            Execute(true, p => action(), endAsyncCallBack);
        }

        public static void Execute(Action<IProgressReporter> action)
        {
            Execute(true, action);
        }

        /// <summary>
        /// 使用异步线程执行某个Action
        /// </summary>
        /// <param name="isIndeterminate">
        /// 是否需要不停的来回显示提示条。
        /// 如果是false：可以使用ActionParam.ProgressValue来设置进度条进度。
        /// </param>
        /// <param name="action"></param>
        /// <param name="endAsyncCallBack">执行完异步操作后调用的回调函数</param>
        public static void Execute(bool isIndeterminate,
            Action<IProgressReporter> action,
            Action endAsyncCallBack = null,
            string promptContent = null)
        {
            WaitDialog win = new WaitDialog();
            if (promptContent != null)
            {
                win.txtTitle.Text = promptContent;
            }
            if (isIndeterminate)
            {
                win.waitBar.IsIndeterminate = true;
            }

            //在后台线程中，执行这个操作
            //ThreadHelper.SafeInvoke(() =>
            ThreadPool.QueueUserWorkItem(e =>
            {
                try
                {
                    //执行Action
                    action(new ActionParam(win));
                }
                catch (Exception ex)
                {
                    ex.Alert();
                }
                finally
                {
                    //执行完毕，关闭提示框
                    Action closeWindow = () =>
                    {
                        win.Close();
                        if (endAsyncCallBack != null)
                        {
                            endAsyncCallBack();
                        }
                    };
                    win.Dispatcher.BeginInvoke(closeWindow);
                }
            });

            //打开提示框
            win.Show();
        }

        private class ActionParam : IProgressReporter
        {
            private WaitDialog _win;

            public ActionParam(WaitDialog win)
            {
                this._win = win;
            }

            /// <summary>
            /// 1到100的数。
            /// </summary>
            public ProgressValue ProgressValue
            {
                get
                {
                    return this._win.ProgressValue;
                }
                set
                {
                    this._win.ProgressValue = value;
                }
            }

            public void Report(double percent, string text = null)
            {
                this.ProgressValue = new ProgressValue()
                {
                    Percent = percent,
                    Text = text
                };
            }
        }
    }

    [DebuggerDisplay("{TimeNeed} : {Percent} - {Text}")]
    public class TimedProgressValue : ProgressValue
    {
        public static TimedProgressValue New(int secondsNeed, double percent, string text)
        {
            return new TimedProgressValue(TimeSpan.FromSeconds(secondsNeed), percent, text);
        }

        public static TimedProgressValue New(TimeSpan timeNeed, double percent, string text)
        {
            return new TimedProgressValue(timeNeed, percent, text);
        }

        private TimedProgressValue(TimeSpan timeNeed, double percent, string text)
        {
            this.TimeNeed = timeNeed;
            this.Percent = percent;
            this.Text = text;
        }

        public TimeSpan TimeNeed { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}", this.TimeNeed, this.Percent, this.Text);
        }
    }

    /// <summary>
    /// 任意开始，一起结束
    /// </summary>
    public class AsyncMultiActionsWindow
    {
        private static readonly AsyncMultiActionsWindow Instance = new AsyncMultiActionsWindow();

        private WaitDialog _win;
        private IAsyncMultiActions _actions;

        private AsyncMultiActionsWindow()
        {
            this._actions = ThreadHelper.AsyncMultiActions;
            this._actions.FirstActionStarted += new EventHandler(Actions_Started);
            this._actions.LastActionEnded += new EventHandler(Actions_Ended);
        }

        private void Actions_Started(object sender, EventArgs e)
        {
            this.Show();
        }
        private void Actions_Ended(object sender, EventArgs e)
        {
            this.Close();
        }
        private void ExecuteInternal(Action action)
        {
            this._actions.Execute(action);
        }
        private void Show()
        {
            if (this._win == null)
            {
                this._win = new WaitDialog();
                this._win.waitBar.IsIndeterminate = true;
                this._win.Show();
            }
        }
        private void Close()
        {
            // 执行完毕，关闭提示框
            Action closeWindow = () =>
            {
                if (this._win != null)
                {
                    this._win.Close();
                    this._win = null;
                }
            };
            this._win.Dispatcher.BeginInvoke(closeWindow);
        }

        public static void Execute(Action action)
        {
            Instance.ExecuteInternal(action);
        }
    }
}