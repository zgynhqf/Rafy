using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Rafy.WPF
{
    /// <summary>
    /// 通知数据变更的对象
    /// </summary>
    public interface INotifyDataChanged
    {
        /// <summary>
        /// 异步获取并设置完数据后，异步线程发生此事件。
        /// </summary>
        event EventHandler DataChanged;
    }
}
