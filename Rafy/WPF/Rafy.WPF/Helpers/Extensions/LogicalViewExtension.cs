/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2011
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2011
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Domain;

namespace Rafy.WPF
{
    /// <summary>
    /// WPFLogicalView 相关的扩展方法。
    /// </summary>
    public static class LogicalViewExtension
    {
        /// <summary>
        /// 获取一个view的“活动”对象集。
        /// 
        /// 如果它是一个列表，则返回选中的所有对象。
        /// 否则，返回当前使用的对象CurrentObject。
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static IList<Entity> GetSelectedEntities(this LogicalView view)
        {
            if (view is ListLogicalView)
            {
                return (view as ListLogicalView).SelectedEntities;
            }

            var list = new List<Entity>();
            list.Add(view.Current);
            return list;
        }

        /// <summary>
        /// 通过 Id 来定位到列表视图中具体的一行。
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="id"></param>
        /// <rereturns>返回是否成功找到目标行。</rereturns>
        public static bool SetCurrentById(this ListLogicalView listView, object id)
        {
            var item = listView.Data.Linq
                .FirstOrDefault(entity => entity.Id.Equals(id));

            listView.Current = item;

            return item != null;
        }

        /// <summary>
        /// 定位焦点到整个视图树中第一个详细面板的第一个编辑器上。
        /// </summary>
        /// <param name="view">整个视图树中的根树。</param>
        /// <rereturns>如果成功定位，则返回该详细视图。</rereturns>
        public static DetailLogicalView FocusFirstEditor(this LogicalView view)
        {
            var detailView = view.TraverseScopeFirst(item =>
            {
                var detail = item as DetailLogicalView;
                if (detail != null && detail.PropertyEditors.Count > 0)
                {
                    //尝试设置第一个非只读的编辑器
                    var editor = detail.PropertyEditors.FirstOrDefault(e => e.IsReadOnly != ReadOnlyStatus.ReadOnly);
                    if (editor != null)
                    {
                        return detail.FocusToEditor(editor);
                    }
                }

                return false;
            }) as DetailLogicalView;

            return detailView;
        }

        /// <summary>
        /// 使用广度优先算法遍历整个 LogicalView 树型结构。
        /// </summary>
        /// <param name="treeRoot">整个视图树中的根树。</param>
        /// <param name="iterator">对树中的每一个元素执行的函数。返回值如果为true，则表示不需要继续查找下去。</param>
        /// <returns>如果 iterator 在某一个元素上返回 true，则返回这个元素。</returns>
        public static LogicalView TraverseScopeFirst(this LogicalView treeRoot, Func<LogicalView, bool> iterator)
        {
            //使用队列实现广度遍历
            var queue = new Queue<LogicalView>();
            queue.Enqueue(treeRoot);

            //由于 View 之间的关系是相互的，所以为了遍历死循环，需要一个列表把已经检测过的 VIew 存储起来。
            var doneList = new List<LogicalView>();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                doneList.Add(current);

                var stop = iterator(current);
                if (stop) return current;

                //同层级的 Relation 应该比 ChildrenViews 优先被检测。
                foreach (var relation in current.Relations)
                {
                    var relationView = relation.View as LogicalView;

                    //如果已经对这个关系检测过，则不需要加入到队列中。
                    if (doneList.Contains(relationView)) continue;

                    queue.Enqueue(relationView);
                }

                foreach (LogicalView child in current.ChildrenViews)
                {
                    queue.Enqueue(child);
                }
            }

            return null;
        }
    }
}