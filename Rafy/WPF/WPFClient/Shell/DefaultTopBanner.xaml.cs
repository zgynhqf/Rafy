/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110309
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100309
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
using Rafy.WPF.Controls;

namespace Rafy.WPF.Shell
{
    /// <summary>
    /// 默认的主窗体顶栏
    /// </summary>
    public partial class DefaultTopBanner : UserControl
    {
        public DefaultTopBanner()
        {
            InitializeComponent();

            //Logo地址
            var logo = RafyEnvironment.BranchProvider.GetCustomerFile("Images/Logo.png");
            imgLogo.Source = new BitmapImage(new Uri(logo));

            //用户名
            txtUserName.Text = RafyEnvironment.Identity.Name;
        }

        private void btnModifyPwd_Click(object sender, RoutedEventArgs e)
        {
            var win = new WPFClient.Shell.TreeGridTest();
            win.Show();
            //ModifyUserPasswordDialog.Execute(RafyIdentity.Current.User);
        }
    }
}
