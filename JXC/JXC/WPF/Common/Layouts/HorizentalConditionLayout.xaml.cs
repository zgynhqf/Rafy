/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
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
using OEA.Module.WPF;
using OEA.Module.WPF.Layout;
using OEA.Module;
using OEA;

namespace JXC.WPF
{
    /// <summary>
    /// 使用左右布局来摆放主体和命令框
    /// </summary>
    public partial class HorizentalConditionLayout : UserControl, ILayoutControl
    {
        public HorizentalConditionLayout()
        {
            InitializeComponent();
        }

        public void Arrange(UIComponents components)
        {
            var control = components.Main;
            if (control != null) { content.Content = control.Control; }

            control = components.CommandsContainer;
            if (control != null) { commands.Content = control.Control; }
        }
    }
}
