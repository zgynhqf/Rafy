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
    public partial class ListDetailPopupChildrenLayout : UserControl, ITraditionalLayoutControl
    {
        public ListDetailPopupChildrenLayout()
        {
            InitializeComponent();
        }

        public void Arrange(TraditionalComponents components)
        {
            this.TryArrangeMain(components.Main);
            this.TryArrangeCommandsContainer(components.CommandsContainer);
            this.TryArrangeNavigation(components.Navigation);
            this.TryArrangeCondition(components.Condition);
            this.TryArrangeDetail(components.Detail);
            this.TryArrangeChildren(components.Children);
            this.OnArrangedCore();
        }

        private void TryArrangeMain(ControlResult control)
        {
            if (control != null)
            {
                listRegion.Content = control.Control;
            }
            else
            {
                listRegion.RemoveFromParent();
            }
        }

        private void TryArrangeCommandsContainer(ControlResult toolBar)
        {
            if (toolBar != null)
            {
                toolBarContainer.Content = toolBar.Control;
            }
        }

        private void TryArrangeNavigation(ControlResult control)
        {
            if (control != null)
            {
                dcNavigate.Content = control.Control;
            }
            else
            {
                dcNavigate.RemoveFromParent();
            }
        }

        private void TryArrangeCondition(ControlResult control)
        {
            if (control != null)
            {
                dcCondition.Content = control.Control;
            }
            else
            {
                dcCondition.RemoveFromParent();
            }
        }

        private void TryArrangeDetail(ControlResult control)
        {
            if (control != null)
            {
                detailRegion.Content = control.Control;
            }
            else
            {
                detailRegion.RemoveFromParent();
            }
        }

        private void TryArrangeChildren(IList<Region> children)
        {
            if (children.Count > 0)
            {
                var toolBar = new ToolBar();
                toolBarContainer.Content = toolBar;
                toolBarContainer.Visibility = Visibility.Visible;

                for (int i = 0, c = children.Count; i < c; i++)
                {
                    var child = children[i];

                    var btn = new Button()
                    {
                        Content = child.ChildrenLabel
                    };

                    btn.Click += (o, e) =>
                    {
                        var control = child.ControlResult.Control;
                        control.RemoveFromParent();
                        new Window()
                        {
                            WindowState = WindowState.Maximized,
                            Content = control
                        }.Show();
                    };

                    toolBar.Items.Add(btn);
                }
            }
        }

        private void OnArrangedCore()
        {
            if (toolBarContainer.Content == null)
            {
                toolBarContainer.RemoveFromParent();
            }
        }
    }
}