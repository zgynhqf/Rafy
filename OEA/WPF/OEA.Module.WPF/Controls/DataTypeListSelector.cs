//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Controls;
//using System.Windows;
//using System.Windows.Data;

//namespace OEA.Module.WPF.Controls
//{
//    public class DataTypeListSelector : DataTemplateSelector
//    {
//        public override DataTemplate SelectTemplate(object item, DependencyObject container)
//        {
//            DataTemplate dt = new DataTemplate();
//            FrameworkElementFactory txt = new FrameworkElementFactory(typeof(TextBlock));
//            //Binding b = new Binding(item.Name);
//            //txt.SetBinding(TextBlock.TextProperty, b);
//            dt.VisualTree = txt;
//            return dt;
//        }
//    }

//}
