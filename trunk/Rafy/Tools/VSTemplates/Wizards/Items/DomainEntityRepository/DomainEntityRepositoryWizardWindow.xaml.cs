/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140505
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140505 22:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EnvDTE;
using Rafy.VSPackage;
using RafySDK;
using RafySDK.Templates.Wizards;
using RafySDK.Templates.Wizards.Items.DomainEntityRepository;

namespace VSTemplates.Wizards
{
    public partial class DomainEntityRepositoryWizardWindow : System.Windows.Window
    {
        public DomainEntityRepositoryWizardWindow()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;

            this.AddHandler(UIElement.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(UIElement_GotKeyboardFocus));
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            txtClassName.Focus();
        }

        void UIElement_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var tb = e.Source as TextBox;
            if (tb != null)
            {
                tb.SelectAll();
                e.Handled = true;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
            }

            base.OnKeyDown(e);
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnSelectTypes_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as DomainEntityRepositoryWizardWindowViewModel;

            var sewVM = new SelectEntityWindowViewModel(vm.DTE);
            var win = new SelectEntityWindow();
            win.DataContext = sewVM;
            var res = win.ShowDialog();
            if (res.GetValueOrDefault())
            {
                //ListBox.SelectedItem 属性无法双向绑定。原因不祥？
                //var type = sewVM.SelectedProject.SelectedEntityType;
                var type = win.lbTypes.SelectedItem as CodeClass;
                vm.DomainNamespace = type.Namespace.FullName;
                vm.EntityTypeName = type.Name;
                vm.BaseTypeName = Helper.GetBaseClass(type).Name + Consts.RepositorySuffix;
            }
        }
    }
}
