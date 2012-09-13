///*******************************************************
// * 
// * 作者：李智
// * 创建时间：20100101
// * 说明：查看详细信息
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 李智 20100101
// * 
//*******************************************************/

//using System.Windows.Controls;
//using System.Windows;
//using System.Windows.Data;
//using OEA.MetaModel;
//using OEA.MetaModel.View;
//using System.Windows.Media.Imaging;
//using System;
//using System.Windows.Media;
//using OEA.Module.WPF.Controls;
//using OEA.Library;

//namespace OEA.Module.WPF.Editors
//{
//    class GLookDetailColumn : OpenDataGridColumn
//    {
//        private static string imagePath = "Images/";
//        private const double MAXHEIGHT = 14.4;

//        protected GLookDetailColumn() { }

//        protected override IWPFPropertyEditor CreateEditorCore()
//        {
//            return this.EditorFactory.Create<LookDetailPropertyEditor>(this.PropertyInfo);
//        }

//        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        {
//            Image showdetail = new Image();
//            BitmapImage bi = new BitmapImage();
//            bi.BeginInit();

//            Uri uri = CoverUri("OpenFile.bmp");
//            bi.UriSource = uri;
//            bi.EndInit();
//            showdetail.Stretch = Stretch.None;
//            showdetail.Source = bi;

//            return showdetail;
//        }

//        private Uri CoverUri(string fileName)
//        {
//            string imageSource = this.GetType().Assembly.GetName().Name;
//            return new Uri(string.Concat(
//                         "pack://application:,,,/",
//                         imageSource,
//                         ";Component/",
//                         imagePath,
//                         fileName));
//        }
//    }
//}