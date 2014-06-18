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
//using Rafy.Library;
//using Rafy.MetaModel;
//using Rafy.MetaModel.View;
//using Rafy.Utils;

//namespace Rafy.WPF.Editors
//{
//    /// <summary>
//    /// GridColumn基类
//    /// </summary>
//    public abstract class OpenDataGridColumn : DataGridColumn
//    {
//        private WPFEntityPropertyViewMeta _propertyInfo;

//        protected PropertyEditorFactory EditorFactory { get; private set; }

//        internal protected virtual void Intialize(WPFEntityPropertyViewMeta info, PropertyEditorFactory editorFactory)
//        {
//            if (info == null) throw new ArgumentNullException("info");

//            this._propertyInfo = info;
//            this.EditorFactory = editorFactory;

//            //这里和TreeColumn不同的是，Editor可以被重用，所以在初始化时使用一个就行了。
//            this.Editor = this.CreateEditorCore();
//        }

//        /// <summary>
//        /// 默认没有Editor
//        /// </summary>
//        protected IWPFPropertyEditor Editor { get; private set; }

//        protected virtual IWPFPropertyEditor CreateEditorCore() { return null; }

//        /// <summary>
//        /// 正在编辑的属性
//        /// </summary>
//        public WPFEntityPropertyViewMeta PropertyInfo
//        {
//            get { return this._propertyInfo; }
//        }

//        internal void UpdateVisibility()
//        {
//            if (this.Editor.IsVisible)
//            {
//                this.Visibility = Visibility.Visible;
//            }
//            else
//            {
//                this.Visibility = Visibility.Collapsed;
//            }
//        }

//        internal void UpdateVisibility(object currData)
//        {
//            var visibilityIndicator = this.Editor.PropertyViewInfo.VisibilityIndicator;
//            if (visibilityIndicator.IsDynamic)
//            {
//                if (currData != null &&
//                    currData.GetPropertyValue<bool>(visibilityIndicator.PropertyName))
//                {
//                    this.Visibility = Visibility.Visible;
//                }
//                else
//                {
//                    this.Visibility = Visibility.Collapsed;
//                }
//            }
//            else
//            {
//                this.Visibility = visibilityIndicator.VisiblityType == VisiblityType.AlwaysShow ?
//                    Visibility.Visible : Visibility.Collapsed;
//            }
//        }

//        /// <summary>
//        /// 默认以TextBlock显示
//        /// </summary>
//        /// <param name="cell"></param>
//        /// <param name="dataItem"></param>
//        /// <returns></returns>
//        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
//        {
//            TextBlock textBlock = new TextBlock();
//            textBlock.SetBinding(TextBlock.TextProperty, new Binding(this._propertyInfo.Name)
//            {
//                StringFormat = this._propertyInfo.StringFormat
//            });

//            var propertyType = this._propertyInfo.PropertyMeta.Runtime.PropertyType;
//            if (TypeHelper.IsNumber(propertyType))
//            {
//                textBlock.SetValue(FrameworkElement.StyleProperty, RafyStyles.GridColumn_TextBlock_Number);
//            }
//            else
//            {
//                textBlock.SetValue(FrameworkElement.StyleProperty, this.DisplayTextBlockStyle);
//            }

//            return textBlock;
//        }

//        protected virtual Style DisplayTextBlockStyle
//        {
//            get { return RafyStyles.GridColumn_TextBlock; }
//        }

//        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
//        {
//            //如果是 IsReadonly 就不生成编辑控件了。
//            if (this.IsReadonly(dataItem))
//            {
//                return cell.Content as FrameworkElement;
//            }

//            //如果没有编辑器，则不能生成编辑控件
//            if (this.Editor == null) throw new NotSupportedException("只读！");

//            var fe = this.Editor.Control;

//            fe.RemoveFromParent(false);

//            return fe;
//        }

//        /// <summary>
//        /// 代理 PrepareCellForEdit 方法到 Editor 中。
//        /// </summary>
//        /// <param name="editingElement"></param>
//        /// <param name="editingEventArgs"></param>
//        /// <returns></returns>
//        protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
//        {
//            //如果是 IsReadonly 就不生成编辑控件了。
//            if (this.IsReadonly(editingElement.DataContext))
//            {
//                return base.PrepareCellForEdit(editingElement, editingEventArgs);
//            }

//            //DataGrid 在每次创建单元格时都需要执行此代码。原因不详。
//            this.Editor.RebindEditingControl();

//            this.Editor.PrepareElementForEdit(editingElement, editingEventArgs);

//            return base.PrepareCellForEdit(editingElement, editingEventArgs);
//        }

//        private bool IsReadonly(object dataItem)
//        {
//            return PropertyEditorHelper.CheckIsReadonly(dataItem as Entity, this.PropertyInfo);
//        }
//    }
//}