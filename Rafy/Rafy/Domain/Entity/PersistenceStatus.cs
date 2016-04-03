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
    /// 实体的持久化状态。
    /// 保存实体时，会根据这个状态来进行对应的增、删、改的操作。
    /// </summary>
    public enum PersistenceStatus
    {
        /// <summary>
        /// 未改动
        /// </summary>
        Unchanged = 0,
        /// <summary>
        /// 数据变更。仓库保存时，需要执行更新操作。
        /// </summary>
        Modified = 1,
        /// <summary>
        /// 新对象。仓库保存时，需要执行添加操作。
        /// </summary>
        New = 2,
        /// <summary>
        /// 已删除。仓库保存时，需要执行删除操作。
        /// </summary>
        Deleted = 3
    }
}