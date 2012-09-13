/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111205
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111205
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls.Primitives;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// GridTreeView 控件中的列定义
    /// </summary>
    public class GridTreeViewColumn : GridViewColumn
    {
        #region 自动列宽

        /***********************GridView 自动列宽的原理及实现*************************
         * 
         * GridView 自动列宽的原理：
         * 调用 RequestDataWidth 方法之后，列的状态被设置为 Init，表明需要自动列宽功能。
         * 接下来到达界面测量过程时，先是 GridTreeViewHeaderRowPresenter 发生它的 MeasureOverride 方法，其中主要完成对最小动态列宽的限制；
         * 然后是每一行的 GridTreeViewRowPresenter 进行测量，其会根据每一行的对应列的宽度来调用本类的 EnsureWidth 方法，来保证所有行的列都使用同一个列宽。
         * 
         * 代码实现：
         * 相关的代码可以参见：GridTreeViewHeaderRowPresenter.MeasureOverride 以及 GridTreeViewRowPresenter.MeasureOverrideReflected 方法
         * 
         * 该部分代码主要是为了重新实现基类 MeasureOverride 方法中 EnsureWidth 方法的逻辑：
         * 由于我们需要 EnsureWidth 方法中考虑 GridTreeViewColumn.MinDataWidth/MaxDataWidth 两个属性，
         * 所以我们需要重写 EnsureWidth 方法，但是基类并没有提供这个扩展点，所以无奈之下，只能使用复制代码 + 反射的方案：
         * 也就是说：把整个 base.MeasureOverride 的方法复制出来，私有属性使用反射（尽量缓存），同时，
         * 重写新的 GridTreeViewColumn.EnsureWidth 方法，使其最后的值处于 GridTreeViewColumn.MinDataWidth/MaxDataWidth 之间。
         * 
        **********************************************************************/

        private bool _maxDataWidthEnabled = true;

        /// <summary>
        /// 重新计算该列的自动列宽
        /// </summary>
        /// <param name="maxDataWidthEnabled">
        /// 是否启用最大自动列宽限制（当双击列时，不需要限制）
        /// </param>
        public void RequestDataWidth(bool maxDataWidthEnabled = true)
        {
            /**********************************************************************
             * 
             * 本方法的代码类似于 GridViewColumnHeader.OnGripperDoubleClicked
             * 
             * Width 为 double.NaN 时表示需要动态列宽，但是这时可能已经计算完成了，
             * 所以需要先把 Width 设置为一个无关的值，再设置为 NaN，这样，它的 State 就会变为 Init 了。
             * 
            **********************************************************************/

            if (double.IsNaN(this.Width))
            {
                this.Width = this.ActualWidth;
            }
            this.Width = double.NaN;

            this._maxDataWidthEnabled = maxDataWidthEnabled;
        }

        internal double EnsureWidth(double width)
        {
            var changed = false;
            var value = this.DesiredWidthReflected;

            if (value < width)
            {
                changed = true;
                value = width;
            }

            //限制最大自动宽度和最小自动宽度
            if (this._maxDataWidthEnabled)
            {
                var minMaxWidth = value;
                var maxWidth = this.MaxWidth;
                if (!double.IsNaN(maxWidth)) minMaxWidth = maxWidth;
                var maxDataWidth = this.MaxDataWidth;
                if (!double.IsNaN(maxDataWidth)) minMaxWidth = Math.Min(minMaxWidth, maxDataWidth);

                if (value > minMaxWidth)
                {
                    value = minMaxWidth;
                    changed = true;
                }
            }

            var maxMinWidth = value;
            var minWidth = this.MinWidth;
            if (!double.IsNaN(minWidth)) maxMinWidth = minWidth;
            var minDataWidth = this.MinDataWidth;
            if (!double.IsNaN(minDataWidth)) maxMinWidth = Math.Max(maxMinWidth, minDataWidth);
            if (value < maxMinWidth)
            {
                value = maxMinWidth;
                changed = true;
            }

            if (changed)
            {
                this.DesiredWidthReflected = value;
            }

            return value;
        }

        #region MinDataWidthProperty

        public static readonly DependencyProperty MinDataWidthProperty = DependencyProperty.Register(
            "MinDataWidth", typeof(double), typeof(GridTreeViewColumn),
            new PropertyMetadata(100d)
            );

        /// <summary>
        /// 动态计算列宽时，不应该低于这个值。默认为 100。
        /// </summary>
        public double MinDataWidth
        {
            get { return (double)this.GetValue(MinDataWidthProperty); }
            set { this.SetValue(MinDataWidthProperty, value); }
        }

        #endregion

        #region MaxDataWidthProperty

        public static readonly DependencyProperty MaxDataWidthProperty = DependencyProperty.Register(
            "MaxDataWidth", typeof(double), typeof(GridTreeViewColumn),
            new PropertyMetadata(400d)
            );

        /// <summary>
        /// 动态计算列宽时，不应该超过这个值。默认为 400。
        /// </summary>
        public double MaxDataWidth
        {
            get { return (double)this.GetValue(MaxDataWidthProperty); }
            set { this.SetValue(MaxDataWidthProperty, value); }
        }

        #endregion

        #region 基类的几个私有属性

        /// <summary>
        /// 当前的列的列宽的计算状态。
        /// 
        /// 基类的 State 属性
        /// </summary>
        internal ColumnMeasureState StateReflected
        {
            get { return (ColumnMeasureState)GridViewInternal.StateProperty.GetValue(this, null); }
            set { GridViewInternal.StateProperty.SetValue(this, (int)value, null); }
        }

        /// <summary>
        /// 基类的 DesiredWidth 属性
        /// </summary>
        internal double DesiredWidthReflected
        {
            get { return (double)GridViewInternal.DesiredWidthProperty.GetValue(this, null); }
            set { GridViewInternal.DesiredWidthProperty.SetValue(this, value, null); }
        }

        /// <summary>
        /// 基类的 ActualIndex 属性
        /// </summary>
        internal int ActualIndexReflected
        {
            get { return (int)GridViewInternal.ActualIndexProperty.GetValue(this, null); }
        }

        #endregion

        #endregion

        #region 宽度限制

        #region MinWidthProperty

        public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register(
            "MinWidth", typeof(double), typeof(GridTreeViewColumn),
            new PropertyMetadata(40d)
            );

        /// <summary>
        /// 最小宽度。
        /// 如果被设置为 double.NaN 则表示不限制。默认为 40。
        /// </summary>
        public double MinWidth
        {
            get { return (double)this.GetValue(MinWidthProperty); }
            set { this.SetValue(MinWidthProperty, value); }
        }

        #endregion

        #region MaxWidthProperty

        public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register(
            "MaxWidth", typeof(double), typeof(GridTreeViewColumn),
            new PropertyMetadata(double.NaN)
            );

        /// <summary>
        /// 最大宽度。
        /// 如果被设置为 double.NaN 则表示不限制。默认为 double.NaN。
        /// </summary>
        public double MaxWidth
        {
            get { return (double)this.GetValue(MaxWidthProperty); }
            set { this.SetValue(MaxWidthProperty, value); }
        }

        #endregion

        /**********************************************************************
        * 
        * 部分代码来自：[WPF疑难] 如何限定ListView列宽度
        * http://www.cnblogs.com/zhouyinhui/archive/2008/06/03/1213030.html
        * 
        **********************************************************************/

        /// <summary>
        /// 判断某列是否已经被规定了宽度。
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public bool IsRanged()
        {
            return !double.IsNaN(this.MinWidth) || !double.IsNaN(this.MaxWidth);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == WidthProperty)
            {
                if (this.IsRanged())
                {
                    this.SetWidthToBounds();
                }
            }
        }

        private void SetWidthToBounds()
        {
            var minWidth = this.MinWidth;
            var maxWidth = this.MaxWidth;

            // invalid case
            if (!double.IsNaN(minWidth) && !double.IsNaN(maxWidth) && minWidth > maxWidth) return;

            //重新设置宽度
            if (!double.IsNaN(minWidth) && this.Width <= minWidth)
            {
                this.Width = minWidth;
            }
            else if (!double.IsNaN(maxWidth) && this.Width >= maxWidth)
            {
                this.Width = maxWidth;
            }
        }

        #endregion

        #region 列的可见性

        internal GridTreeViewColumnCollection _ownerCollection;

        internal int _oldIndexAsUnvisible;

        #region IsVisible DependencyProperty

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.Register(
            "IsVisible", typeof(bool), typeof(GridTreeViewColumn),
            new PropertyMetadata(true, (d, e) => (d as GridTreeViewColumn).OnIsVisibleChanged(e))
            );

        public bool IsVisible
        {
            get { return (bool)this.GetValue(IsVisibleProperty); }
            set { this.SetValue(IsVisibleProperty, value); }
        }

        private void OnIsVisibleChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this._ownerCollection == null) return;
            var value = (bool)e.NewValue;
            this._ownerCollection.SetVisibility(this, value);
        }

        #endregion

        #endregion
    }
}