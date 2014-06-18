/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120606 11:49
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120606 11:49
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
using Microsoft.Win32;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace Rafy.DevTools.Document
{
    public partial class PropertyDocBuilderUI : UserControl
    {
        public PropertyDocBuilderUI()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            var parser = new PropertyDocParser();
            parser.EntityViewMeta = UIModel.Views.CreateBaseView(typeof(TestDocument));
            parser.ClassContent = System.IO.File.ReadAllText(@"D:\_Code\Rafy\Rafy\Plugins\Rafy.DocBuilder\TestDocument.cs");
            parser.Parse();

            //var dialog = new OpenFileDialog
            //{
            //    Filter = "*.csproj",
            //};
            //var res = dialog.ShowDialog();
            //if (res == true)
            //{
            //    var fileName = dialog.FileName;

            //}
        }
    }
}
