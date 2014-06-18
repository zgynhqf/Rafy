/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130419
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130419 12:40
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

namespace VSTemplates.Wizards
{
    public partial class DomainLibraryProjectWindow : Window
    {
        public DomainLibraryProjectWindow()
        {
            InitializeComponent();

            this.Loaded += DomainLibraryProjectWindow_Loaded;
        }

        void DomainLibraryProjectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txtDomainNameSpace.Focus();
            txtDomainNameSpace.SelectAll();
        }

        private void btnContinue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}