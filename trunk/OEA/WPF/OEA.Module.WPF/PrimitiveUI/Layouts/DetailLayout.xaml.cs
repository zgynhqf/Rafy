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

namespace OEA.Module.WPF.Layout
{
    public partial class DetailLayout : UserControl, ILayoutControl
    {
        public DetailLayout()
        {
            InitializeComponent();
        }

        public void Arrange(UIComponents components)
        {
            var control = components.Main;
            if (control != null)
            {
                result.Content = control.Control;
            }
            else
            {
                result.RemoveFromParent();
            }

            var toolBar = components.CommandsContainer;
            if (toolBar != null)
            {
                toolBarContainer.Content = toolBar.Control;
            }
            else
            {
                toolBarContainer.RemoveFromParent();
            }

            //Children
            components.ArrangeChildrenByTabControl(childrenTab);

            if (childrenTab.Parent != null)
            {
                ResizingPanelExt.SetStarGridLength(detail, 3);
                ResizingPanelExt.SetStarGridLength(childrenTab, 7);

                if (components.LayoutMeta.IsLayoutChildrenHorizonal)
                {
                    container.Orientation = Orientation.Horizontal;
                }
                else
                {
                    container.Orientation = Orientation.Vertical;
                }
            }
        }
    }
}
