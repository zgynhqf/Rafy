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

using System.ComponentModel;
using System.Windows.Media.Animation;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Layout
{
    public partial class NaviListDetailLayout : UserControl, ITraditionalLayoutControl
    {
        public NaviListDetailLayout()
        {
            InitializeComponent();
        }

        public void Arrange(TraditionalComponents components)
        {
            ResizingPanelSlippingAnimation.Initialize(mainPart, resultChildren, components.AggtBlocks.Layout.ParentChildProportion);

            //Children
            components.ArrangeChildrenByTabControl(childrenTab);

            this.TryArrangeMain(components.Main);
            this.TryArrangeCommandsContainer(components.CommandsContainer);
            this.TryArrangeNavigation(components.Navigation);
            this.TryArrangeDetail(components.Detail);

            this.OnArrangedCore(components.AggtBlocks);
        }

        private void TryArrangeMain(ControlResult control)
        {
            if (control != null)
            {
                main.Content = control.Control;
            }
            else
            {
                main.RemoveFromParent();
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
                toolBarContainer.RemoveFromParent();
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
                navigate.RemoveFromParent();
            }
        }

        private void TryArrangeDetail(ControlResult control)
        {
            if (control != null)
            {
                result.Content = control.Control;
            }
            else
            {
                result.RemoveFromParent();
            }
        }

        private void OnArrangedCore(AggtBlocks blocksInfo)
        {
            if (result.Parent == null)
            {
                //如果 resultChildren 中只显示聚合子类的视图
                if (childrenTab.Parent != null)
                {
                    ResizingPanelExt.SetStarGridLength(resultChildren, 0);

                    container.Orientation = blocksInfo.Layout.IsLayoutChildrenHorizonal ?
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