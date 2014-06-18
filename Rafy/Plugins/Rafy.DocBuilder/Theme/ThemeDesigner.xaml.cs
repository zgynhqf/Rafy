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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Rafy.WPF;

namespace Rafy.DevTools.Theme
{
    public partial class ThemeDesigner : UserControl
    {
        public ThemeDesigner()
        {
            InitializeComponent();
        }

        private bool _boundToWindow = false;

        private void BindToWindow()
        {
            if (!this._boundToWindow)
            {
                var cmd = new RoutedCommand();

                var win = Window.GetWindow(this);
                win.InputBindings.Add(new InputBinding(cmd, new KeyGesture(Key.R, ModifierKeys.Control | ModifierKeys.Shift)));
                win.CommandBindings.Add(new CommandBinding(cmd, RefreshXamlCommand_Execute));

                this._boundToWindow = true;
            }
        }

        private void RefreshXamlCommand_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var xamlFile = this.XamlFile;
            if (!string.IsNullOrEmpty(xamlFile) && File.Exists(xamlFile))
            {
                var xaml = File.ReadAllText(xamlFile);
                var dic = XamlReader.Parse(xaml) as ResourceDictionary;
                Application.Current.Resources.MergedDictionaries.Add(dic);

                this.RefreshControl();

                App.Current.Status = "界面刷新完成。".Translate() + DateTime.Now;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.BindToWindow();

            App.MessageBox.Show("已经绑定到界面，可用 Ctrl+Shift+R 进行加载。".Translate());
        }

        private void RefreshControl()
        {
            txtPath.Text = this.XamlFile;
            txtContent.Text = File.ReadAllText(this.XamlFile);
        }

        private string XamlFile
        {
            get { return txtPath.Text; }
        }

        //生成默认的 Xaml 样式到临时文件，准备开发编辑。
        //private void btnGenerateBasicColors_Click(object sender, RoutedEventArgs e)
        //{
        //    var packUri = Helper.GetPackUri(typeof(Helper), "Resources/Colors/Blue.xaml");

        //    var dic = Application.LoadComponent(packUri);
        //    using (var writer = new StreamWriter(this.CurrentEditingFile, false))
        //    {
        //        XamlWriter.Save(dic, writer);
        //    }

        //    //var streamInfo = Application.GetResourceStream(packUri);
        //    //using (var reader = new StreamReader(streamInfo.Stream, Encoding.ASCII))
        //    //{
        //    //    var xaml = reader.ReadToEnd();
        //    //    File.WriteAllText(this.XamlFile, xaml);
        //    //}

        //    this.RefreshControl();
        //}
    }
}
