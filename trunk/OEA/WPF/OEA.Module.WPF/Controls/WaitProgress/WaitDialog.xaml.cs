/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101021
 * 说明：进度条
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101021
 * 
*******************************************************/

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
using System.Windows.Media.Animation;

namespace OEA.Module.WPF
{
    public partial class WaitDialog : Window
    {
        public WaitDialog()
        {
            InitializeComponent();
        }

        public ProgressValue ProgressValue
        {
            get
            {
                return new ProgressValue()
                {
                    Percent = this.waitBar.Value,
                    Text = this.txtTitle.Text
                };
            }
            set
            {
                Action setWaitBarValue = () =>
                {
                    if (value.Text != null)
                    {
                        this.txtTitle.Text = value.Text;
                    }

                    var sb = this.FindResource("MoveStoryboard") as Storyboard;
                    (sb.Children[0] as DoubleAnimationUsingKeyFrames).KeyFrames[0].Value = value.Percent;
                    sb.Begin();
                };
                this.Dispatcher.BeginInvoke(setWaitBarValue);
            }
        }
    }
}
