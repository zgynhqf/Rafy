/*******************************************************
 * 
 * 作者：hardcodet
 * 创建时间：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：2.0.0
 * 
 * 历史记录：
 * 创建文件 hardcodet 2008
 * 2.0 胡庆访 20120911 14:42
 * 
*******************************************************/

using System.Collections.Generic;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 封装树的布局信息（选中、展开的结点集合。）
    /// </summary>
    public class TreeLayout
    {
        public TreeLayout()
        {
            this.ExpandedNodeIds = new List<int>();
        }

        /// <summary>
        /// The ID of the selected item, if any. Defaults to null
        /// (no node is selected).
        /// </summary>
        public int SelectedItemId { get; set; }

        /// <summary>
        /// A list of expanded nodes.
        /// </summary>
        public List<int> ExpandedNodeIds { get; private set; }

        /// <summary>
        /// Checks whether a given node is supposed to be
        /// expanded or not.
        /// </summary>
        /// <param name="nodeId">The ID of the processed node.</param>
        /// <returns>True if <paramref name="nodeId"/> is contained
        /// in the list of expanded nodes.</returns>
        public bool IsNodeExpanded(int nodeId)
        {
            return this.ExpandedNodeIds.Contains(nodeId);
        }
    }
}