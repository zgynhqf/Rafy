/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：20130330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130330
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace DesignerEngine
{
    // Represents a selectable item in the Toolbox.
    public class ToolboxItem : ContentControl
    {
        // caches the start point of the drag operation
        private Point? dragStartPoint = null;

        static ToolboxItem()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolboxItem), new FrameworkPropertyMetadata(typeof(ToolboxItem)));
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.dragStartPoint = new Point?(e.GetPosition(this));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed)
                this.dragStartPoint = null;

            if (this.dragStartPoint.HasValue)
            {
                // XamlWriter.Save() has limitations in exactly what is serialized,
                // see SDK documentation; short term solution only;
                string xamlString = XamlWriter.Save(this.Content);
                DragObject dataObject = new DragObject();
                dataObject.Xaml = xamlString;

                WrapPanel panel = VisualTreeHelper.GetParent(this) as WrapPanel;
                if (panel != null)
                {
                    // desired size for DesignerCanvas is the stretched Toolbox item size
                    double scale = 1.3;
                    dataObject.DesiredSize = new Size(panel.ItemWidth * scale, panel.ItemHeight * scale);
                }

                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.Copy);

                e.Handled = true;
            }
        }
    }

    // Wraps info of the dragged object into a class
    public class DragObject
    {
        // Xaml string that represents the serialized content
        public String Xaml { get; set; }

        // Defines width and height of the DesignerItem
        // when this DragObject is dropped on the DesignerCanvas
        public Size? DesiredSize { get; set; }
    }
}