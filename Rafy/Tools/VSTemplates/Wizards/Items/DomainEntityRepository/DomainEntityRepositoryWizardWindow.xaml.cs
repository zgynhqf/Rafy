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

        private DomainEntityRepositoryWizardWindowViewModel VM
        {
            get { return this.DataContext as DomainEntityRepositoryWizardWindowViewModel; }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            txtClassName.Focus();

            this.TrySelectType();
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
            if (string.IsNullOrWhiteSpace(this.VM.DomainNamespace))
            {
                MessageBox.Show("没有填写实体的命名空间。");
            }
            else
            {
                this.DialogResult = true;
            }
        }

        private void btnSelectTypes_Click(object sender, RoutedEventArgs e)
        {
            var win = new SelectEntityWindow();
            win.DataContext = this.SelectEntityWindowViewModel;
            var res = win.ShowDialog();
            if (res.GetValueOrDefault())
            {
                //ListBox.SelectedItem 属性无法双向绑定。原因不祥？
                //var type = sewVM.SelectedProject.SelectedEntityType;
                var type = win.lbTypes.SelectedItem as CodeClass;
                SelectType(type);
            }
        }

        private void TrySelectType()
        {
            var typeName = this.VM.EntityTypeName;
            if (!string.IsNullOrWhiteSpace(typeName))
            {
                var projects = this.SelectEntityWindowViewModel.Projects;
                foreach (var project in projects)
                {
                    foreach (var item in project.EntityTypes)
                    {
                        if (item.Name == typeName)
                        {
                            this.SelectType(item);
                            break;
                        }
                    }
                }
            }
        }

        private void SelectType(CodeClass type)
        {
            this.VM.DomainNamespace = type.Namespace.FullName;
            this.VM.EntityTypeName = type.Name;
            this.VM.BaseTypeName = Helper.GetBaseClass(type).Name + Consts.RepositorySuffix;
        }

        private SelectEntityWindowViewModel _selectEntityWindowViewModel;

        private SelectEntityWindowViewModel SelectEntityWindowViewModel
        {
            get
            {
                if (_selectEntityWindowViewModel == null)
                {
                    _selectEntityWindowViewModel = new SelectEntityWindowViewModel(this.VM.DTE);
                }

                return _selectEntityWindowViewModel;
            }
        }
    }
}
