/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121011 10:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121011 10:54
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGridHeaderRowPresenter 及可能的合计行的基类。
    /// </summary>
    public abstract class TreeGridRowPresenterBase : FrameworkElement, IWeakEventListener
    {
        internal const double c_EndPadding = 2.0;

        /// <summary>
        /// 每个列所需要的宽度列表。
        /// 
        /// 注意索引是列的 ActualIndex。
        /// </summary>
        private List<double> _desiredWidthList;

        /// <summary>
        /// 是否需要重新构造本行中的可视树元素。
        /// </summary>
        internal bool NeedUpdateVisualTree = true;

        private UIElementCollection _uiChildren;

        internal TreeGridRowPresenterBase()
        {
            this._uiChildren = new UIElementCollection(this, this);
        }

        /// <summary>
        /// 每个列所需要的宽度列表。
        /// 
        /// 注意索引是列的 ActualIndex。
        /// </summary>
        internal List<double> DesiredWidthList
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get { return this._desiredWidthList; }
        }

        /// <summary>
        /// 内部生成的所有元素。
        /// </summary>
        internal UIElementCollection UIChildren
        {
            get { return this._uiChildren; }
        }

        #region 重写可视树孩子列表。

        protected override IEnumerator LogicalChildren
        {
            get { return this._uiChildren.GetEnumerator(); }
        }

        protected override int VisualChildrenCount
        {
            get { return this._uiChildren.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this._uiChildren[index];
        }

        #endregion

        #region 连接到表格控件，并监听列的变化

        private TreeGrid _treeGrid;
        private TreeGridColumnCollection _columns;

        protected TreeGrid TreeGrid
        {
            get { return _treeGrid; }
            //get { return base.TemplatedParent as TreeGrid; }
        }

        internal TreeGridColumnCollection Columns
        {
            get { return _columns; }
        }

        /// <summary>
        /// 子类在 Measure 时，调用此方法来连接到 TreeGrid，并同步一些必要的属性。
        /// </summary>
        internal void ConnectToGrid()
        {
            if (_treeGrid == null)
            {
                _treeGrid = this.GetVisualParent<TreeGrid>();
                if (_treeGrid != null)
                {
                    _columns = _treeGrid.Columns;
                    InternalCollectionChangedEventManager.AddListener(_columns, this);
                }
            }
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(InternalCollectionChangedEventManager))
            {
                this.ColumnCollectionChanged(sender, e as NotifyCollectionChangedEventArgs);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 当本行对应的 TreeGridColumnCollection 列集合变化时，发生此事件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="arg"></param>
        private void ColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs arg)
        {
            var columnArgs = arg as TreeGridColumnCollectionChangedEventArgs;
            if (columnArgs != null)
            {
                var isPresenterVisualReady = base.IsInitialized && !this.NeedUpdateVisualTree;
                if (isPresenterVisualReady)
                {
                    if (columnArgs.Column != null)
                    {
                        this.OnColumnPropertyChanged(columnArgs.Column, columnArgs.PropertyName);
                    }
                    else
                    {
                        this.OnColumnCollectionChanged(columnArgs);
                    }
                }
            }
        }

        /// <summary>
        /// 当前 Columns 集合发生变更时，执行此回调。
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnColumnCollectionChanged(TreeGridColumnCollectionChangedEventArgs e)
        {
            TreeGridHelper.UpdateDesiredWidthListOnColumnChanged(ref this._desiredWidthList, e);
        }

        /// <summary>
        /// 当某个 TreeGridColumn 的属性变更时，会发生此事件。
        /// </summary>
        /// <param name="column"></param>
        /// <param name="propertyName"></param>
        internal virtual void OnColumnPropertyChanged(TreeGridColumn column, string propertyName)
        {
            if (column.StableIndex >= 0)
            {
                if (TreeGridColumn.WidthProperty.Name == propertyName || TreeGridColumn.c_ActualWidthName == propertyName)
                {
                    base.InvalidateMeasure();
                }
            }
        }

        #endregion

        /// <summary>
        /// 保证 _desiredWidthList 与 columns 长度一致。
        /// </summary>
        internal void EnsureDesiredWidthList()
        {
            TreeGridHelper.EnsureDesiredWidthList(ref this._desiredWidthList, this.Columns);
        }

        private ScrollViewer _scrollViewer;

        internal double GetHorizontalOffsetForArrange()
        {
            //在虚拟化的情况下，ScrollViewer 不再自动安排这个列头行，
            //我们需要返回 ScrollViewer 的 HOffset 来实现 Arrange。
            if (_treeGrid != null && _treeGrid.IsVirtualizing)
            {
                this.ConnectToScrollViewer();

                if (_scrollViewer != null) { return _scrollViewer.HorizontalOffset; }
            }

            return 0;
        }

        private void ConnectToScrollViewer()
        {
            if (_scrollViewer == null)
            {
                for (var parent = VisualTreeHelper.GetParent(this); parent != null && !(parent is TreeGrid); parent = VisualTreeHelper.GetParent(parent))
                {
                    if (parent is ScrollViewer)
                    {
                        _scrollViewer = parent as ScrollViewer;
                        _scrollViewer.ScrollChanged += (o, e) =>
                        {
                            if (e.HorizontalChange != 0)
                            {
                                this.InvalidateArrange();
                            }
                        };
                        break;
                    }
                }
            }
        }
    }
}