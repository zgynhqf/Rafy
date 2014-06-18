using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
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
using Rafy;
using Rafy.WPF;

namespace MP.WPF
{
    /// <summary>
    /// Interaction logic for EmptyShell.xaml
    /// </summary>
    public partial class EmptyShell : Window, IShell
    {
        public EmptyShell()
        {
            InitializeComponent();

            this.Workspace = new TabControlWorkSpace(workspace);

            //选择皮肤
            SkinManager.Apply(ConfigurationHelper.GetAppSettingOrDefault("Skin", "Blue"));
        }

        public IWorkspace Workspace { get; private set; }

        private void sdScale_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sdScale.Value = 1.1;
        }
    }
}