using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;


namespace Rafy.WPF.Behaviors
{
    /// <summary>
    /// 在双击视图的时候，关闭当前窗口
    /// </summary>
    public class DoubleCloseViewBehavior : ViewBehavior
    {
        public DoubleCloseViewBehavior() { }

        protected override void OnAttach()
        {
            (this.View.Control as Control).MouseDoubleClick += (o, e) =>
            {
                this.CloseWindow();
            };
        }

        private void CloseWindow()
        {
            foreach (Window win in Application.Current.Windows)
            {
                if (win.IsActive)
                {
                    win.DialogResult = true;
                    win.Close();
                }
            }
        }
    }
}
