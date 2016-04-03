/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140521
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140521 21:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy
{
    /// <summary>
    /// 一个拥有名称的对象。
    /// </summary>
    public interface IHasName
    {
        /// <summary>
        /// 名称
        /// </summary>
        string Name { get; }
    }
}
