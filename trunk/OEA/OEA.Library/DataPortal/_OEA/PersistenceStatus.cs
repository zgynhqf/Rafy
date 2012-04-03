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
    /// 实体状态
    /// </summary>
    public enum PersistenceStatus
    {
        /// <summary>
        /// 未改动
        /// </summary>
        Unchanged,
        /// <summary>
        /// 数据变更
        /// </summary>
        Modified,
        /// <summary>
        /// 新对象
        /// </summary>
        New,
        /// <summary>
        /// 标记删除
        /// </summary>
        Deleted
    }
}
