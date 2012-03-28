using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 一个自我“活动的”ICurrentObjectContext
    /// </summary>
    public interface IActiveCurrentObjectContext : ICurrentObjectContext
    {
        /// <summary>
        /// 刷新CurrentObject
        /// </summary>
        void RefreshCurrentEntity();

        /// <summary>
        /// 加载需要的数据
        /// </summary>
        void LoadDataFromParent();
    }
}
