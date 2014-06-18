using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Windows.Input;

namespace Rafy.WPF.Controls
{
    /// <summary>
    /// Interaction logic for ViewDialog.xaml
    /// </summary>
    public partial class ViewDialog : Window
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ViewDialog"/>
        /// </summary>
        public ViewDialog()
        {
            InitializeComponent();

            //暂时为自动化测试关闭回车自动确定的功能
            this.YesAsDefault = false;

            WPFHelper.SetTrackFocusScope(this);
        }

        private void HideButtons()
        {
            commandPanel.Visibility = Visibility.Collapsed;
        }

        private ViewDialogButtons? _buttons;

        public ViewDialogButtons Buttons
        {
            get
            {
                return this._buttons.GetValueOrDefault(ViewDialogButtons.YesNo);
            }
            set
            {
                if (this._buttons.HasValue) throw new NotSupportedException("只能设置一次。");
                this._buttons = value;

                switch (value)
                {
                    case ViewDialogButtons.None:
                        this.HideButtons();
                        break;
                    case ViewDialogButtons.Yes:
                        btnCancel.Visibility = Visibility.Collapsed;
                        break;
                    case ViewDialogButtons.YesNo:
                        break;
                    case ViewDialogButtons.Close:
                        btnConfirm.Visibility = Visibility.Collapsed;
                        btnCancel.Content = "关闭";
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 是否把确定按钮设置为窗体中的 默认按钮。
        /// </summary>
        public bool YesAsDefault
        {
            get { return btnConfirm.IsDefault; }
            set { btnConfirm.IsDefault = value; }
        }

        internal bool ShowAsDialog { get; set; }

        /// <summary>
        /// Gets or sets the view to show in the dialog
        /// </summary>
        public FrameworkElement InnerContent
        {
            get
            {
                return innerContent.Content as FrameworkElement;
            }
            set
            {
                innerContent.Content = value;
            }
        }

        public void AddCommand(string commandName, object commandArg = null)
        {
            this.AddCommand(UIModel.WPFCommands.Find(commandName), commandArg);
        }

        public void AddCommand(Type commandName, object commandArg = null)
        {
            this.AddCommand(UIModel.WPFCommands.Find(commandName), commandArg);
        }

        public void AddCommand(WPFCommand cmd, object commandArg = null)
        {
            if (commandArg == null)
            {
                var window = WorkspaceWindow.GetOuterWorkspaceWindow(this.InnerContent) as ModuleWorkspaceWindow;
                if (window == null) throw new ArgumentNullException("内部控件没有 WindowTemplate 时，必须提供 commandArg 参数。");
                commandArg = window.MainView;
            }

            var items = commandPanel.Items;
            items.Remove(btnConfirm);
            items.Remove(btnCancel);

            AutoUI.BlockUIFactory.AppendCommands(commandPanel, commandArg, cmd);

            //始终把 btnConfirm、btnCancel 放到最后。
            items.Add(btnConfirm);
            items.Add(btnCancel);

            //commandPanel.Style = RafyResources.Rafy_CommandsContainer_Style;

            //Button btn = new Button()
            //{
            //    Margin = new Thickness(10, 2, 20, 2),
            //};
            //commandPanel.Children.Insert(0, btn);
            //if (window != null)
            //{
            //    btn.CommandParameter = window.View;
            //}
            //else
            //    btn.CommandParameter = View;
            //ButtonCommand.SetCommand(btn, CommandRepository.NewCommand(commandName));
        }

        #region Validation

        public event EventHandler<CancelEventArgs> ValidateOperations;

        protected virtual void RaiseDialogValidateOperations(CancelEventArgs e)
        {
            var hander = this.ValidateOperations;
            if (hander != null) { hander(this, e); }
        }

        #endregion

        #region Event handlers

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape)
            {
                this.OnCancelButtonClick(this, e);
            }
        }

        /// <summary>
        /// Handles the click event of the cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.ShowAsDialog)
            {
                this.DialogResult = false;
            }
            else
            {
                this._windowResult = WindowButton.No;
                this.Close();
            }
        }

        /// <summary>
        /// Handles the click event of the accept button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnAcceptButtonClick(object sender, RoutedEventArgs e)
        {
            var args = new CancelEventArgs();

            this.RaiseDialogValidateOperations(args);

            if (!args.Cancel)
            {
                if (this.ShowAsDialog)
                {
                    this.DialogResult = true;
                }
                else
                {
                    this._windowResult = WindowButton.Yes;

                    this.Close();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            this.OnWindowClosedByUser(new WindowClosedByUserEventArgs(this._windowResult));
        }

        private WindowButton _windowResult = WindowButton.Cancel;

        public event EventHandler<WindowClosedByUserEventArgs> WindowClosedByUser;

        private void OnWindowClosedByUser(WindowClosedByUserEventArgs e)
        {
            var handler = this.WindowClosedByUser;
            if (handler != null) handler(this, e);
        }

        public class WindowClosedByUserEventArgs : EventArgs
        {
            public WindowClosedByUserEventArgs(WindowButton dialogButton)
            {
                this.Button = dialogButton;
            }

            public WindowButton Button { get; private set; }
        }

        #endregion
    }

    /// <summary>
    /// 可用的按钮组合
    /// </summary>
    public enum ViewDialogButtons
    {
        None,
        Yes,
        YesNo,
        Close
    }
}