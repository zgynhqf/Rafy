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
using OEA.Module;
using OEA.Module.WPF;
using OEA.Module.WPF.Layout;

namespace JXC.WPF
{
    /// <summary>
    /// 使用上下布局来摆放条件面板和主体
    /// </summary>
    public partial class ConditionQueryLayout : UserControl, ILayoutControl
    {
        public ConditionQueryLayout()
        {
            InitializeComponent();
        }

        public void Arrange(UIComponents components)
        {
            var control = components.Main;
            if (control != null) { content.Content = control.Control; }

            control = components.CommandsContainer;
            if (control != null) { commands.Content = control.Control; }

            control = components.Condition;
            if (control != null) { queryPanel.Content = control.Control; }
        }
    }
}