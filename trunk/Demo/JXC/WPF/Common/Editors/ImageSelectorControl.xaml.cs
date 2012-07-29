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
using System.IO;

namespace JXC.WPF
{
    public partial class ImageSelectorControl : UserControl
    {
        public ImageSelectorControl()
        {
            InitializeComponent();
        }

        private void btnSelector_Click(object sender, RoutedEventArgs e)
        {
            var fd = new OpenFileDialog();
            if (fd.ShowDialog() == true)
            {
                var bytes = File.ReadAllBytes(fd.FileName);
                this.SetCurrentValue(ImageBytesProperty, bytes);
            }
        }

        #region ImageBytes DependencyProperty

        public static readonly DependencyProperty ImageBytesProperty = DependencyProperty.Register(
            "ImageBytes", typeof(byte[]), typeof(ImageSelectorControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, (d, e) => (d as ImageSelectorControl).OnImageBytesChanged(e))
            );

        public byte[] ImageBytes
        {
            get { return (byte[])this.GetValue(ImageBytesProperty); }
            set { this.SetValue(ImageBytesProperty, value); }
        }

        private void OnImageBytesChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (byte[])e.NewValue;
            if (value != null && value.Length > 0)
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.StreamSource = new MemoryStream(value);
                source.EndInit();

                img.Source = source;

                var binding = BindingOperations.GetBinding(this, ImageBytesProperty);
                var exp = this.GetBindingExpression(ImageBytesProperty);
                exp.UpdateSource();
            }
        }

        #endregion
    }
}
