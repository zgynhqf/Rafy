/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：????????
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 ????????
 * 添加 CheckingMode 属性。 胡庆访 20110810
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using OEA.MetaModel;
using OEA.Library;
using OEA.MetaModel.View;
using System.ComponentModel;

namespace OEA
{
    public interface IListObjectView : IObjectView, IActiveSelectionContext
    {
        /// <summary>
        /// IListObjectView对应的数据是一个列表
        /// </summary>
        new EntityList Data { get; set; }

        /// <summary>
        /// 是否这个 ListObjectView 正在被显示在下拉框中
        /// </summary>
        ShowInWhere ShowInWhere { get; set; }

        /// <summary>
        /// 是否是显示一个树型的列表
        /// </summary>
        bool IsShowingTree { get; }

        /// <summary>
        /// 当前视图是否处于只读的状态。
        /// </summary>
        bool IsReadOnly { get; set; }

        event EventHandler Refreshed;

        /// <summary>
        /// 如果当前的ListObject正在显示一颗树，
        /// 则可以使用这个方法来根据rootPid绑定数据。
        /// </summary>
        /// <param name="rootPid">
        /// 如果这个值不是null，则这个值表示绑定的所有根节点的父id。
        /// </param>
        void BindData(int? rootPid);

        /// <summary>
        /// 列表视图中的控件的 “Check选择” 模式
        /// </summary>
        CheckingMode CheckingMode { get; set; }

        Predicate<Entity> Filter { get; set; }
    }

    /// <summary>
    /// 列表视图中的控件的 “Check选择” 模式
    /// </summary>
    public enum CheckingMode
    {
        /// <summary>
        /// 未开启 CheckBox 选择
        /// </summary>
        None,

        /// <summary>
        /// 使用 CheckBox 进行选择，并双向绑定到数据的 IsSelected 属性上。
        /// </summary>
        CheckingViewModel,

        /// <summary>
        /// 使用 CheckBox 进行选择，并双向绑定到行的 IsSelected 属性上。
        /// </summary>
        CheckingRow
    }

    /// <summary>
    /// CheckingRow 模式下的级联勾选行为模式
    /// </summary>
    public enum CheckingRowCascade
    {
        None = 0,

        /// <summary>
        /// 级联把父节点勾选上
        /// </summary>
        CascadeParent = 1,

        /// <summary>
        /// 级联把所有孩子节点勾选上
        /// </summary>
        CascadeChildren = 2
    }
}
