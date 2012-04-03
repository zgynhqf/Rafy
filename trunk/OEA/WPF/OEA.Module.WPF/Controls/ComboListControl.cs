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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Itenso.Windows.Input;
using OEA.Editors;
using OEA.MetaModel;
using OEA.MetaModel.View;

using OEA.Module.WPF.Automation;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;
using OEA.Utils;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using OEA.Library;
using OEA.Module.WPF.Command;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 这是一个使用 DataGrid/TreeGrid 来作为下拉内容的下拉控件（ComboBox）。
    /// 
    /// 核心思想 是使用外部提供的的 ListObjectView 来生成 选择控件，并在内部把它们的事件整合进来。
    /// 所以，外部操作 ListObjectView 的事件时，都应该同步到本对象上。
    /// 
    /// 继承ComboBox。
    /// 使用Resources/ComboListControl.xaml作为模板。
    /// </summary>
    [TemplatePart(Name = "PART_DropDownPanel", Type = typeof(DockPanel))]
    [TemplatePart(Name = "PART_ButtonPanel", Type = typeof(DockPanel))]
    public class ComboListControl : System.Windows.Controls.ComboBox
    {
        #region 字段 及 静态构造函数

        static ComboListControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboListControl),
                new FrameworkPropertyMetadata(typeof(ComboListControl)));
        }

        private PropertyEditor _clearCmdArg;

        /// <summary>
        /// 下拉的弹出框使用的是一个临时的ListObjectView来生成动态Grid。
        /// </summary>
        private IListObjectView _listView;

        private CLCProgress _curProgress = CLCProgress.NotStarted;

        #endregion

        internal CLCProgress CurrentProgress
        {
            get { return this._curProgress; }
            set { this._curProgress = value; }
        }

        #region 构造函数

        internal ComboListControl(IListObjectView listObjectView) : this(listObjectView, null) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listObjectView">内部界面使用这个 View 所对应的控件</param>
        /// <param name="clearCmdArg">为这个属性生成的下拉框。</param>
        internal ComboListControl(IListObjectView listObjectView, PropertyEditor clearCmdArg)
        {
            if (listObjectView == null) throw new ArgumentNullException("listObjectView");

            this._listView = listObjectView;
            ////弹出的 DataGrid 是不能编辑的，见：http://ipm.grandsoft.com.cn/issues/114217
            this._listView.IsReadOnly = true;
            this._listView.CurrentObjectChanged += (s, e) => this.CloseDropdownIfSelected();
            this._clearCmdArg = clearCmdArg;

            //构造Timer
            if (!DesignerProperties.GetIsInDesignMode(this)) { this.InitLazyFilter(); }

            this.IsEditable = true;
            this.StaysOpenOnEdit = true;
            this.IsTextSearchEnabled = false;
        }

        #endregion

        #region 构造 - 界面生成

        /// <summary>
        /// 获取控件的编辑区域
        /// </summary>
        private TextBox EditableTextBox
        {
            get { return base.GetTemplateChild("PART_EditableTextBox") as TextBox; }
        }

        /// <summary>
        /// 在应用模板时，在模板中生成一些动态控件。
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.CreateButtons();

            this.CreateTextBox();

            this.CreateListControl();
        }

        /// <summary>
        /// 生成一个清空数据的按钮
        /// </summary>
        private void CreateButtons()
        {
            if (this._clearCmdArg != null)
            {
                Panel buttonPanel = this.Template.FindName("PART_ButtonPanel", this) as Panel;

                //清除属性值按钮
                var btnClearValue = new Button();
                btnClearValue.Content = "清除属性值";
                btnClearValue.SetValue(DockPanel.DockProperty, Dock.Right);
                btnClearValue.Click += (o, e) => { this._clearCmdArg.PropertyValue = null; };

                buttonPanel.Children.Add(btnClearValue);
            }
        }

        private void CreateTextBox()
        {
            var txtBox = this.EditableTextBox;
            if (txtBox != null)
            {
                txtBox.PreviewKeyDown += new KeyEventHandler(On_EditableTextBox_PreviewKeyDown);
                txtBox.TextChanged += new TextChangedEventHandler(On_EditableTextBox_TextChanged);
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
            Panel dropDownPanel = this.Template.FindName("PART_DropDownPanel", this) as Panel;
            this._listControl = this._listView.Control as FrameworkElement;
            dropDownPanel.Children.Add(this._listControl);
        }

        internal FrameworkElement _listControl;

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
        private void On_EditableTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            this._curProgress = CLCProgress.KeyPressed;
        }

        /// <summary>
        /// 如果是用户开始输入，则开始计时。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void On_EditableTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._curProgress == CLCProgress.KeyPressed)
            {
                if (!this._listView.IsShowingTree)
                {
                    //ResetTimer
                    this._lazyFilterInterval.Stop();
                    this._lazyFilterInterval.Start();
                }
            }
        }

        /// <summary>
        /// 下拉关闭时，停止过滤功能。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDropDownClosed(EventArgs e)
        {
            base.OnDropDownClosed(e);

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
                                ConvertHZToPY.Convert(match).Contains(this.Text) ||
                                ConvertHZToPY.GetShortPY(match).Contains(this.Text);

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

        #endregion

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

        private string GetDisplay(object item)
        {
            return item.GetPropertyValue<string>(this.TextPath);
        }

        /// <summary>
        /// 当前 Text 在数据项中的属性路径。
        /// </summary>
        internal string TextPath { get; set; }

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
            if (owner._listControl != null)
            {
                var peer = UIElementAutomationPeer.CreatePeerForElement(owner._listControl);
                list.Add(peer);
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