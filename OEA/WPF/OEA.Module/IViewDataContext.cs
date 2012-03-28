using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenExpressApp
{
    /// <summary>
    /// 对象绑定上下文
    /// </summary>
    public interface IViewDataContext
    {
        /// <summary>
        /// 绑定的对象
        /// </summary>
        object Data { get; set; }
    }
}
