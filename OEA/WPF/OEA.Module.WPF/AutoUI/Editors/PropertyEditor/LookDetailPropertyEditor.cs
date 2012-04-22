/*******************************************************
 * 
 * 作者：李智
 * 创建时间：20100101
 * 说明：生成查看详细信息的菜单
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 李智 20100101
 * 
*******************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Media;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Controls;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 一个用于弹出当前对象详细面板的属性编辑器
    /// </summary>
    public class LookDetailPropertyEditor : WPFPropertyEditor
    {
        private static string imagePath = "Images/";

        public LookDetailPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            Image showdetail = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();

            string imageSource = this.GetType().Assembly.GetName().Name;
            Uri uri = CoverUri("OpenFile.bmp");
            bi.UriSource = uri;
            bi.EndInit();
            showdetail.Stretch = Stretch.None;
            showdetail.Source = bi;
            return showdetail;
        }

        protected override void ResetBinding(FrameworkElement editingControl) { }

        protected override DependencyProperty BindingProperty() { return null; }

        #region 以下代码拷贝自：DataGridTextColumn 类。

        protected override void PrepareElementForEditCore(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            OnButtonClick(editingEventArgs);
        }


        #endregion

        #region 弹出信息信息

        private void PopupDetail()
        {
            var curListEntity = this.Context.CurrentObject;
            Type currType = curListEntity.GetType();
            var tmp = Entity.New(currType);
            tmp.Clone(curListEntity);

            //弹出窗体显示详细面板
            var viewMeta = UIModel.Views.Create(currType);
            viewMeta.NotAllowEdit = true;

            var detailView = AutoUI.ViewFactory.CreateDetailObjectView(viewMeta);
            detailView.Data = tmp;
            detailView.Control.VerticalAlignment = VerticalAlignment.Top;

            App.Windows.ShowDialog(detailView.Control, w =>
            {
                w.Title = "详细信息";
                w.Buttons = ViewDialogButtons.Close;
                w.SizeToContent = SizeToContent.Height;
                w.MinHeight = 100;
                w.MinWidth = 200;
                w.Width = 400 * detailView.ColumnsCount;
            });
        }

        protected virtual void OnButtonClick(RoutedEventArgs e)
        {
            PopupDetail();
        }

        private Uri CoverUri(string fileName)
        {
            string imageSource = this.GetType().Assembly.GetName().Name;
            return new Uri(string.Concat(
                         "pack://application:,,,/",
                         imageSource,
                         ";Component/",
                         imagePath,
                         fileName));
        }

        #endregion
    }
}