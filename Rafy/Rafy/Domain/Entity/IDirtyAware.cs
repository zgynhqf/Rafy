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
        /// 递归将整个组合对象树都标记为已保存。（IsDirty 为 false）。
        /// 同时：
        /// * 实体所有属性的状态，都将改为已保存。
        /// * 递归将聚合子列表、树型子列表中的实体，都标记为已保存。
        /// * 聚合子列表中已经删除的实体，也会在这个列表中被移除。（EntityList 会被标记为已保存）
        /// </summary>
        void MarkSaved();
    }
}
