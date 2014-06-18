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
using System.Runtime;
using System.Windows.Automation.Peers;
using System.Security;
using System.Reflection;
using System.Security.Permissions;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// TreeGrid 控件中的列头显示控件
    /// 
    /// From GridViewColumnHeader
    /// </summary>
    public class TreeGridColumnHeader : ButtonBase
    {
        #region 静态字段及构造函数

        #region 常量

        private const int c_SPLIT = 100;
        private const int c_SPLITOPEN = 101;
        private const string HeaderGripperTemplateName = "PART_HeaderGripper";
        private const string FloatingHeaderCanvasTemplateName = "PART_FloatingHeaderCanvas";

        #endregion

        static TreeGridColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(typeof(TreeGridColumnHeader)));
            ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));
            ContentControl.ContentTemplateSelectorProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));
            FrameworkElement.StyleProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));
            FrameworkElement.ContextMenuProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));
            FrameworkElement.ToolTipProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(PropertyChanged)));
            UIElement.FocusableProperty.OverrideMetadata(typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.False));

            ColumnPropertyKey = DependencyProperty.RegisterReadOnly("Column", typeof(TreeGridColumn), typeof(TreeGridColumnHeader), null);
            ColumnProperty = ColumnPropertyKey.DependencyProperty;
            RolePropertyKey = DependencyProperty.RegisterReadOnly("Role", typeof(TreeGridColumnHeaderRole), typeof(TreeGridColumnHeader), new FrameworkPropertyMetadata(TreeGridColumnHeaderRole.Normal));
            RoleProperty = RolePropertyKey.DependencyProperty;
        }

        #endregion

        #region Column DependencyProperty

        internal static readonly DependencyPropertyKey ColumnPropertyKey;
        public static readonly DependencyProperty ColumnProperty;

        public TreeGridColumn Column
        {
            get
            {
                return (TreeGridColumn)base.GetValue(TreeGridColumnHeader.ColumnProperty);
            }
        }

        #endregion

        #region Role DependencyProperty

        internal static readonly DependencyPropertyKey RolePropertyKey;

        public static readonly DependencyProperty RoleProperty;

        [Category("Behavior")]
        public TreeGridColumnHeaderRole Role
        {
            get
            {
                return (TreeGridColumnHeaderRole)base.GetValue(TreeGridColumnHeader.RoleProperty);
            }
        }

        #endregion

        #region SortDirection DependencyProperty

        private static readonly DependencyPropertyKey SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly(
            "SortDirection", typeof(TreeGridColumnSortDirection), typeof(TreeGridColumnHeader),
            new PropertyMetadata(TreeGridColumnSortDirection.None)
            );

        public static readonly DependencyProperty SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;

        /// <summary>
        /// 本控件的排序显示方向。
        /// </summary>
        /// 主要用于控制界面中上下箭头的显示。
        public TreeGridColumnSortDirection SortDirection
        {
            get { return (TreeGridColumnSortDirection)this.GetValue(SortDirectionProperty); }
            internal set { this.SetValue(SortDirectionPropertyKey, value); }
        }

        #endregion

        #region Flags

        private Flags _flags;

        /// <summary>
        /// 使用某个值来更新指定的依赖属性。
        /// </summary>
        /// <param name="dp"></param>
        /// <param name="value"></param>
        internal void UpdateProperty(DependencyProperty dp, object value)
        {
            var flag = Flags.None;
            if (!this.IsInternalGenerated)
            {
                Flags flag2;
                PropertyToFlags(dp, out flag2, out flag);
                if (this.GetFlag(flag2))
                {
                    return;
                }
                this.SetFlag(flag, true);
            }
            if (value != null)
            {
                base.SetValue(dp, value);
            }
            else
            {
                base.ClearValue(dp);
            }
            this.SetFlag(flag, false);
        }

        private bool GetFlag(Flags flag)
        {
            return (this._flags & flag) == flag;
        }

        private void SetFlag(Flags flag, bool set)
        {
            if (set)
            {
                this._flags |= flag;
                return;
            }
            this._flags &= ~flag;
        }

        private static void PropertyToFlags(DependencyProperty dp, out Flags flag, out Flags ignoreFlag)
        {
            if (dp == FrameworkElement.StyleProperty)
            {
                flag = Flags.StyleSetByUser;
                ignoreFlag = Flags.IgnoreStyle;
                return;
            }
            if (dp == ContentControl.ContentTemplateProperty)
            {
                flag = Flags.ContentTemplateSetByUser;
                ignoreFlag = Flags.IgnoreContentTemplate;
                return;
            }
            if (dp == ContentControl.ContentTemplateSelectorProperty)
            {
                flag = Flags.ContentTemplateSelectorSetByUser;
                ignoreFlag = Flags.IgnoreContentTemplateSelector;
                return;
            }
            if (dp == ContentControl.ContentStringFormatProperty)
            {
                flag = Flags.ContentStringFormatSetByUser;
                ignoreFlag = Flags.IgnoreContentStringFormat;
                return;
            }
            if (dp == FrameworkElement.ContextMenuProperty)
            {
                flag = Flags.ContextMenuSetByUser;
                ignoreFlag = Flags.IgnoreContextMenu;
                return;
            }
            if (dp == FrameworkElement.ToolTipProperty)
            {
                flag = Flags.ToolTipSetByUser;
                ignoreFlag = Flags.IgnoreToolTip;
                return;
            }
            flag = (ignoreFlag = Flags.None);
        }

        [Flags]
        private enum Flags
        {
            None = 0,
            StyleSetByUser = 1,
            IgnoreStyle = 2,
            ContentTemplateSetByUser = 4,
            IgnoreContentTemplate = 8,
            ContentTemplateSelectorSetByUser = 16,
            IgnoreContentTemplateSelector = 32,
            ContextMenuSetByUser = 64,
            IgnoreContextMenu = 128,
            ToolTipSetByUser = 256,
            IgnoreToolTip = 512,
            SuppressClickEvent = 1024,
            IsInternalGenerated = 2048,
            IsAccessKeyOrAutomation = 4096,
            ContentStringFormatSetByUser = 8192,
            IgnoreContentStringFormat = 16384
        }

        #endregion

        #region 属性

        internal TreeGridColumnHeader PreviousVisualHeader
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._previousHeader;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._previousHeader = value;
            }
        }

        internal bool SuppressClickEvent
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetFlag(Flags.SuppressClickEvent);
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SetFlag(Flags.SuppressClickEvent, value);
            }
        }

        internal TreeGridColumnHeader FloatSourceHeader
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._srcHeader;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._srcHeader = value;
            }
        }

        internal bool IsInternalGenerated
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.GetFlag(Flags.IsInternalGenerated);
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.SetFlag(Flags.IsInternalGenerated, value);
            }
        }

        private bool IsAccessKeyOrAutomation
        {
            get
            {
                return this.GetFlag(Flags.IsAccessKeyOrAutomation);
            }
            set
            {
                this.SetFlag(Flags.IsAccessKeyOrAutomation, value);
            }
        }

        private double ColumnActualWidth
        {
            get
            {
                if (this.Column == null)
                {
                    return base.ActualWidth;
                }
                return this.Column.ActualWidth;
            }
        }

        #endregion

        #region override parents

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = (TreeGridColumnHeader)d;
            if (!sender.IsInternalGenerated)
            {
                Flags flag;
                Flags flag2;
                PropertyToFlags(e.Property, out flag, out flag2);
                if (!sender.GetFlag(flag2))
                {
                    if (sender.IsLocalValue(e.Property))
                    {
                        sender.SetFlag(flag, true);
                        return;
                    }
                    sender.SetFlag(flag, false);
                    var parent = sender.Parent as TreeGridHeaderRowPresenter;
                    if (parent != null)
                    {
                        parent.UpdateHeaderProperty(sender, e.Property);
                    }
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TreeGridColumnHeaderRole role = this.Role;
            if (role == TreeGridColumnHeaderRole.Normal)
            {
                this.HookupGripperEvents();
                return;
            }
            if (role == TreeGridColumnHeaderRole.Floating)
            {
                this._floatingHeaderCanvas = (base.GetTemplateChild("PART_FloatingHeaderCanvas") as Canvas);
                this.UpdateFloatingHeaderCanvas();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            e.Handled = false;
            if (base.ClickMode == ClickMode.Hover && base.IsMouseCaptured)
            {
                base.ReleaseMouseCapture();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            e.Handled = false;
            if (base.ClickMode == ClickMode.Hover && e.ButtonState == MouseButtonState.Pressed)
            {
                base.CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (base.ClickMode != ClickMode.Hover && base.IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
            {
                base.IsPressed = true;
            }
            e.Handled = false;
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            this.CheckWidthForPreviousHeaderGripper();
        }

        protected override void OnClick()
        {
            if (!this.SuppressClickEvent && (this.IsAccessKeyOrAutomation || !this.IsMouseOutside()))
            {
                this.IsAccessKeyOrAutomation = false;
                this.ClickImplement();
                this.MakeParentGotFocus();
            }
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            this.IsAccessKeyOrAutomation = true;
            base.OnAccessKey(e);
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if (this.IsInternalGenerated)
            {
                return false;
            }
            Flags flags;
            Flags flags2;
            PropertyToFlags(dp, out flags, out flags2);
            return (flags == Flags.None || this.GetFlag(flags)) && base.ShouldSerializeProperty(dp);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (this.HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (this.HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (base.ClickMode == ClickMode.Hover && base.IsMouseCaptured)
            {
                base.ReleaseMouseCapture();
            }
        }

        #endregion

        #region 可拖动的 headerGripper

        private Thumb _headerGripper;

        internal void CheckWidthForPreviousHeaderGripper()
        {
            bool hide = false;
            if (this._headerGripper != null)
            {
                hide = DoubleUtil.LessThan(base.ActualWidth, this._headerGripper.Width);
            }
            if (this._previousHeader != null)
            {
                this._previousHeader.HideGripperRightHalf(hide);
            }
        }

        internal void OnColumnHeaderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && this._headerGripper != null && this._headerGripper.IsDragging)
            {
                this._headerGripper.CancelDrag();
                e.Handled = true;
            }
        }

        private void HookupGripperEvents()
        {
            this.UnhookGripperEvents();

            this._headerGripper = base.GetTemplateChild("PART_HeaderGripper") as Thumb;
            if (this._headerGripper != null)
            {
                this._headerGripper.DragStarted += new DragStartedEventHandler(this.OnColumnHeaderGripperDragStarted);
                this._headerGripper.DragDelta += new DragDeltaEventHandler(this.OnColumnHeaderGripperDragDelta);
                this._headerGripper.DragCompleted += new DragCompletedEventHandler(this.OnColumnHeaderGripperDragCompleted);
                this._headerGripper.MouseDoubleClick += new MouseButtonEventHandler(this.OnGripperDoubleClicked);
                this._headerGripper.MouseEnter += new MouseEventHandler(this.OnGripperMouseEnterLeave);
                this._headerGripper.MouseLeave += new MouseEventHandler(this.OnGripperMouseEnterLeave);
                this.GripperCursor = Cursors.SizeWE;
            }
        }

        private void UnhookGripperEvents()
        {
            if (this._headerGripper != null)
            {
                this._headerGripper.DragStarted -= new DragStartedEventHandler(this.OnColumnHeaderGripperDragStarted);
                this._headerGripper.DragDelta -= new DragDeltaEventHandler(this.OnColumnHeaderGripperDragDelta);
                this._headerGripper.DragCompleted -= new DragCompletedEventHandler(this.OnColumnHeaderGripperDragCompleted);
                this._headerGripper.MouseDoubleClick -= new MouseButtonEventHandler(this.OnGripperDoubleClicked);
                this._headerGripper.MouseEnter -= new MouseEventHandler(this.OnGripperMouseEnterLeave);
                this._headerGripper.MouseLeave -= new MouseEventHandler(this.OnGripperMouseEnterLeave);
                this._headerGripper = null;
            }
        }

        private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            var column = this.Column;
            if (column != null)
            {
                //这里需要重新计算动态宽度，同时不受最大宽度的限制。
                column.RequestDataWidth(false);

                e.Handled = true;
            }
        }

        private void OnGripperMouseEnterLeave(object sender, MouseEventArgs e)
        {
            this.HandleIsMouseOverChanged();
        }

        private void OnColumnHeaderGripperDragStarted(object sender, DragStartedEventArgs e)
        {
            this.MakeParentGotFocus();
            this._originalWidth = this.ColumnActualWidth;
            e.Handled = true;
        }

        private void OnColumnHeaderGripperDragDelta(object sender, DragDeltaEventArgs e)
        {
            this.UpdateGripperCursor();

            this.OnColumnHeaderResize(e);
        }

        private void OnColumnHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.Canceled)
            {
                this.UpdateColumnHeaderWidth(this._originalWidth);
            }
            e.Handled = true;
        }

        /// <summary>
        /// 在鼠标移动时更改图标。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGripperCursor()
        {
            var gridViewColumn = this.Column;
            if (gridViewColumn == null) return;

            // check range column bounds
            if (this._headerGripper.IsMouseCaptured)
            {
                var minWidth = gridViewColumn.MinWidth;
                var maxWidth = gridViewColumn.MaxWidth;
                if (double.IsNaN(minWidth) || double.IsNaN(maxWidth))
                {
                    if (!double.IsNaN(minWidth) && !double.IsNaN(maxWidth) && minWidth > maxWidth) return; // invalid case

                    if (!double.IsNaN(minWidth) && gridViewColumn.Width <= minWidth)
                    {
                        GripperCursor = Cursors.No;
                    }
                    else if (!double.IsNaN(maxWidth) && gridViewColumn.Width >= maxWidth)
                    {
                        GripperCursor = Cursors.No;
                    }
                    else
                    {
                        GripperCursor = Cursors.SizeWE;
                    }
                }
            }
        }

        private void HideGripperRightHalf(bool hide)
        {
            if (this._headerGripper != null)
            {
                FrameworkElement frameworkElement = this._headerGripper.Parent as FrameworkElement;
                if (frameworkElement != null)
                {
                    frameworkElement.ClipToBounds = hide;
                }
            }
        }

        private Cursor GripperCursor
        {
            get { return this._headerGripper.Cursor; }
            set { this._headerGripper.Cursor = value; }
        }

        #endregion

        #region Floating Header

        private Canvas _floatingHeaderCanvas;

        private TreeGridColumnHeader _srcHeader;

        private TreeGridColumnHeader _previousHeader;

        internal void ResetFloatingHeaderCanvasBackground()
        {
            if (this._floatingHeaderCanvas != null)
            {
                this._floatingHeaderCanvas.Background = null;
            }
        }

        private void UpdateFloatingHeaderCanvas()
        {
            if (this._floatingHeaderCanvas != null && this.FloatSourceHeader != null)
            {
                Vector offset = VisualTreeHelper.GetOffset(this.FloatSourceHeader);
                VisualBrush visualBrush = new VisualBrush(this.FloatSourceHeader);
                visualBrush.ViewboxUnits = BrushMappingMode.Absolute;
                visualBrush.Viewbox = new Rect(offset.X, offset.Y, this.FloatSourceHeader.ActualWidth, this.FloatSourceHeader.ActualHeight);
                this._floatingHeaderCanvas.Background = visualBrush;
                this.FloatSourceHeader = null;
            }
        }

        #endregion

        #region Width

        private double _originalWidth;

        private void OnColumnHeaderResize(DragDeltaEventArgs e)
        {
            double num = this.ColumnActualWidth + e.HorizontalChange;
            if (DoubleUtil.LessThanOrClose(num, 0.0))
            {
                num = 0.0;
            }
            this.UpdateColumnHeaderWidth(num);
            e.Handled = true;
        }

        private void UpdateColumnHeaderWidth(double width)
        {
            if (this.Column != null)
            {
                this.Column.Width = width;
                return;
            }
            base.Width = width;
        }

        #endregion

        #region 其它方法

        private void ClickImplement()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            {
                AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(this);
                if (automationPeer != null)
                {
                    automationPeer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
                }
            }
            base.OnClick();
        }

        private bool HandleIsMouseOverChanged()
        {
            if (base.ClickMode == ClickMode.Hover)
            {
                if (base.IsMouseOver && (this._headerGripper == null || !this._headerGripper.IsMouseOver))
                {
                    this.IsPressed = true;
                    this.OnClick();
                }
                else
                {
                    this.IsPressed = false;
                    //base.ClearValue(ButtonBase.IsPressedPropertyKey);
                }
                return true;
            }
            return false;
        }

        private void MakeParentGotFocus()
        {
            var presenter = base.Parent as TreeGridHeaderRowPresenter;
            if (presenter != null)
            {
                presenter.MakeParentItemsControlGotFocus();
            }
        }

        private bool IsMouseOutside()
        {
            Point position = Mouse.PrimaryDevice.GetPosition(this);
            return position.X < 0.0 || position.X > base.ActualWidth || position.Y < 0.0 || position.Y > base.ActualHeight;
        }

        #endregion

        #region 无法实现

        //private static DependencyObjectType _dType = DependencyObjectType.FromSystemType(typeof(TreeGridColumnHeader))

        //internal override DependencyObjectType DTypeThemeStyleKey
        //{
        //    [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        //    get
        //    {
        //        return TreeGridColumnHeader._dType;
        //    }
        //}

        #endregion
    }
}