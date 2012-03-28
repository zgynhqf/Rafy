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
    public partial class ListDetailLayout : TraditionalLayout
    {
        public ListDetailLayout()
        {
            InitializeComponent();
        }

        public override void OnArraging(AggtBlocks blocksInfo)
        {
            base.OnArraging(blocksInfo);

            LayoutSlippingAnimation.Initialize(this, main, resultChildren);
        }

        public override void TryArrangeMain(ControlResult control)
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

        public override void TryArrangeCommandsContainer(ControlResult toolBar)
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

        public override void TryArrangeNavigate(ControlResult control)
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

        public override void TryArrangeCondition(ControlResult control)
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

        public override void TryArrangeDetail(ControlResult control)
        {
            if (control != null)
            {
                result.Content = control.Control;
            }
            else
            {
                //result.Visibility = Visibility.Collapsed;
                result.RemoveFromParent(false);
            }
        }

        protected override TabControl ChildrenTab
        {
            get { return childrenTab; }
        }

        protected override void OnArrangedCore()
        {
            base.OnArrangedCore();

            if (queryPanel.Items.Count == 0)
            {
                queryPanel.RemoveFromParent(false);
            }

            if (result.Parent == null)
            {
                //如果 resultChildren 中只显示聚合子类的视图
                if (childrenTab.Parent != null)
                {
                    if (childrenTab.Visibility == Visibility.Collapsed)
                    {
                        ResizingPanelExt.SetStarGridLength(resultChildren, 0);
                    }

                    container.Orientation = this.AggtBlocks.Layout.IsLayoutChildrenHorizonal ?
                        Orientation.Horizontal : Orientation.Vertical;
                }
                else
                {
                    resultChildren.RemoveFromParent(false);
                }
            }
        }
    }
}