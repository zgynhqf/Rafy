/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111206
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111206
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.ComponentModel;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// GridTreeView 控件中的列头显示控件
    /// </summary>
    public class GridTreeViewColumnHeader : GridViewColumnHeader
    {
        static GridTreeViewColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GridTreeViewColumnHeader),
                new FrameworkPropertyMetadata(typeof(GridTreeViewColumnHeader))
                );
        }

        public new GridTreeViewColumn Column
        {
            get { return base.Column as GridTreeViewColumn; }
        }

        private Thumb _headerGripper;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._headerGripper = this.Template.FindName("PART_HeaderGripper", this) as Thumb;
            if (this._headerGripper != null)
            {
                this._headerGripper.PreviewMouseMove += _headerGripper_PreviewMouseMove;
                this._headerGripper.PreviewMouseDoubleClick += _headerGripper_PreviewMouseDoubleClick;
                this._headerGripper.Cursor = Cursors.SizeWE;
            }
        }

        private void _headerGripper_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var column = this.Column;
            if (column != null)
            {
                //这里需要重新计算动态宽度，同时不受最大宽度的限制。
                column.RequestDataWidth(false);

                //处理掉这个事件，使得基类的监听无效，基类的事件处理函数不会发生。
                e.Handled = true;
            }
        }

        private void _headerGripper_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            /*********************** 代码块解释 *********************************
             * 
             * 以下代码实现：在鼠标移动时更改图标。
             * 
            **********************************************************************/

            var gridViewColumn = this.Column;
            if (gridViewColumn == null) return;

            // check range column bounds
            if (this._headerGripper.IsMouseCaptured && gridViewColumn.IsRanged())
            {
                var minWidth = gridViewColumn.MinWidth;
                var maxWidth = gridViewColumn.MaxWidth;

                if (!double.IsNaN(minWidth) && !double.IsNaN(maxWidth) && minWidth > maxWidth) return; // invalid case

                if (!double.IsNaN(minWidth) && gridViewColumn.Width <= minWidth)
                {
                    this._headerGripper.Cursor = Cursors.No;
                }
                else if (!double.IsNaN(maxWidth) && gridViewColumn.Width >= maxWidth)
                {
                    this._headerGripper.Cursor = Cursors.No;
                }
                else
                {
                    this._headerGripper.Cursor = Cursors.SizeWE;
                }
            }
        }
    }
}