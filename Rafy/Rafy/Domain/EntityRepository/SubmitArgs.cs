/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130416 19:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 提交参数。
    /// </summary>
    public struct SubmitArgs
    {
        private SubmitAction _action;

        /// <summary>
        /// 被保存的领域实体
        /// </summary>
        public Entity Entity { get; internal set; }

        /// <summary>
        /// 保存的操作
        /// </summary>
        public SubmitAction Action
        {
            get { return _action; }
            internal set { _action = value; }
        }

        /// <summary>
        /// 对应的数据提供器。
        /// </summary>
        public RepositoryDataProvider DataProvider { get; internal set; }

        /// <summary>
        /// 标记本次操作，不但要保存所有的子实体，也要保存当前对象。
        /// 场景：
        /// 子类重写 Submit 方法后，在当前实体数据不脏、只更新组合子实体（ChildrenOnly）的模式下，
        /// 如果修改了当前实体的状态，则需要使用这个方法把提交操作提升为保存整个组合对象（Update），这样当前实体才会被保存。
        /// 
        /// 注意，由于 SubmitArgs 是一个结构体，所以调用此方法只会更改当前对象的值。需要把这个改了值的对象传入基类的方法，才能真正地更新当前的实体对象。
        /// </summary>
        /// <exception cref="System.InvalidOperationException">只有在 ChildrenOnly 模式下，才可以调用此方法。</exception>
        public void UpdateCurrent()
        {
            if (_action != SubmitAction.ChildrenOnly)
            {
                throw new InvalidOperationException("只有在 ChildrenOnly 模式下，才可以调用此方法。");
            }

            _action = SubmitAction.Update;
        }

        /// <summary>
        /// 是否需要同时处理当前树的子节点。
        /// 本属性只用于判断当前类型的树节点，不用于指定组合子中出现的其它树。
        /// </summary>
        internal bool WithTreeChildren;
    }

    /// <summary>
    /// 提交数据的操作类型
    /// </summary>
    public enum SubmitAction
    {
        /// <summary>
        /// 更新实体
        /// 将会执行 Update、SubmitChildren、SubmitTreeChildren。
        /// </summary>
        Update,
        /// <summary>
        /// 插入实体
        /// 将会执行 Insert、SubmitChildren、SubmitTreeChildren。
        /// </summary>
        Insert,
        /// <summary>
        /// 删除实体
        /// 将会执行 DeleteChildren、DeleteTreeChildren、Delete
        /// </summary>
        Delete,
        /// <summary>
        /// 当前对象未变更，只提交其中的子对象。
        /// 将会执行 SubmitChildren、SubmitTreeChildren。
        /// </summary>
        ChildrenOnly
    }
}
