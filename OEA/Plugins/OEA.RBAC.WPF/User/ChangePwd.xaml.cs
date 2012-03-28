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
using OEA.Library;
using SimpleCsla.OEA;
using System.Security.Cryptography;
using OEA.Utils;
using OEA.Command;
using OEA.RBAC;

namespace OEA.Module.WPF
{
    public partial class ChangePwd : Window
    {
        public static void Execute(User user)
        {
            var dialog = new ChangePwd(user);
            dialog.ShowDialog();
        }

        private User user;
        public ChangePwd(User user)
        {
            InitializeComponent();
            this.user = user;
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (StringHelper.MD5(txtOldPassword.Password) != user.Password)
            {
                App.Current.MessageBox.Show("旧密码输入错误，请重新输入!", "错误");
                return;
            }
            else if (txtNewPassword1.Password != txtNewPassword2.Password)
            {
                App.Current.MessageBox.Show("两次输入的新密码不一致，请重新输入!", "错误");
                return;
            }
            user.Password = StringHelper.MD5(txtNewPassword1.Password);

            RF.Save(user);

            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
