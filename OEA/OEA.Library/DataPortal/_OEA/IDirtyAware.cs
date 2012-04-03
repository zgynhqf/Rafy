/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Library
{
    /// <summary>
    /// 是否被修改的数据的接口
    /// </summary>
    public interface IDirtyAware
    {
        /// <summary>
        /// 当前的模型，是否是糟的。
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 清除“IsDirty”的状态
        /// </summary>
        void MarkOld();
    }
}
