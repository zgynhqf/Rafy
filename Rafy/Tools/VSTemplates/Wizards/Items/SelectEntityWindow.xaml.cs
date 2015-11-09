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

namespace RafySDK.Templates.Wizards
{
    /// <summary>
    /// Interaction logic for SelectEntityWindow.xaml
    /// </summary>
    public partial class SelectEntityWindow : System.Windows.Window
    {
        public SelectEntityWindow()
        {
            InitializeComponent();
        }

        public SelectEntityWindowViewModel ViewModel
        {
            get { return this.DataContext as SelectEntityWindowViewModel; }
            set { this.DataContext = value; }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void lbTypes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DialogResult = true;
        }

        public static CodeClass SelectTypeInIDE(DTE dte)
        {
            return SelectTypeInIDE(new SelectEntityWindowViewModel(dte));
        }

        public static CodeClass SelectTypeInIDE(SelectEntityWindowViewModel viewModel)
        {
            var win = new SelectEntityWindow();
            win.ViewModel = viewModel;
            var res = win.ShowDialog();
            if (res.GetValueOrDefault())
            {
                //ListBox.SelectedItem 属性无法双向绑定。原因不祥？
                //var type = viewModel.SelectedProject.SelectedEntityType;
                var type = win.lbTypes.SelectedItem as CodeClass;
                return type;
            }
            return null;
        }
    }
}
