/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101111
 * 说明：元数据：实体的类型
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101111
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 元数据：实体对象类型
    /// </summary>
    public enum EntityCategory
    {
        /// <summary>
        /// 孩子对象
        /// </summary>
        Child,
        /// <summary>
        /// 根对象
        /// </summary>
        Root,
        /// <summary>
        /// 查询面板对象
        /// </summary>
        QueryObject
    }
}
