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
//using OEA.MetaModel;
//using OEA.MetaModel.View;
////using OEA.Module.WPF.Controls;
//using System.Drawing;

//namespace OEA.Module.WPF.Editors
//{
//    /// <summary>
//    /// Column Factory
//    /// </summary>
//    public class GridColumnFactory : NamedTypeFactory<OpenDataGridColumn>
//    {
//        private PropertyEditorFactory _propertyEditorFactory;

//        public GridColumnFactory(PropertyEditorFactory propertyEditorFactory)
//        {
//            if (propertyEditorFactory == null) throw new ArgumentNullException("propertyEditorFactory");
//            this._propertyEditorFactory = propertyEditorFactory;

//            this.SetDictionary(new Dictionary<string, Type>() { 
//                { EditorNames.String, typeof(GDataGridStringColumn) },
//                { EditorNames.Enum, typeof(GDataGridEnumColumn) },
//                { EditorNames.LookupDropDown, typeof(GDataGridLookupListColumn) },
//                { EditorNames.Memo, typeof(GDataGridMemoColumn)},
//                { EditorNames.Boolean,typeof(GDataGridBooleanColumn)},
//                { EditorNames.Date, typeof(GDataGridDateColumn)},
//                { EditorNames.LookDetail, typeof(GLookDetailColumn)},
//            });
//        }

//        /// <summary>
//        /// 生成一个可用于编辑info的列编辑器
//        /// </summary>
//        /// <param name="property"></param>
//        /// <returns></returns>
//        public DataGridColumn GetDefaultColumn(EntityPropertyViewMeta property)
//        {
//            var result = this.CreateInstance(property.EditorName) ??
//                new GDataGridCommonColumn();

//            result.Intialize(property, this._propertyEditorFactory);

//            // 设置排序字段
//            Type propertyType = property.PropertyMeta.Runtime.PropertyType;
//            if (!typeof(IComparable).IsAssignableFrom(propertyType))
//            {
//                result.CanUserSort = false;
//            }
//            else
//            {
//                result.SortMemberPath = property.Name;
//            }

//            result.Header = property.Label;

//            int colWith = 10 * (property.Label.Length + 2);
//            result.MinWidth = colWith > 100 ? 100 : colWith;

//            if (property.GridWidth.HasValue)
//            {
//                result.Width = new DataGridLength(property.GridWidth.Value);
//            }

//            return result;
//        }
//    }
//}
