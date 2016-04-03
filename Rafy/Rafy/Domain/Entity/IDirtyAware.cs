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

namespace Rafy.Domain
{
    /// <summary>
    /// 是否被修改的数据的接口
    /// </summary>
    public interface IDirtyAware
    {
        /// <summary>
        /// 当前的模型，是否是脏的。
        /// 一个脏的对象，表示它的状态还没有保存起来。
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// 标记为已经保存。IsDirty 为 false。
        /// </summary>
        void MarkSaved();
    }
}
