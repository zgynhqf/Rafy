using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module;

namespace OEA
{
    public static class AsyncDataContextHelper
    {
        #region DataLoaded Event

        /// <summary>
        /// 监听IAsyncDataContext的DataChanged事件。且保证其只发生一次。
        /// </summary>
        /// <param name="con"></param>
        /// <param name="callBack">在异步加载数据完成后，调用callBack方法</param>
        public static void ListenDataLoadedOnce(this IAsyncDataContext con, Action<DataLoadedEventArgs> callBack)
        {
            var dele = new DetachLoadedEventDelegate();
            dele._con = con;
            dele._action = callBack;

            dele._con.DataLoaded += dele.OnDataLoaded;
        }

        private class DetachLoadedEventDelegate
        {
            public IAsyncDataContext _con;

            public Action<DataLoadedEventArgs> _action;

            public void OnDataLoaded(object sender, DataLoadedEventArgs e)
            {
                this._con.DataLoaded -= this.OnDataLoaded;

                this._action(e);
            }
        }

        #endregion

        #region DataChanged Event

        /// <summary>
        /// 监听IAsyncDataContext的DataChanged事件。且保证其只发生一次。
        /// </summary>
        /// <param name="con"></param>
        /// <param name="callBack">在异步加载数据完成后，调用callBack方法</param>
        public static void ListenDataChangedOnce(this INotifyDataChanged con, Action callBack)
        {
            var dele = new DetachChangedEventDelegate();
            dele._con = con;
            dele._action = callBack;

            dele._con.DataChanged += dele.OnDataChanged;
        }

        private class DetachChangedEventDelegate
        {
            public INotifyDataChanged _con;

            public Action _action;

            public void OnDataChanged(object sender, EventArgs e)
            {
                this._con.DataChanged -= this.OnDataChanged;

                this._action();
            }
        }

        #endregion
    }
}