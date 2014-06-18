using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF
{
    /// <summary>
    /// 异步获取数据的环境
    /// </summary>
    public interface IAsyncDataContext : INotifyDataChanged
    {
        /// <summary>
        /// 异步获取完数据后，异步线程发生此事件。
        /// </summary>
        event EventHandler<DataLoadedEventArgs> DataLoaded;
    }
}
