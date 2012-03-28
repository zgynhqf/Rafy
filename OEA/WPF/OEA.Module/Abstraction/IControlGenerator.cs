using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA
{
    /// <summary>
    /// 控件生成器
    /// </summary>
    public interface IControlGenerator
    {
        /// <summary>
        /// 创建当前ObjectView关联的Control
        /// </summary>
        /// <returns></returns>
        object CreateControl();
    }
}
