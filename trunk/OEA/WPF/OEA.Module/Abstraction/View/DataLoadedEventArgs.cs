using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// DataLoaded的事件参数。
    /// </summary>
    public class DataLoadedEventArgs : EventArgs
    {
        private object _data;

        public DataLoadedEventArgs(object data)
        {
            this._data = data;
        }

        /// <summary>
        /// 事件中，可以对这个数据进行更改。
        /// </summary>
        public object Data
        {
            get
            {
                return this._data;
            }
            set
            {
                this._data = value;
            }
        }
    }
}
