///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120415
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120415
// * 
//*******************************************************/

//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using JXC.WPF.Templates;
//using Rafy;
//using Rafy.Library;
//using Rafy.MetaModel;
//using Rafy.MetaModel.Attributes;
//using Rafy.MetaModel.View;
//using Rafy.WPF;
//using Rafy.WPF.Behaviors;
//using Rafy.WPF.Controls;
//using Rafy.WPF.Command;

//namespace JXC.Commands
//{
//    public abstract class PopupBillCommandBase : ListViewCommand
//    {
//        protected WindowButton PopupEditingDialog(
//            ListLogicalView view, Entity tmpEntity,
//            Action<ViewDialog> windowSetter,
//            bool isReadonly = false
//            )
//        {
//            //弹出窗体显示详细面板
//            var billTemplate = new BillTemplate();
//            var ui = billTemplate.CreateUI(view.EntityType);

//            var detailView = ui.MainView.CastTo<DetailLogicalView>();
//            detailView.Data = tmpEntity;

//            //使用一个 StackPanel，保证详细面板不会自动变大
//            var ctrl = detailView.Control;
//            ctrl.VerticalAlignment = VerticalAlignment.Top;
//            ctrl = new StackPanel { Children = { ctrl } };

//            var result = App.Windows.ShowDialog(ctrl, w =>
//            {
//                w.Buttons = ViewDialogButtons.YesNo;
//                w.SizeToContent = SizeToContent.Height;
//                w.MinHeight = 200;
//                w.MinWidth = 400;
//                w.Width = 400 * detailView.ColumnsCount;
//                w.ValidateOperations += (o, e) =>
//                {
//                    var broken = tmpEntity.ValidationRules.Validate();
//                    if (broken.Count > 0)
//                    {
//                        App.MessageBox.Show(broken.ToString(), "属性错误");
//                        e.Cancel = true;
//                    }
//                };

//                windowSetter(w);

//                w.Loaded += (o, e) =>
//                {
//                    var txt = w.GetVisualChild<TextBox>();
//                    if (txt != null) { Keyboard.Focus(txt); }
//                };

//                //窗口在数据改变后再关闭窗口，需要提示用户是否保存。
//                bool changed = false;
//                tmpEntity.PropertyChanged += (o, e) => changed = true;
//                w.Closing += (o, e) =>
//                {
//                    if (changed)
//                    {
//                        var res = App.MessageBox.Show("直接退出将不会保存数据，是否继续？", MessageBoxButton.YesNo);
//                        e.Cancel = res == MessageBoxResult.No;
//                    }
//                };

//                this.OnWindowShowing(w);
//            });

//            return result;
//        }

//        public event EventHandler<WindowShowingEventArgs> WindowShowing;

//        protected virtual void OnWindowShowing(Window win)
//        {
//            var handler = this.WindowShowing;
//            if (handler != null) handler(this, new WindowShowingEventArgs(win));
//        }

//        public class WindowShowingEventArgs : EventArgs
//        {
//            public WindowShowingEventArgs(Window window)
//            {
//                this.Window = window;
//            }

//            public Window Window { get; private set; }
//        }
//    }
//}