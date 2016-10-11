/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140516
 * 说明：见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140516 00:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain
{
    /// <summary>
    /// 实体都必须实现此接口。
    /// </summary>
    public interface IEntityWithId
    {
        /// <summary>
        /// 唯一的标识属性
        /// </summary>
        object Id { get; set; }

        /// <summary>
        /// 标识属性的算法提供器。
        /// </summary>
        IKeyProvider IdProvider { get; }
    }
}
