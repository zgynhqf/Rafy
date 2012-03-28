/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110402
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100402
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
using System.Windows.Shapes;
using SimpleCsla;
using OEA.Library;

using OEA.MetaModel.Audit;
using OEA.Utils;
using System.ComponentModel.Composition;
using Common;
using OEA.RBAC.Security;
using OEA.RBAC;

namespace OEA.Module.WPF.Shell
{
    [Export(ComposableNames.LoginWindow, typeof(Window))]
    public partial class DefaultLoginWindow : Window
    {
        public DefaultLoginWindow()
        {
            InitializeComponent();

            title.Content = ConfigurationHelper.GetAppSettingOrDefault("登录窗口标题", "管理信息系统");

            //Loaded += (o, e) => this.DialogResult = true;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (!OEAPrincipal.Login(txtUserName.Text, StringHelper.MD5(txtPassword.Password)))
            {
                MessageBox.Show("用户或密码错误，请重新输入！", "登录失败");
            }
            else
            {
                UserLoginLogService.NotifyLogin(OEAIdentity.Current.User);
                this.DialogResult = true;
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}