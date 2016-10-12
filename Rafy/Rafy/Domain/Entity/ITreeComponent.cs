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

    /// <summary>
    /// 树型实体对应的属性与操作。
    /// </summary>
    public interface ITreeEntity : ITreeComponent, IEntity
    {
        /// <summary>
        /// 是否为树型实体。
        /// </summary>
        bool SupportTree { get; }

        /// <summary>
        /// 返回当前的 TreeParent 的值是否已经加载。
        /// </summary>
        bool IsTreeParentLoaded { get; }

        /// <summary>
        /// 此节点在树中的级别。 根节点是第一级。 此级别是完全根据 Rafy.Domain.Entity.TreeIndex 计算出来的。 如果此实体不是一个树实体，则返回 -1。
        /// </summary>
        int TreeLevel { get; }

        /// <summary>
        /// 树型实体的树型索引编码 这个属性是实现树型实体的关键所在！
        /// </summary>
        string TreeIndex { get; set; }

        /// <summary>
        /// 树型父实体的 Id 属性 默认使用存储于数据库中的字段，子类可以重写此属性以实现自定义的父子结构逻辑。
        /// </summary>
        object TreePId { get; set; }

        /// <summary>
        /// 树中的父对象。 操作此属性，同样引起 TreeChildren、EntityList 的变化。 同时，注意此属性并不是懒加载属性。
        /// </summary>
        Entity TreeParent { get; set; }

        /// <summary>
        /// 树中的子对象集合。 操作此属性，同样引起 TreeParent、EntityList 的变化。 同时，注意此属性并不是懒加载属性。
        /// </summary>
        Entity.EntityTreeChildren TreeChildren { get; }
    }
}