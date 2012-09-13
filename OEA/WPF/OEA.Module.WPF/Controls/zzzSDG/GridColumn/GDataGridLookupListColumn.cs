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
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using OEA.Editors;
//using OEA.MetaModel;
//using OEA.MetaModel.View;
////using OEA.Module.WPF;

//namespace OEA.Module.WPF.Editors
//{
//    public class GDataGridLookupListColumn : OpenDataGridColumn
//    {
//        protected GDataGridLookupListColumn() { }

//        protected override IWPFPropertyEditor CreateEditorCore()
//        {
//            return this.EditorFactory.Create<LookupListPropertyEditor>(this.PropertyInfo);
//        }

//        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        {
//            //如果是可输入下拉列表,非编辑状态下是绑定到自身属性上
//            var refInfo = this.PropertyInfo.ReferenceViewInfo;
//            if (string.IsNullOrEmpty(refInfo.RefEntityProperty))
//            {
//                return base.GenerateElement(cell, dataItem);
//            }
//            else
//            {
//                var result = new TextBlock();

//                var TextBinding = new Binding(refInfo.TitlePath());
//                result.SetBinding(TextBlock.TextProperty, TextBinding);

//                return result;
//            }
//        }
//    }
//}
