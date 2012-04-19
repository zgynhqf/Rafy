using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Itenso.Windows.Input;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Layout;

namespace OEA.WPF.Command
{
    [Command(Label = "选择全部")]
    public class SelectAllCommand : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            view.SelectAll();
        }
    }

    [Command(ImageName = "MaxShowView.bmp", Label = "最大化")]
    public class MaxShowViewCommand : ClientCommand<WPFObjectView>
    {
        public override bool CanExecute(WPFObjectView view)
        {
            var curControl = GetParentLayout(view.Control);
            if (curControl == null) return false;

            return base.CanExecute(view);
        }

        public override void Execute(WPFObjectView view)
        {
            var curControl = GetParentLayout(view.Control);
            if (curControl == null) return;

            view.IsActive = true;

            var oldPrent = curControl.Parent;
            curControl.RemoveFromParent(false);

            double initScale = GetInitScale(curControl);  //必须放在 parent.Content = null;之前

            //Title
            var title = string.Empty;
            if (view.ChildBlock == null)
            {
                title = view.Meta.Label;
            }
            else
            {
                title = view.ChildBlock.ViewMeta.Label;
            }

            //window
            App.Windows.ShowDialog(curControl, win =>
            {
                win.ResizeMode = ResizeMode.CanResize;
                win.Title = title;
                win.Buttons = ViewDialogButtons.None;

                #region 窗体设置

                //不要显示最大化，根据屏幕分辨率获取高度和宽度，上面留一块儿
                win.WindowStartupLocation = WindowStartupLocation.Manual;
                win.Top = 100;
                win.Left = 0;
                win.Width = SystemParameters.PrimaryScreenWidth;
                win.Height = SystemParameters.PrimaryScreenHeight - 200;
                win.Topmost = false;

                #endregion

                Zoom.EnableZoom(win, initScale);
            });

            curControl.LayoutTransform = null;
            curControl.RemoveFromParent();

            curControl.AttachToParent(oldPrent);
        }

        private double GetInitScale(FrameworkElement fe)
        {
            FrameworkElement v = LogicalTreeHelper.GetParent(fe) as FrameworkElement;
            if (null != v)
            {
                if (v.LayoutTransform is ScaleTransform)
                    return (v.LayoutTransform as ScaleTransform).ScaleX;
                else
                    return GetInitScale(v);
            }
            return 1;
        }

        private static FrameworkElement GetParentLayout(DependencyObject child)
        {
            var v = LogicalTreeHelper.GetParent(child);
            if (v == null) return null;
            if (v is ITraditionalLayoutControl) return v as FrameworkElement;

            return GetParentLayout(v);
        }
    }
}
