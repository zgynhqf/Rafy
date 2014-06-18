/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120829 20:14
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120829 20:14
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rafy.Domain;
using Rafy.ManagedProperty;

namespace Rafy.WPF.Controls
{
    public class RafyTreeGrid : TreeGrid
    {
        static RafyTreeGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RafyTreeGrid), new FrameworkPropertyMetadata(typeof(RafyTreeGrid)));
        }

        /// <summary>
        /// 是否允许懒加载实体节点。
        /// </summary>
        public bool EnabledDataLazyLoading { get; set; }

        /// <summary>
        /// 通过属性查找对应的列
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public TreeColumn FindColumnByProperty(IManagedProperty property)
        {
            foreach (var column in this.Columns)
            {
                var c = column as TreeColumn;
                if (c != null && c.Meta != null && c.Meta.PropertyMeta.ManagedProperty == property)
                {
                    return c;
                }
            }

            return null;
        }

        public override object GetId(object item)
        {
            return (item as Entity).Id;
        }

        /// <summary>
        /// for LazyTreeNodeRelation
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected override bool HasChildItems(object parent)
        {
            var e = parent as Entity;
            return e.SupportTree && e.TreeChildren.IsLoaded && e.TreeChildren.Count > 0;

            //var anyChildCanShow = this.FindChildrenNodes(parent).Any(c => this.PassFilter(c));
            //if (anyChildCanShow) { return true; }

            ////如果没有返回True，再检查子对象
            //var treeChildPropertyInfo = this.SearchMeta(parent).TreeChildPropertyInfo;
            //if (treeChildPropertyInfo != null)
            //{
            //    var children = parent.GetPropertyValue<IList<Entity>>(treeChildPropertyInfo);
            //    return children.Any(c => this.PassFilter(c));
            //}
        }

        public override IEnumerable<object> GetChildItems(object parent)
        {
            //检查主对象
            var e = parent as Entity;
            if (e.SupportTree)
            {
                if (e.TreeChildren.IsLoaded)
                {
                    return e.TreeChildren;
                }
                else if (EnabledDataLazyLoading)
                {
                    e.TreeChildren.Load();
                    return e.TreeChildren;
                }
            }

            return Enumerable.Empty<object>();

            //foreach (var child in this.FindChildrenNodes(parent))
            //{
            //    if (this.PassFilter(child)) { list.Add(child); }
            //}

            ////判断子对象
            //var treeChildPropertyInfo = this.SearchMeta(parent).TreeChildPropertyInfo;
            //if (treeChildPropertyInfo != null)
            //{
            //    var childList = parent.GetPropertyValue<IList>(treeChildPropertyInfo);
            //    if (childList.Count > 0)
            //    {
            //        foreach (Entity child in childList)
            //        {
            //            //如果孩子对象也是树型对象，则只显示第一层孩子对象。
            //            var isFirstLayerChild = !child.SupportTree || child.TreePId == null;
            //            if (isFirstLayerChild && this.PassFilter(child)) { list.Add(child); }
            //        }
            //    }
            //}
        }

        public override object GetParentItem(object item)
        {
            var entity = item as Entity;
            if (entity.IsTreeParentLoaded || this.EnabledDataLazyLoading)
            {
                return entity.TreeParent;
            }
            return null;
        }

        protected override object GetPId(object item)
        {
            return (item as Entity).TreePId;
        }

        //protected override void OnCommitEditCommandExecuted(ExecutedRoutedEventArgs e)
        //{
        //    //((e.OriginalSource as MTTGCell).Column as TreeColumn).CommitEdit();
        //}

        ///// <summary>
        ///// 提交当前的编辑状态，更新绑定的数据源。
        ///// </summary>
        //internal void CommitEdit()
        //{
        //    var editor = this.Editor;
        //    var dp = editor.GetBindingProperty();
        //    if (dp != null)
        //    {
        //        //需要把焦点从这个控件上移除，否则用户输入的值还没有同步到控件上，这时 UpdateSource 无用。
        //        //象 TextBox、NumericUpDown 这些控件都是需要焦点移除后才起使用的。
        //        //（NumericUpDown 在输入模式下是这样，内部其实也是 TextBox）
        //        var ctrl = editor.Control;
        //        ctrl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

        //        var exp = ctrl.GetBindingExpression(dp);
        //        if (exp != null) { exp.UpdateSource(); }
        //    }
        //}

        //以下代码用于当 TreeParent 不能保证关系时，直接在列表中查找对应元素。
        ///// <summary>
        ///// 以不依赖树型属性的方式查找父元素
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //private Entity FindParentNode(Entity node)
        //{
        //    if (!node.SupportTree) return null;

        //    var parentList = node.ParentList;
        //    if (parentList == null) return null;

        //    var id = node.TreePId;
        //    return parentList.FirstOrDefault(n => GetId(n) == id);
        //}

        ///// <summary>
        ///// 以不依赖树型属性的方式查找树型子元素
        ///// </summary>
        ///// <param name="node"></param>
        ///// <returns></returns>
        //private IEnumerable<Entity> FindChildrenNodes(Entity node)
        //{
        //    if (!node.SupportTree) return new Entity[0];

        //    var parentList = node.ParentList;
        //    if (parentList == null) return new Entity[0];

        //    var id = GetId(node);

        //    var result = parentList.Where(e => e.TreePId == id);

        //    //if (parentList.SupportOrder) { result = result.OrderBy(e => e.OrderNo); }

        //    return result;
        //}
    }
}