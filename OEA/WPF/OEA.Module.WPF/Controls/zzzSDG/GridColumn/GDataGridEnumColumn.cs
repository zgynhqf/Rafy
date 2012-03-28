///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110217
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100217
// * 
//*******************************************************/

//using System;

//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using OEA.Module.WPF.Converter;
//using OEA.Utils;

//namespace OEA.Module.WPF.Editors
//{
//    class GDataGridEnumColumn : OpenDataGridColumn
//    {
//        protected GDataGridEnumColumn() { }

//        protected override IWPFPropertyEditor CreateEditorCore()
//        {
//            return this.EditorFactory.Create<EnumPropertyEditor>(this.PropertyInfo);
//        }

//        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        {
//            var tb = new TextBlock();

//            var textBinding = new Binding(PropertyInfo.Name);
//            textBinding.Converter = new EnumConverter();
//            tb.SetBinding(TextBlock.TextProperty, textBinding);

//            return tb;
//        }
//    }
//}
