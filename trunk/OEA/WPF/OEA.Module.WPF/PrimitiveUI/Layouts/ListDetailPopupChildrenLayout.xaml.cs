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
    public partial class ListDetailPopupChildrenLayout : TraditionalLayout
    {
        public ListDetailPopupChildrenLayout()
        {
            InitializeComponent();
        }

        public override void TryArrangeMain(ControlResult control)
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

        public override void TryArrangeCommandsContainer(ControlResult toolBar)
        {
            if (toolBar != null)
            {
                toolBarContainer.Content = toolBar.Control;
            }
        }

        public override void TryArrangeNavigate(ControlResult control)
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

        public override void TryArrangeCondition(ControlResult control)
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

        public override void TryArrangeDetail(ControlResult control)
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

        public override void TryArrangeChildren(IList<Region> children)
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

        protected override void OnArrangedCore()
        {
            base.OnArrangedCore();

            if (toolBarContainer.Content == null)
            {
                toolBarContainer.RemoveFromParent();
            }
        }
    }
}