/*******************************************************
 *
 * 作者：胡庆访
 * 创建日期：20140527
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 *
 * 历史记录：
 * 创建文件 胡庆访 20140527 20:24
 *
*******************************************************/

using System;

namespace Rafy.Domain
{
    /// <summary>
    /// 树结构中的组成元素。
    /// 目前有三个实现该接口的类型：Entity、EntityList、EntityTreeChildren。
    /// </summary>
    public interface ITreeComponent : IDirtyAware
    {
        /// <summary>
        /// 返回当前树是否已经加载完全。
        /// </summary>
        bool IsFullLoaded { get; }

        /// <summary>
        /// 此组件的上一级组件。
        /// </summary>
        ITreeComponent TreeComponentParent { get; }

        /// <summary>
        /// 当前组件的类型。
        /// </summary>
        TreeComponentType ComponentType { get; }

        /// <summary>
        /// 递归对于整个树中的每一个节点都调用 action。
        /// </summary>
        /// <param name="action">对每一个节点调用的方法。方法如何返回 true，则表示停止循环，返回该节点。</param>
        /// <returns>第一个被调用 action 后返回 true 的节点。</returns>
        Entity EachNode(Func<Entity, bool> action);

        /// <summary>
        /// 统计当前树中已经加载的节点的个数。
        /// </summary>
        /// <returns></returns>
        int CountNodes();

        /// <summary>
        /// 递归加载所有树节点。
        /// </summary>
        void LoadAllNodes();
    }

    /// <summary>
    /// 树中的所有组件。
    /// </summary>
    public enum TreeComponentType
    {
        /// <summary>
        /// 最上层节点的列表。
        /// </summary>
        NodeList = 0,
        /// <summary>
        /// 实体节点。
        /// </summary>
        Node = 1,
        /// <summary>
        /// 某个节点的所有子节点的集合。
        /// </summary>
        TreeChildren = 2
    }
}