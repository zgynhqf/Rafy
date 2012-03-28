using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Reflection;
using OEA.MetaModel.Attributes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using OEA.Module.View;

namespace OEA.Module.WPF.Behaviors
{
    /// <summary>
    /// 在双击视图的时候，关闭当前窗口
    /// </summary>
    public class DoubleCloseViewBehavior : ViewBehavior
    {
        public DoubleCloseViewBehavior() { }

        protected override void OnAttach()
        {
            (this.View as ListObjectView).MouseDoubleClick += (o, e) =>
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
