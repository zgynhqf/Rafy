using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF.Editors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Automation;

namespace OEA.Module.WPF.Controls
{

    /// <summary>
    /// MTTG 控件中对应的每一个单元格对象
    /// 
    /// 实现：
    /// 行对象的查找直接使用可视树即可，而列对象则需要在构造的时候赋值（模板中完成）。
    /// </summary>
    public class MTTGCell : ContentControl
    {
        static MTTGCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MTTGCell), new FrameworkPropertyMetadata(typeof(MTTGCell)));
        }

        /// <summary>
        /// 将 element 包装在 MTTGCell 中返回。
        /// 如果是直接写模板，需要把 Column 的值赋上。
        /// </summary>
        /// <param name="element"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public static FrameworkElementFactory Wrap(FrameworkElementFactory element, TreeColumn column)
        {
            var cellContainer = new FrameworkElementFactory(typeof(MTTGCell));

            cellContainer.AppendChild(element);

            cellContainer.SetValue(MTTGCell.ColumnProperty, column);

            return cellContainer;
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            this.Row.AddCell(this);
        }

        #region ColumnProperty

        public static readonly DependencyProperty ColumnProperty = DependencyProperty.Register(
            "Column", typeof(TreeColumn), typeof(MTTGCell)
            );

        /// <summary>
        /// 对应的列
        /// </summary>
        public TreeColumn Column
        {
            get { return (TreeColumn)this.GetValue(ColumnProperty); }
        }

        #endregion

        #region RowProperty

        private GridTreeViewRow _row;

        /// <summary>
        /// 所对应的行
        /// </summary>
        public GridTreeViewRow Row
        {
            get
            {
                if (this._row == null)
                {
                    this._row = this.GetVisualParent<GridTreeViewRow>();
                }

                return this._row;
            }
        }

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            //左键点击时，通知树型控件需要进入编辑状态。
            this.Column.TryBeginEdit(this, e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            //在编辑控件按 Tab 切换到下一列
            if (e.Key == Key.Tab)
            {
                this.Column.EditNextColumnOnTabKey(this, e);
            }
        }

        #region UI Automation

        //*****************************
        //MTTGCellAutomationPeer 类被 UIAutomation 调用 Invoke 时，实现以下功能：
        //当前单元格进入编辑状态
        //*****************************

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new MTTGCellAutomationPeer(this);
        }

        private class MTTGCellAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
        {
            public MTTGCellAutomationPeer(MTTGCell owner) : base(owner) { }

            public override object GetPattern(PatternInterface patternInterface)
            {
                //支持 Invoke
                if (patternInterface == PatternInterface.Invoke) { return this; }

                return base.GetPattern(patternInterface);
            }

            void IInvokeProvider.Invoke()
            {
                Action a = () =>
                {
                    var cell = this.Owner.CastTo<MTTGCell>();
                    var column = cell.Column;
                    if (column != null)
                    {
                        column.Owner.TryEditCell(cell);
                    }
                };
                this.Dispatcher.BeginInvoke(DispatcherPriority.Input, a);
            }
        }
        #endregion
    }
}