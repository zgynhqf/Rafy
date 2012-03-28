/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111123
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111123
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Windows.Automation.Peers;
using System.Collections.Specialized;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 一个特殊的 TreeView，其中的每一项都是一个表格行。
    /// </summary>
    public class GridTreeView : TreeView
    {
        static GridTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridTreeView), new FrameworkPropertyMetadata(typeof(GridTreeView)));
        }

        public GridTreeView()
        {
            this.Columns = new GridTreeViewColumnCollection();
        }

        public MultiTypesTreeGrid TreeGrid { get; internal set; }

        /// <summary>
        /// 设置或获取在表格模式下（<see cref="OnlyGridMode"/> = true）是否已经打开了 UI 虚拟化。
        /// </summary>
        internal bool IsGridVirtualizing
        {
            get { return VirtualizingStackPanel.GetIsVirtualizing(this); }
            set
            {
                /*********************** 代码块解释 *********************************
                 * 设置该属性会触发 Style 中的 Trigger 来替换 ItemsPanel。
                 * （该方案与 TreeView 默认模板模板中的方案一致）
                 * 
                 * 关于 UIV，参见：http://www.cnblogs.com/zgynhqf/archive/2011/12/12/2284335.html
                **********************************************************************/

                VirtualizingStackPanel.SetIsVirtualizing(this, value);
            }
        }

        #region ColumnsProperty

        private static readonly DependencyPropertyKey ColumnsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Columns", typeof(GridTreeViewColumnCollection), typeof(GridTreeView), new PropertyMetadata((d, e) => (d as GridTreeView).OnColumnsChanged(e))
            );

        public static readonly DependencyProperty ColumnsProperty = ColumnsPropertyKey.DependencyProperty;

        /// <summary> 
        /// 所有显示的列的列表
        /// </summary>
        public GridTreeViewColumnCollection Columns
        {
            get { return (GridTreeViewColumnCollection)this.GetValue(ColumnsProperty); }
            internal set { this.SetValue(ColumnsPropertyKey, value); }
        }

        private void OnColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldColumns = e.OldValue as GridTreeViewColumnCollection;
            if (oldColumns != null) { oldColumns.Owner = null; }

            var newColumns = e.NewValue as GridTreeViewColumnCollection;
            if (newColumns != null) { newColumns.Owner = this; }
        }

        #endregion

        #region DataProperty

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(IList), typeof(GridTreeView));

        /// <summary>
        /// GridTreeView上显示的ItemScource。
        /// 
        /// 加这个属性的原因是MultiTypesTreeGrid的设计上，不使用GridTreeView的ItemSource属性，而是直接控制Items。
        /// 所以这里使用这个属性保存ItemSource里面的值。
        /// 
        /// 主要用于Xaml绑定。
        /// </summary>
        public IList Data
        {
            get { return (IList)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        #endregion

        #region HasRowNoProperty

        public static readonly DependencyProperty HasRowNoProperty = DependencyProperty.Register(
            "HasRowNo", typeof(bool), typeof(GridTreeView),
            new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否需要显示行号。
        /// </summary>
        public bool HasRowNo
        {
            get { return (bool)this.GetValue(HasRowNoProperty); }
            set { this.SetValue(HasRowNoProperty, value); }
        }

        #endregion

        #region OnlyGridModeProperty

        public static readonly DependencyProperty OnlyGridModeProperty = DependencyProperty.Register(
            "OnlyGridMode", typeof(bool), typeof(GridTreeView)
            );

        /// <summary>
        /// 表示是否处于 “简单表格” 模式下。
        /// 此模式下，没有层级节点。
        /// </summary>
        public bool OnlyGridMode
        {
            get { return (bool)this.GetValue(OnlyGridModeProperty); }
            set { this.SetValue(OnlyGridModeProperty, value); }
        }

        #endregion

        protected override DependencyObject GetContainerForItemOverride()
        {
            throw new NotSupportedException("Container 由 MultiTypesTreeGrid 控件内部生成，此方法无效。");
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GridTreeViewRow;
        }

        #region 支持自动化

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GridTreeViewAutomationPeer(this);
        }

        #endregion
    }

    public class GridTreeViewAutomationPeer : TreeViewAutomationPeer
    {
        private GridTreeView _owner;

        public GridTreeViewAutomationPeer(GridTreeView owner)
            : base(owner)
        {
            this._owner = owner;
        }

        protected override string GetNameCore()
        {
            return this._owner.TreeGrid.RootEntityViewMeta.Label;
        }
    }
}
