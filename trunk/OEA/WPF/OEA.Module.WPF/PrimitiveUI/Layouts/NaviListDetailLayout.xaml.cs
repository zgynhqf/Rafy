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
    public partial class NaviListDetailLayout : TraditionalLayout
    {
        public NaviListDetailLayout()
        {
            InitializeComponent();
        }

        public override void OnArraging(AggtBlocks blocksInfo)
        {
            base.OnArraging(blocksInfo);

            LayoutSlippingAnimation.Initialize(this, mainPart, resultChildren);
        }

        public override void TryArrangeMain(ControlResult control)
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

        public override void TryArrangeCommandsContainer(ControlResult toolBar)
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

        public override void TryArrangeNavigate(ControlResult control)
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

        public override void TryArrangeDetail(ControlResult control)
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

        protected override TabControl ChildrenTab
        {
            get
            {
                return childrenTab;
            }
        }

        protected override void OnArrangedCore()
        {
            base.OnArrangedCore();

            if (result.Parent == null)
            {
                //如果 resultChildren 中只显示聚合子类的视图
                if (childrenTab.Parent != null)
                {
                    ResizingPanelExt.SetStarGridLength(resultChildren, 0);

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