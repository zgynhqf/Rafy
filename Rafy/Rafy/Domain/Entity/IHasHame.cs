/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110525
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110525
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 有名称的对象
    /// </summary>
    public interface IHasHame
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; set; }
    }
}
