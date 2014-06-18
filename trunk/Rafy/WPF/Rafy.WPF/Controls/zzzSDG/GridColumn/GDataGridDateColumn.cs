///*******************************************************
// * 
// * 作者：李智
// * 创建时间：20100101
// * 说明：文件描述
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 李智 20100101
// * 
//*******************************************************/

//using System.Windows;
//using System.Windows.Forms;
//using System.Windows.Controls;
//namespace Rafy.WPF.Editors
//{
//    class GDataGridDateColumn : OpenDataGridColumn
//    {
//        protected GDataGridDateColumn() { }

//        protected override IWPFPropertyEditor CreateEditorCore()
//        {
//            return this.EditorFactory.Create<DatePropertyEditor>(this.PropertyInfo);
//        }

//        //protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        //{
//        //    TextBlock textBlock = new TextBlock();

//        //    Binding textBinding = new Binding(this._propertyInfo.Name)
//        //    {
//        //        StringFormat = this._propertyInfo.StringFormat
//        //    };
//        //    textBlock.SetBinding(TextBlock.TextProperty, textBinding);

//        //    var propertyType = this._propertyInfo.PropertyInfo.RuntimeProperty.PropertyType;
//        //    if (TypeHelper.IsNumber(propertyType))
//        //    {
//        //        textBlock.TextAlignment = TextAlignment.Right;
//        //    }

//        //    return textBlock;
//        //}
//    }
//}