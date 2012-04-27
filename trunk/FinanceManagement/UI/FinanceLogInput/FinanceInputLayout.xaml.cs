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
using OEA.Module.WPF;

namespace FM.UI
{
    public partial class FinanceInputLayout : UserControl, ITraditionalLayoutControl
    {
        public FinanceInputLayout()
        {
            InitializeComponent();
        }

        public void Arrange(TraditionalComponents components)
        {
            commands.Content = components.CommandsContainer.Control;
            main.Content = components.Main.Control;
            list.Content = components.FindControl("list").Control;
        }
    }
}
