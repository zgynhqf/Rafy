/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Controls;
using OEA.Utils;
using System.Windows.Media;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using OEA.Reflection;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 树型表的列的基类
    /// </summary>
    public abstract class TreeColumn : GridTreeViewColumn
    {
        #region 字段

        /// <summary>
        /// 该字段可能为空，表示该列没有对应任何元数据。
        /// </summary>
        private EntityPropertyViewMeta _propertyInfo;

        private PropertyEditorFactory _editorFactory;

        #endregion

        protected TreeColumn() { }

        internal void IntializeViewMeta(EntityPropertyViewMeta info, PropertyEditorFactory editorFactory)
        {
            this._propertyInfo = info;
            this._editorFactory = editorFactory;
        }

        #region Editor

        /// <summary>
        /// 默认没有Editor
        /// </summary>
        protected IWPFPropertyEditor Editor { get; private set; }

        protected virtual IWPFPropertyEditor CreateEditorCore(PropertyEditorFactory factory) { return null; }

        private void CreateNewEditor()
        {
            this.Editor = this.CreateEditorCore(this._editorFactory);
        }

        #endregion

        /// <summary>
        /// 正在编辑的属性
        /// </summary>
        public EntityPropertyViewMeta PropertyInfo
        {
            get { return this._propertyInfo; }
        }

        /// <summary>
        /// 重设列对应到新的元数据上
        /// </summary>
        /// <param name="value"></param>
        internal void ResetPropertyInfo(EntityPropertyViewMeta value)
        {
            /*********************** 代码块解释 *********************************
             * 该属性可以被设置。
             * 这是因为在多类型 TreeGrid 中，同样的列可能被多个不同的实体所重用。
            **********************************************************************/

            this._propertyInfo = value;
        }

        /// <summary>
        /// 对应的 TreeGrid 控件
        /// </summary>
        public MultiTypesTreeGrid Owner { get; internal set; }

        #region 编辑状态

        /// <summary>
        /// 在编辑控件按 Tab 切换到下一列
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="e"></param>
        internal void EditNextColumnOnTabKey(MTTGCell cell, KeyEventArgs e)
        {
            var gridColumns = this.Owner.Columns;

            var index = gridColumns.IndexOf(this) + 1;

            //循环：后果是最后一个，则使用第一个
            bool looped = false;
            if (index == gridColumns.Count)
            {
                index = 0;
                looped = true;
            }

            //如果在范围内，则尝试编辑
            if (index < gridColumns.Count)
            {
                var nextColumn = gridColumns[index] as TreeColumn;
                var nextCell = cell.Row.Cells.First(c => c.Column == nextColumn);

                var success = nextColumn.TryBeginEdit(nextCell, e);
                if (success)
                {
                    e.Handled = true;
                    return;
                }

                //如果 nextCell 编辑失败，并且不是循环第二次，则继续编辑下一个。
                if (!looped) nextColumn.EditNextColumnOnTabKey(nextCell, e);
            }
        }

        internal bool TryBeginEdit(MTTGCell cell, RoutedEventArgs editingEventArgs)
        {
            //如果是 IsReadonly 就不能进入编辑状态。
            if (this.ForceEditing || !this.TryCheckIsReadonly(cell.DataContext))
            {
                return this.Owner.TryEditCell(cell, editingEventArgs);
            }

            return false;
        }

        /// <summary>
        /// 当前列是否需要不检测只读属性而强制进入编辑状态。
        /// </summary>
        protected virtual bool ForceEditing
        {
            get { return false; }
        }

        protected virtual bool TryCheckIsReadonly(object dataItem)
        {
            //如果一个树用于多个对象,此方法不适用，需要切换PropertyInfo
            if (dataItem != null && dataItem.GetType() == this.PropertyInfo.Owner.EntityType)
            {
                return PropertyEditorHelper.CheckIsReadonly(dataItem as Entity, this.PropertyInfo);
            }

            return false;
        }

        /// <summary>
        /// 提交当前的编辑状态，更新绑定的数据源。
        /// </summary>
        public virtual void CommitEdit() { }

        internal protected virtual void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            if (this.Editor != null)
            {
                this.Editor.PrepareElementForEdit(editingElement, editingEventArgs);
            }
        }

        #endregion

        #region 生成控件的外部接口

        /// <summary>
        /// 生成一个元素集
        /// 
        /// 默认生成一个有 Border 的 TextBlock
        /// </summary>
        /// <returns></returns>
        internal FrameworkElementFactory GenerateDisplayTemplate()
        {
            var template = this.GenerateDisplayTemplateInCell();

            var cellContainer = MTTGCell.Wrap(template, this);
            cellContainer.SetValue(AutomationProperties.NameProperty, this._propertyInfo.Label);

            return cellContainer;
        }

        /// <summary>
        /// 生成这个Column使用的Element
        /// 默认生成一个TextBlock，并绑定到属性
        /// </summary>
        /// <returns></returns>
        internal FrameworkElement GenerateDisplayElement()
        {
            var template = this.GenerateDisplayTemplateInCell();
            return template.LoadContent();
        }

        internal FrameworkElement GenerateEditingElement()
        {
            //这里 Editor 不可以被重用，所以必须每次都构造一个。
            this.CreateNewEditor();

            var editingElement = this.GenerateEditingElementCore();

            return editingElement;
        }

        protected virtual Binding GenerateBindingFormat(string name, string stringformat)
        {
            return new Binding(name)
            {
                StringFormat = stringformat
            };
        }

        #endregion

        #region 生成确切控件的虚方法

        /// <summary>
        /// 生成一个控件集合，用于显示。
        /// 这个集合在GenerateFrameworkElementFactory方法中会被加上Border显示出来。
        /// </summary>
        /// <returns></returns>
        protected virtual FrameworkElementFactory GenerateDisplayTemplateInCell()
        {
            var textBlock = new FrameworkElementFactory(typeof(TextBlock));

            var textBinding = GenerateBindingFormat(this._propertyInfo.Name, this._propertyInfo.StringFormat);

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);

            var propertyType = this._propertyInfo.PropertyMeta.Runtime.PropertyType;
            if (TypeHelper.IsNumber(propertyType))
            {
                textBlock.SetValue(TextBlock.StyleProperty, OEAStyles.TreeColumn_TextBlock_Number);
            }
            else
            {
                textBlock.SetValue(TextBlock.StyleProperty, OEAStyles.TreeColumn_TextBlock);
            }

            return textBlock;
        }

        /// <summary>
        /// 生成这个Column使用的编辑Element
        /// 未实现
        /// </summary>
        /// <returns></returns>
        protected virtual FrameworkElement GenerateEditingElementCore()
        {
            var editor = this.Editor;
            if (editor == null) throw new ArgumentNullException("只读！");

            //由于树形可以包含多个对象，一个列可能编辑多个对象，所以现在简单处理为每次重新生成控件
            var control = editor.Control;

            return control;
        }

        #endregion

        internal void UpdateVisibility(object currData)
        {
            if (this._propertyInfo != null)
            {
                bool isVisible = false;
                var visibilityIndicator = this._propertyInfo.VisibilityIndicator;

                //如果是动态计算，则尝试从数据中获取是否可见的值。
                if (visibilityIndicator.IsDynamic)
                {
                    //实体类型与属性的实体类型一致时，才进行计算。
                    var propertyOwnerType = this._propertyInfo.Owner.EntityType;
                    if (currData != null && propertyOwnerType.IsAssignableFrom(currData.GetType()))
                    {
                        isVisible = currData.GetPropertyValue<bool>(visibilityIndicator.PropertyName);
                    }
                    else
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    isVisible = visibilityIndicator.VisiblityType == VisiblityType.AlwaysShow;
                }

                this.IsVisible = isVisible;
            }
        }
    }
}