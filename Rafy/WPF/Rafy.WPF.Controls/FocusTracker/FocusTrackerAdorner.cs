/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130712
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130712 09:45
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 一个用于跟踪指定范围内焦点移动的跟踪框。
    /// 
    /// <remarks>
    /// 一个元素一旦被设置为跟踪范围，那么在它内部的所有元素在获取焦点时，都会显示一个对应的跟踪框。
    /// 焦点如果从这个范围中跳出，则跟踪框会停留在最后一个元素上。
    /// 如果该元素中某个元素 a 标记为 <see cref="TrackFocusScope.Exclude"/>，那么 a 中的焦点也不会被跟踪。
    /// 
    /// 另外
    /// 使用 Windows API 的方法：http://msdn.microsoft.com/library/vstudio/aa358508(v=vs.90).aspx
    /// 
    /// 关于本控件的设计：
    /// 在范围上注册焦点跟踪后，当焦点移动到一个 ScrollViewer 中后，需要把 Adorner 添加到 ScrollContentPresenter 包含的 AdornerLayer中，
    /// 否则在拖动滚动条时，Adorner 不会移动。同时，动画的移动，都由最外层的 FocusTrackerAdorner 完成。
    /// 
    /// 所以，在切换焦点时，有以下情况：
    /// * 如果从 scope 移动到 ScrollViewer 中时，在 scope 中播放动画移动，移动完成后，在 ScrollContentPresenter 中生成一个对应的 FocusTrackerAdorner。
    /// * 如果从 ScrollViewer 移动到 scope 中时，先在 scope 中生成 FocusTrackerAdorner，然后播放移动动画。
    /// * 如果从 ScrollViewer 移动到 ScrollViewer 中时，则临时在 scope 中生成一个 FocusTrackerAdorner，用于播放动画；动画完成后，在目标 ScrollViewer 中生成一个 FocusTrackerAdorner。
    /// </remarks>
    /// </summary>
    public class FocusTrackerAdorner : Adorner
    {
        #region AwareInputElements AttachedDependencyProperty

        private static readonly Type[] DefaultAwareInputElementTypes = new Type[]{
            typeof(TextBox), typeof(PasswordBox), typeof(ComboBox), typeof(DatePicker),
        };

        /// <summary>
        /// 焦点跟踪范围控件内支持的输入元素的列表属性。
        /// </summary>
        public static readonly DependencyProperty AwareInputElementsProperty = DependencyProperty.RegisterAttached(
            "AwareInputElements", typeof(Type[]), typeof(FocusTrackerAdorner), new PropertyMetadata(DefaultAwareInputElementTypes)
            );

        /// <summary>
        /// 获取焦点跟踪范围控件内支持的输入元素的列表。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Type[] GetAwareInputElements(DependencyObject element)
        {
            return (Type[])element.GetValue(AwareInputElementsProperty);
        }

        /// <summary>
        /// 设置焦点跟踪范围控件内支持的输入元素的列表
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetAwareInputElements(DependencyObject element, Type[] value)
        {
            element.SetValue(AwareInputElementsProperty, value);
        }

        #endregion

        #region TrackFocusScope AttachedDependencyProperty

        /// <summary>
        /// 焦点跟踪的范围标记属性。
        /// </summary>
        public static readonly DependencyProperty TrackFocusScopeProperty = DependencyProperty.RegisterAttached(
            "TrackFocusScope", typeof(TrackFocusScope), typeof(FocusTrackerAdorner),
            new PropertyMetadata(new PropertyChangedCallback(OnTrackFocusScopeChanged))
            );

        /// <summary>
        /// 获取指定元素的焦点跟踪范围标记。
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static TrackFocusScope GetTrackFocusScope(DependencyObject scope)
        {
            return (TrackFocusScope)scope.GetValue(TrackFocusScopeProperty);
        }

        /// <summary>
        /// 设置指定元素的焦点跟踪范围标记。
        /// </summary>
        /// <param name="scope">需要设置为跟踪范围的元素。</param>
        /// <param name="value">是否为跟踪范围。</param>
        public static void SetTrackFocusScope(DependencyObject scope, TrackFocusScope value)
        {
            scope.SetValue(TrackFocusScopeProperty, value);
        }

        private static void OnTrackFocusScopeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var scope = sender as FrameworkElement;
            if (scope == null) throw new ArgumentException("scope 对象必须是一个 FrameworkElement。");

            var value = (TrackFocusScope)e.NewValue;
            //如果该元素被设置为跟踪范围，则为它加入 Adorner。
            if (value == TrackFocusScope.Scope)
            {
                if (scope.IsLoaded)
                {
                    AddAdorner(scope);
                }
                else
                {
                    scope.Loaded += scope_Loaded;
                }
            }
            else
            {
                //另外两种情况（None、Exclude），都需要移除 Layer 中的 FocusTrackerAdorner
                var layer = AdornerLayer.GetAdornerLayer(scope);
                if (layer != null)
                {
                    var adorners = layer.GetAdorners(scope);
                    if (adorners != null)
                    {
                        foreach (var adorner in adorners)
                        {
                            if (adorner is FocusTrackerAdorner)
                            {
                                layer.Remove(adorner);
                            }
                        }
                    }
                }
            }
        }

        static void scope_Loaded(object sender, RoutedEventArgs e)
        {
            var scope = sender as FrameworkElement;
            scope.Loaded -= scope_Loaded;

            AddAdorner(scope);
        }

        private static void AddAdorner(FrameworkElement scope)
        {
            //如果元素是一个窗体，那么它的 AdornerLayer 是定义在窗体的模板中的。所以只能对窗体内部的元素来查找 AdornerLayer。
            if (scope is Window)
            {
                scope = (scope as Window).Content as FrameworkElement;
            }

            if (scope != null)
            {
                var layer = AdornerLayer.GetAdornerLayer(scope);
                if (layer != null)
                {
                    layer.Add(new FocusTrackerAdorner(scope, layer));
                }
            }
        }

        #endregion

        /// <summary>
        /// 被装饰的最处层的范围控件。
        /// 其中所有元素在获取焦点时，都会显示跟踪框。
        /// </summary>
        private UIElement _scope;
        /// <summary>
        /// _scope 对应的 AdornerLayer。
        /// </summary>
        private AdornerLayer _scopeLayer;
        /// <summary>
        /// _layer 对应的根控件。
        /// </summary>
        private UIElement _layerRoot;
        /// <summary>
        /// 本 Adorner 所添加到的 Layer。
        /// </summary>
        private AdornerLayer _layer;
        /// <summary>
        /// 范围内当前获取焦点的文本框。
        /// </summary>
        private UIElement _focusedElement;
        /// <summary>
        /// 用于显示焦点位置的控件。
        /// </summary>
        private FocusTrackerControl _tracker;
        /// <summary>
        /// 所有的孩子集合。只有 _tracker 一个元素。
        /// </summary>
        private UIElementCollection _children;

        /// <summary>
        /// 直接使用最外层的范围控件及对应的 AdornerLayer 来构造一个 FocusTrackerAdorner 对象。
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="scopeLayer"></param>
        private FocusTrackerAdorner(UIElement scope, AdornerLayer scopeLayer) : this(scope, scopeLayer, scope, scopeLayer) { }

        /// <summary>
        /// 构造一个 FocusTrackerAdorner 对象，并指定它所在的 AdornerLayer 及所在范围。
        /// </summary>
        /// <param name="layerRoot">layer 对应的根控件。</param>
        /// <param name="layer">本 Adorner 所添加到的 Layer。</param>
        /// <param name="scope">最外层的范围控件。</param>
        /// <param name="scopeLayer">最外层的范围控件对应的 AdornerLayer。</param>
        private FocusTrackerAdorner(UIElement layerRoot, AdornerLayer layer, UIElement scope, AdornerLayer scopeLayer)
            : base(layerRoot)
        {
            if (scope == null) throw new ArgumentNullException("scope");
            if (layer == null) throw new ArgumentNullException("layer");

            _scope = scope;
            _scopeLayer = scopeLayer;
            _layerRoot = layerRoot;
            _layer = layer;

            _children = new UIElementCollection(this, this);
            _tracker = new FocusTrackerControl { Adorner = this };
            _children.Add(_tracker);

            this.Loaded += OnLoaded;
            scope.GotKeyboardFocus += scope_GotKeyboardFocus;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            //在首次加载本控件时，需要定位到已经获取焦点的元素上。
            var focusedElement = Keyboard.FocusedElement as UIElement;
            if (focusedElement != null && focusedElement.IsDescendantOf(_scope))
            {
                BindToNewFocus(focusedElement);
            }
        }

        void scope_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            BindToNewFocus(e.NewFocus);
        }

        /// <summary>
        /// 当焦点变更时，会调用此函数来绑定到目标焦点元素上。
        /// </summary>
        /// <param name="newFocusElement"></param>
        private void BindToNewFocus(IInputElement newFocusElement)
        {
            var newFocus = newFocusElement as Visual;
            if (newFocus != null)
            {
                var focusedElement = GetInputElement(newFocus);
                if (focusedElement != null && _focusedElement != focusedElement)
                {
                    //_scopeLayer != _layer 表示当前 Adorner 是一个 ScrollViewer 中的 Adorner，而不是属于最外层范围控件。
                    //而此时，如果新的焦点不在当前层的范围内，则回到最外层，播放移动动画。
                    if (_scopeLayer != _layer && !newFocus.IsDescendantOf(_layerRoot))
                    {
                        MoveToScope();
                        return;
                    }

                    //同层内切换焦点，先绑定到最新的元素；然后移动跟踪控件。
                    this.BindToInputElement(focusedElement);
                    this.MoveTracker();
                }
            }
        }

        /// <summary>
        /// 获取焦点控件外部可用的输入控件。
        /// </summary>
        /// <param name="newFocus"></param>
        /// <returns></returns>
        private UIElement GetInputElement(Visual newFocus)
        {
            //先找到最上层的范围控件中定义的所有支持的输入控件列表。
            var awareElementTypes = DefaultAwareInputElementTypes;
            for (DependencyObject visual = newFocus; visual != null; visual = VisualTreeHelper.GetParent(visual))
            {
                //如果新的焦点在一个排除范围内，则直接退出。否则继续查找。
                var tfs = GetTrackFocusScope(visual);
                if (tfs == TrackFocusScope.Exclude) { break; }
                if (tfs == TrackFocusScope.Scope)
                {
                    awareElementTypes = GetAwareInputElements(visual) ?? DefaultAwareInputElementTypes;
                    break;
                }
            }

            //从 visual 开始往上找，找到最外层的一个输入控件。
            //找最外层的原因是，ComboBox 控件内部本身就有一个 TextBox，而这时应该选中外部的 ComboBox。
            UIElement inputElement = null;
            for (DependencyObject visual = newFocus; visual != null; visual = VisualTreeHelper.GetParent(visual))
            {
                //如果新的焦点在一个排除范围内，则直接退出。否则继续查找。
                var tfs = GetTrackFocusScope(visual);
                if (tfs == TrackFocusScope.Exclude) { return null; }
                if (tfs == TrackFocusScope.Scope) { break; }

                if (IsInputElement(visual, awareElementTypes))
                {
                    inputElement = visual as UIElement;
                }
            }

            return inputElement;
        }

        /// <summary>
        /// 判断指定的可视元素是否为输入控件。
        /// 默认情况下，识别 TextBox、PasswordBox、ComboBox、DatePicker 等控件。
        /// </summary>
        /// <param name="visual">The visual.</param>
        /// <param name="awareElementTypes">The aware element types.</param>
        /// <returns></returns>
        private static bool IsInputElement(DependencyObject visual, Type[] awareElementTypes)
        {
            for (int i = 0; i < awareElementTypes.Length; i++)
            {
                var e = awareElementTypes[i];
                if (e.IsInstanceOfType(visual)) return true;
            }
            return false;
        }

        private void BindToInputElement(UIElement focusedElement)
        {
            if (_focusedElement != null)
            {
                _focusedElement.LayoutUpdated -= _focusedElement_LayoutUpdated;
                _focusedElement.IsVisibleChanged -= _focusedElement_IsVisibleChanged;
            }

            _focusedElement = focusedElement;

            _focusedElement.IsVisibleChanged += _focusedElement_IsVisibleChanged;
            _focusedElement.LayoutUpdated += _focusedElement_LayoutUpdated;
        }

        /// <summary>
        /// 当焦点元素的大小变化时，需要同步到焦点跟踪框上。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _focusedElement_LayoutUpdated(object sender, EventArgs e)
        {
            var renderSize = _focusedElement.RenderSize;
            var current = _tracker.Dest;
            if (!DoubleUtil.AreClose(current.Width, renderSize.Width) ||
                !DoubleUtil.AreClose(current.Height, renderSize.Height))
            {
                _tracker.MoveTo(new Rect(current.TopLeft, renderSize), true);
            }
        }

        /// <summary>
        /// 输入元素不可见时，需要隐藏焦点框。（例如切换到其它页签。）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _focusedElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_tracker != null && _focusedElement != null)
            {
                _tracker.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 根据当前焦点控件的位置，来移动 Tracker。
        /// </summary>
        private void MoveTracker()
        {
            if (_tracker != null && _focusedElement != null)
            {
                if (_focusedElement.IsVisible)
                {
                    _tracker.Visibility = Visibility.Visible;

                    //动画移动显示控件到目的位置。
                    var start = _focusedElement.TransformToAncestor(_layerRoot).Transform(new Point());
                    _tracker.MoveTo(new Rect(start, _focusedElement.RenderSize), true);
                }
                else
                {
                    _tracker.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region 绘制

        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _children[index];
        }

        protected override IEnumerator LogicalChildren
        {
            get { return _children.GetEnumerator(); }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            //_tracker 需要占用整个范围的大小，然后根据自身的参数来显示小的跟踪框。
            _tracker.Arrange(new Rect(finalSize));

            return finalSize;
        }

        #endregion

        #region 跨层切换焦点时，生成新的 Adorner

        /// <summary>
        /// 表示当前对象是否已经从对应的 AdornerLayer 中移除，不再可用。
        /// 不可用时，将不再监听范围焦点事件。
        /// </summary>
        private bool _removed;

        /// <summary>
        /// 在最外层的 Layer 中来进行移动。
        /// </summary>
        private void MoveToScope()
        {
            if (!_removed)
            {
                _removed = true;
                _scope.GotKeyboardFocus -= scope_GotKeyboardFocus;

                var tracker = new FocusTrackerAdorner(_scope, _scopeLayer);
                _scopeLayer.Add(tracker);

                //如果本次焦点切换后，本 Adorner 还在 _scope 中，则找到它的位置并设置为 ScopeAdorner 的初始位置。
                //如果本 Adorner 在一个面签中，并且焦点切换是因为切换这个页签造成，则这个判断为 false.
                if (this.IsDescendantOf(_scope))
                {
                    var startFrom = this.TransformToAncestor(_scope).Transform(_tracker.Dest.TopLeft);
                    tracker._tracker.MoveTo(new Rect(startFrom, _focusedElement.RenderSize), false);
                }

                _layer.Remove(this);
            }
        }

        /// <summary>
        /// 在移动的动画完成时自动调用。
        /// </summary>
        internal void NotifyMoveCompleted()
        {
            //如果焦点元素所在的 Layer 不同，则在新的层上生成一个新的 Adorner
            var targetLayer = AdornerLayer.GetAdornerLayer(_focusedElement);
            if (targetLayer != _layer)
            {
                this.MoveToInnerLayer(targetLayer);
            }
        }

        /// <summary>
        /// 把当前层加入到内部层中。
        /// </summary>
        private void MoveToInnerLayer(AdornerLayer targetLayer)
        {
            var layerParent = VisualTreeHelper.GetParent(targetLayer) as Visual;
            var root = VisualTreeHelper.GetChild(layerParent, 0) as UIElement;
            if (root != null)
            {
                if (!_removed)
                {
                    _removed = true;
                    _scope.GotKeyboardFocus -= scope_GotKeyboardFocus;

                    //必须重新构造一个，否则由于 AdornedElement 不在 root 内，造成无法添加到 targetLayer 中。
                    var tracker = new FocusTrackerAdorner(root, targetLayer, _scope, _scopeLayer);
                    targetLayer.Add(tracker);

                    //设置初始的位置。
                    var startFrom = _focusedElement.TransformToAncestor(root).Transform(new Point());
                    tracker._tracker.MoveTo(new Rect(startFrom, _focusedElement.RenderSize), false);

                    _layer.Remove(this);
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// 表示某个元素是否是一个焦点跟踪框的范围
    /// </summary>
    public enum TrackFocusScope
    {
        /// <summary>
        /// 未指定
        /// </summary>
        None,
        /// <summary>
        /// 是一个焦点跟踪范围
        /// </summary>
        Scope,
        /// <summary>
        /// 该元素内部的所有元素都不会被焦点跟踪。
        /// </summary>
        Exclude
    }
}