using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace OEA
{
    /// <summary>
    /// 这是一个特殊的数据上下文，
    /// 它有一个特殊的数据对象CurrentObject，
    /// CurrentObject是Data或者Data的部分组成对象。
    /// </summary>
    public interface ICurrentObjectContext
    {
        /// <summary>
        /// 当前“激活/显示/选中”的对象
        /// </summary>
        Entity Current { get; set; }

        event EventHandler CurrentObjectChanged;
    }
}
