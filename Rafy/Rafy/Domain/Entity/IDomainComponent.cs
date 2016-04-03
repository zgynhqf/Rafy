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
    /// 实体模块组件（<see cref="Entity"/> Or <see cref="EntityList"/>）
    /// </summary>
    public interface IDomainComponent : IDirtyAware
    {
        /// <summary>
        /// 获取父组件
        /// 
        /// 列表的父组件是一个实体，而实体的父组件则是其所在的列表。
        /// </summary>
        IDomainComponent Parent { get; }

        /// <summary>
        /// 获取该实体列表对应的仓库类，如果没有找到，则抛出异常。
        /// </summary>
        /// <returns></returns>
        IRepository GetRepository();

        /// <summary>
        /// 尝试找到这个实体列表对应的仓库类。
        /// 
        /// 没有标记 RootEntity/ChildEntity 的类型是没有仓库类的，例如所有的条件类型。
        /// </summary>
        /// <returns></returns>
        IRepository FindRepository();

        /// <summary>
        /// 设置父组件
        /// </summary>
        /// <param name="parent"></param>
        void SetParent(IDomainComponent parent);
    }
}
