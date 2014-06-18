using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Rafy.WPF
{
    /// <summary>
    /// 对象绑定上下文
    /// </summary>
    public interface IDataContext : INotifyDataChanged
    {
        /// <summary>
        /// 绑定的对象
        /// </summary>
        object Data { get; set; }
    }
}