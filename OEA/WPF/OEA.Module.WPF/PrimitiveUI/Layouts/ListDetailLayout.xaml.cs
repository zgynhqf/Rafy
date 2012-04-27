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
using AvalonDock;
using OEA.Module.WPF.Controls;
using System.ComponentModel;
using System.Windows.Media.Animation;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Layout
{
    public partial class ListDetailLayout : UserControl, ITraditionalLayoutControl
    {
        public ListDetailLayout()
        {
            InitializeComponent();
        }

        public void Arrange(TraditionalComponents components)
        {
            ResizingPanelSlippingAnimation.Initialize(main, childrenTab, components.AggtBlocks.Layout.ParentChildProportion);

            this.TryArrangeMain(components.Main);
            this.TryArrangeCommandsContainer(components.CommandsContainer);
            this.TryArrangeNavigation(components.Navigation);
            this.TryArrangeCondition(components.Condition);

            //Children
            components.ArrangeChildrenByTabControl(childrenTab);

            this.OnArrangedCore(components);
        }

        private void TryArrangeMain(ControlResult control)
        {
            if (control != null)
            {
                main.Content = control.Control;
            }
            else
            {
                main.RemoveFromParent(false);
            }
        }

        private void TryArrangeCommandsContainer(ControlResult toolBar)
        {
            if (toolBar != null)
            {
                toolBarContainer.Content = toolBar.Control;
            }
            else
            {
                toolBarContainer.RemoveFromParent(false);
            }
        }

        private void TryArrangeNavigation(ControlResult control)
        {
            if (control != null)
            {
                navigate.Content = control.Control;
            }
            else
            {
                navigate.RemoveFromParent(false);
            }
        }

        private void TryArrangeCondition(ControlResult control)
        {
            if (control != null)
            {
                condition.Content = control.Control;
            }
            else
            {
                condition.RemoveFromParent();
            }
        }

        private void OnArrangedCore(TraditionalComponents components)
        {
            if (queryPanel.Items.Count == 0)
            {
                queryPanel.RemoveFromParent(false);
            }

            if (childrenTab.Parent != null)
            {
                container.Orientation = components.AggtBlocks.Layout.IsLayoutChildrenHorizonal ?
                    Orientation.Horizontal : Orientation.Vertical;
            }
        }
    }
}