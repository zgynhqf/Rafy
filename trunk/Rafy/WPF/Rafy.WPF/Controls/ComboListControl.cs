/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 不再使用 ComboListControl 的 DisplayMemberPath 及 ItemsSource 属性，
 *      而是直接绑定 Text，内部直接使用 AutoUI 生成的列表控件 胡庆访 20110810
 * ComboDataGrid 重命名为 ComboListControl。
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Reflection;
using Rafy.Utils;
using Rafy.WPF.Automation;
using Rafy.WPF.Command;
using Rafy.WPF.Editors;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// 这是一个使用 DataGrid/TreeGrid 来作为下拉内容的下拉控件（ComboBox）。
    /// 
    /// 核心思想 是使用外部提供的的 ListLogicalView 来生成 选择控件，并在内部把它们的事件整合进来。
    /// 所以，外部操作 ListLogicalView 的事件时，都应该同步到本对象上。
    /// 
    /// 继承ComboBox。
    /// 
    /// 注意，虽然继承自 ComboBox，但是并没有使用到基类中定义的 Selected 机制相关的属性和事件。
    /// </summary>
    [TemplatePart(Name = "PART_DropDownPanel", Type = typeof(Panel))]
    public class ComboListControl : ComboBox
    {
        static ComboListControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboListControl), new FrameworkPropertyMetadata(typeof(ComboListControl)));
            IsEditableProperty.OverrideMetadata(typeof(ComboListControl), new FrameworkPropertyMetadata(true));

            CommandManager.RegisterClassCommandBinding(typeof(ComboListControl),
                new CommandBinding(ClearValueCommand, ClearValueCommand_Executed));
        }

        #region 字段

        /// <summary>
        /// 下拉的弹出框使用的是一个临时的ListLogicalView来生成动态Grid。
        /// </summary>
        private ListLogicalView _listView;

        private WPFEntityViewMeta _refViewMeta;

        private CLCProgress _curProgress = CLCProgress.NotStarted;

        /// <summary>
        /// 当前 Text 在数据项中的属性路径。
        /// </summary>
        private string _textPath;

        #endregion

        /// <summary>
        /// 清除值的命令。
        /// </summary>
        public static RoutedCommand ClearValueCommand = new RoutedCommand("ClearValue", typeof(ComboListControl));
        private static void ClearValueCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            (sender as ComboListControl).ClearSelection();
        }

        #region 属性接口

        #region RefEntityType DependencyProperty

        public static readonly DependencyProperty RefEntityTypeProperty = DependencyProperty.Register(
            "RefEntityType", typeof(Type), typeof(ComboListControl),
            new PropertyMetadata((d, e) => (d as ComboListControl).OnRefEntityTypeChanged(e))
            );

        /// <summary>
        /// 设置引用实体的类型。
        /// </summary>
        public Type RefEntityType
        {
            get { return (Type)this.GetValue(RefEntityTypeProperty); }
            set { this.SetValue(RefEntityTypeProperty, value); }
        }

        private void OnRefEntityTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (Type)e.NewValue;
            if (value == null)
            {
                this._refViewMeta = null;
            }
            else
            {
                if (this._refViewMeta.EntityType != value)
                {
                    this._refViewMeta = UIModel.Views.CreateBaseView(value) as WPFEntityViewMeta;
                }
            }
        }

        #endregion

        #region TextFilterEnabled DependencyProperty

        public static readonly DependencyProperty TextFilterEnabledProperty = DependencyProperty.Register(
            "TextFilterEnabled", typeof(bool), typeof(ComboListControl),
            new PropertyMetadata((d, e) => (d as ComboListControl).OnTextFilterEnabledChanged(e))
            );

        /// <summary>
        /// 是否启用文本输入过滤功能
        /// </summary>
        public bool TextFilterEnabled
        {
            get { return (bool)this.GetValue(TextFilterEnabledProperty); }
            set { this.SetValue(TextFilterEnabledProperty, value); }
        }

        private void OnTextFilterEnabledChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
        }

        #endregion

        #region IsMultiSelection DependencyProperty

        public static readonly DependencyProperty IsMultiSelectionProperty = DependencyProperty.Register(
            "IsMultiSelection", typeof(bool), typeof(ComboListControl),
            new PropertyMetadata((d, e) => (d as ComboListControl).OnIsMultiSelectionChanged(e))
            );

        public bool IsMultiSelection
        {
            get { return (bool)this.GetValue(IsMultiSelectionProperty); }
            set { this.SetValue(IsMultiSelectionProperty, value); }
        }

        private void OnIsMultiSelectionChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
        }

        #endregion

        #region CanClear DependencyProperty

        public static readonly DependencyProperty CanClearProperty = DependencyProperty.Register(
            "CanClear", typeof(bool), typeof(ComboListControl),
            new PropertyMetadata(true, (d, e) => (d as ComboListControl).OnCanClearChanged(e))
            );

        /// <summary>
        /// 标记本控件是否支持清空当前选择项。
        /// </summary>
        public bool CanClear
        {
            get { return (bool)this.GetValue(CanClearProperty); }
            set { this.SetValue(CanClearProperty, value); }
        }

        private void OnCanClearChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
        }

        #endregion

        /// <summary>
        /// 引用实体的视图元数据。
        /// 
        /// 本元数据将会被用来生成列表控件。
        /// </summary>
        public WPFEntityViewMeta RefViewMeta
        {
            get { return this._refViewMeta; }
            set
            {
                this._refViewMeta = value;
                if (value == null)
                {
                    this.RefEntityType = null;
                }
                else
                {
                    this.RefEntityType = value.EntityType;
                }
            }
        }

        /// <summary>
        /// 返回内部使用的列表视图。
        /// </summary>
        public ListLogicalView InnerListView
        {
            get
            {
                this.InitListView();

                return this._listView;
            }
            set
            {
                this._listView = value;
                if (value == null)
                {
                    this._refViewMeta = null;
                    this.RefEntityType = null;
                }
                else
                {
                    this._refViewMeta = value.Meta;
                    this.RefEntityType = value.Meta.EntityType;
                }
            }
        }

        internal CLCProgress CurrentProgress
        {
            get { return this._curProgress; }
            set { this._curProgress = value; }
        }

        #endregion

        #region 构造 - 界面生成

        /// <summary>
        /// 在应用模板时，在模板中生成一些动态控件。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.CreateTextBox();

            this.CreateListControl();
        }

        private void CreateTextBox()
        {
            var txtBox = this.Template.FindName("PART_EditableTextBox", this) as TextBox;
            if (txtBox != null)
            {
                txtBox.PreviewKeyDown += On_EditableTextBox_PreviewKeyDown;
                txtBox.KeyDown += (o, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                        TraversalRequest request = new TraversalRequest(focusDirection);
                        txtBox.MoveFocus(request);
                    }
                };
            }
        }

        /// <summary>
        /// 生成用于显示弹出数据的一个DataGrid
        /// </summary>
        private void CreateListControl()
        {
            Panel ddPanel = this.Template.FindName("PART_DropDownPanel", this) as Panel;
            if (ddPanel != null) { ddPanel.Children.Add(this.InnerListView.Control); }
        }

        private void InitListView()
        {
            if (this._listView == null)
            {
                var rvm = this._refViewMeta;
                if (rvm == null) throw new InvalidProgramException("还没有设置控件的 RefViewMeta 或者 RefEntityType 属性。");

                var title = rvm.TitleProperty;
                if (title == null) throw new InvalidProgramException(string.Format("{0} 没有设置代表属性，无法为其生成下拉控件。", rvm.Name));
                this._textPath = title.Name;

                //创建一个只读的 ListLogicalView
                var listView = AutoUI.ViewFactory.CreateListView(rvm, true);
                listView.IsReadOnly = ReadOnlyStatus.ReadOnly;
                if (this.IsMultiSelection) { listView.CheckingMode = CheckingMode.CheckingRow; }
                listView.CurrentChanged += (s, e) => this.OnListViewSelectionChanged();

                //构造Timer
                if (!DesignerProperties.GetIsInDesignMode(this)) { this.InitLazyFilter(); }

                this._listView = listView;
            }
        }

        private void On_EditableTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.TextFilterEnabled)
            {
                this.StartFilterTimer();
            }
            else
            {
                //如果用户在可清空的模式下点击了以下两个按键，则清空选项。
                if (this.CanClear)
                {
                    if (e.Key == Key.Delete || e.Key == Key.Back)
                    {
                        this.ClearSelection();
                    }
                }

                //非过滤状态下，不让用户输入。
                e.Handled = true;
            }
        }

        #endregion

        #region 文本输入过滤

        /// <summary>
        /// 控制输入定时器
        /// </summary>
        private Timer _lazyFilterInterval;

        /// <summary>
        /// 表示用户点击界面引起此事件。
        /// 如果不是，则表示 框架其它类 引发的选择事件。
        /// 
        /// http://ipm.grandsoft.com.cn/issues/243370
        /// </summary>
        internal bool SelectedByUser = true;

        private void InitLazyFilter()
        {
            //定时器延迟时间 800
            this._lazyFilterInterval = new Timer(800);
            this._lazyFilterInterval.AutoReset = false;
            this._lazyFilterInterval.Elapsed += (o, e) => this.FireFiltering();
        }

        /// <summary>
        /// 开始控件的 KeyPressed 流程。
        /// （下一步：文本改变。）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartFilterTimer()
        {
            this._curProgress = CLCProgress.KeyPressed;

            //如果是用户开始输入，则开始计时。(ResetTimer)
            this._lazyFilterInterval.Stop();
            this._lazyFilterInterval.Start();
        }

        /// <summary>
        /// 下拉关闭时，停止过滤功能。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

            if (!this.TextFilterEnabled) { return; }

            this._lazyFilterInterval.Stop();

            //关闭下拉时，就把数据还原为所有数据
            this._listView.Filter = null;
        }

        /// <summary>
        /// 执行过滤操作
        /// </summary>
        private void FireFiltering()
        {
            if (this._curProgress == CLCProgress.KeyPressed)
            {
                //输入后异步触发事件
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    try
                    {
                        this._curProgress = CLCProgress.LazyFiltering;

                        // 绑定新的数据源
                        this._listView.Filter = item =>
                        {
                            string match = this.GetDisplay(item);

                            //在数据源中按输入的文字查找符合要求的项
                            var result = match.Contains(this.Text) ||
                                PinYinConverter.Convert(match).Contains(this.Text) ||
                                PinYinConverter.GetShortPY(match).Contains(this.Text);

                            return result;
                        };

                        this.IsDropDownOpen = true;
                    }
                    finally
                    {
                        this._curProgress = CLCProgress.NotStarted;
                    }
                }, DispatcherPriority.ApplicationIdle);
            }
        }

        private string GetDisplay(object item)
        {
            return ObjectHelper.GetPropertyValue<string>(item, this._textPath);
        }

        #endregion

        /// <summary>
        /// 外层需要使用此事件来监听选择变更事件。
        /// （基类的 Selector.SelectionChanged 事件是无法使用的。）
        /// </summary>
        public event EventHandler ListViewSelectionChanged;

        protected virtual void OnListViewSelectionChanged()
        {
            this.CloseDropdownIfSelected();

            var handler = this.ListViewSelectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private void ClearSelection()
        {
            this._listView.Current = null;
        }

        private void CloseDropdownIfSelected()
        {
            //如果用户选择完毕，则关闭弹出界面。
            if (this._curProgress == CLCProgress.NotStarted)
            {
                if (this._listView.CheckingMode != CheckingMode.CheckingRow && this.SelectedByUser)
                {
                    this.IsDropDownOpen = false;
                }
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ComboListControlAutomationPeer(this);
        }
    }

    public class ComboListControlAutomationPeer : ComboBoxAutomationPeer
    {
        public ComboListControlAutomationPeer(ComboListControl owner) : base(owner) { }

        /// <summary>
        /// 获取所有的子自动化节点
        /// </summary>
        /// <returns></returns>
        protected override List<AutomationPeer> GetChildrenCore()
        {
            var list = base.GetChildrenCore();

            //需要在子自动化节点列表中加入列表控件，方便自动化进行查找
            var owner = this.Owner as ComboListControl;
            var view = owner.InnerListView;
            if (view != null)
            {
                var peer = UIElementAutomationPeer.CreatePeerForElement(view.Control);
                if (peer != null) { list.Add(peer); }
            }

            return list;
        }
    }

    /// <summary>
    /// ComboListControl 的所有过程
    /// </summary>
    internal enum CLCProgress
    {
        /// <summary>
        /// 没有进行任何流程
        /// </summary>
        NotStarted,

        /// <summary>
        /// 用户按键
        /// </summary>
        KeyPressed,

        /// <summary>
        /// 过滤过程中
        /// </summary>
        LazyFiltering
    }
}