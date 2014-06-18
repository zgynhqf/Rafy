/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 15:53
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 15:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
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
using Rafy.DomainModeling;
using Rafy.DomainModeling.Models;

namespace ModelingEnv
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
            {
                var file = args[1];
                LoadFile(file);
            }
        }

        private void ExePath_PreviewDrag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Link;
            e.Handled = true;
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "ODML files (*.odml)|*.odml";
            var res = dialog.ShowDialog();
            if (res == true)
            {
                LoadFile(dialog.FileName);
            }
        }

        private void LoadFile(string file)
        {
            viewer.LoadDocument(file);
            viewer.Visibility = Visibility.Visible;
            this.Title = System.IO.Path.GetFileName(file) + "（" + this.Title + "）";
        }
    }
}