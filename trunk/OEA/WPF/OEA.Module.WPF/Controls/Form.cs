/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120706 11:21
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120706 11:21
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 表单控件
    /// </summary>
    public class Form : ContentControl
    {
        #region DetailView DependencyProperty

        public static readonly DependencyProperty DetailViewProperty = DependencyProperty.Register(
            "DetailView", typeof(DetailObjectView), typeof(Form),
            new PropertyMetadata((d, e) => (d as Form).OnDetailViewChanged(e))
            );

        /// <summary>
        /// 表单控件对应的 DetailObjectView
        /// </summary>
        public DetailObjectView DetailView
        {
            get { return (DetailObjectView)this.GetValue(DetailViewProperty); }
            set { this.SetValue(DetailViewProperty, value); }
        }

        private void OnDetailViewChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (DetailObjectView)e.NewValue;
        }

        #endregion

        internal static DetailObjectView GetDetailObjectView(FrameworkElement element)
        {
            var form = element.GetLogicalParent<Form>();
            if (form == null) return null;

            return form.DetailView;
        }
    }
}