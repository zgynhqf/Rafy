/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// 当前对象的上下文环境
    /// </summary>
    public interface ICurrentContext
    {
        /// <summary>
        /// 当前“激活/显示/选中”的对象
        /// </summary>
        Entity Current { get; set; }

        /// <summary>
        /// 当前对象变化事件。
        /// </summary>
        event EventHandler CurrentChanged;
    }
}
