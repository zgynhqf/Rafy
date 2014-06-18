using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// DataLoaded的事件参数。
    /// </summary>
    public class DataLoadedEventArgs : EventArgs
    {
        private IDomainComponent _data;

        public DataLoadedEventArgs(IDomainComponent data)
        {
            this._data = data;
        }

        /// <summary>
        /// 事件中，可以对这个数据进行更改。
        /// </summary>
        public IDomainComponent Data
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
