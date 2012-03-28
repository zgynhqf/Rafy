using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 产品的“721”级别
    /// </summary>
    public enum ReuseLevel
    {
        /// <summary>
        /// 系统级别的重用级别，一般不要使用。
        /// </summary>
        _System = 1,

        /// <summary>
        /// 7
        /// </summary>
        Main = 10,

        /// <summary>
        /// 2
        /// </summary>
        Part = 20,

        /// <summary>
        /// 1
        /// </summary>
        Customized = 30
    }
}
