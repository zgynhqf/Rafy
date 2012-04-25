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
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
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
        private EntityPropertyViewMeta _meta;

        #endregion

        protected TreeColumn() { }

        internal void IntializeViewMeta(EntityPropertyViewMeta meta, PropertyEditorFactory editorFactory)
        {
            this._meta = meta;

            this.Editor = editorFactory.Create(meta, true);
        }

        /// <summary>
        /// 默认没有Editor
        /// </summary>
        public WPFPropertyEditor Editor { get; private set; }

        /// <summary>
        /// 正在编辑的属性
        /// </summary>
        public EntityPropertyViewMeta Meta
        {
            get { return this._meta; }
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
                var nextCell = cell.Row.GetCell(nextColumn);

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
            if (dataItem != null && dataItem.GetType() == this.Meta.Owner.EntityType)
            {
                return PropertyEditorHelper.CheckIsReadonly(dataItem as Entity, this.Meta);
            }

            return false;
        }

        /// <summary>
        /// 提交当前的编辑状态，更新绑定的数据源。
        /// </summary>
        public void CommitEdit()
        {
            var editor = this.Editor;
            var dp = editor.GetBindingProperty();
            if (dp != null)
            {
                //需要把焦点从这个控件上移除，否则用户输入的值还没有同步到控件上，这时 UpdateSource 无用。
                //象 TextBox、NumericUpDown 这些控件都是需要焦点移除后才起使用的。
                //（NumericUpDown 在输入模式下是这样，内部其实也是 TextBox）
                var ctrl = editor.Control;
                ctrl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                //Keyboard.ClearFocus();

                var exp = ctrl.GetBindingExpression(dp);
                exp.UpdateSource();
            }
        }

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
            var l = this._meta.Label;
            if (l != null) { cellContainer.SetValue(AutomationProperties.NameProperty, l); }

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
            return this.GenerateEditingElementCore();
        }

        protected virtual Binding GenerateBindingFormat(string name, string stringformat)
        {
            //使用 PropertyEditor 来生成 Binding 的原因是：
            //如果是下拉框、则不能直接使用默认的绑定方案。
            var binding = this.Editor.CreateBindingInternal();

            binding.StringFormat = stringformat;

            return binding;
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

            var textBinding = GenerateBindingFormat(this._meta.Name, this._meta.StringFormat);

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);

            var propertyType = this._meta.PropertyMeta.Runtime.PropertyType;
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

        internal void UpdateVisibility(Entity currData)
        {
            if (this._meta != null)
            {
                bool isVisible = false;
                var visibilityIndicator = this._meta.VisibilityIndicator;

                //如果是动态计算，则尝试从数据中获取是否可见的值。
                if (visibilityIndicator.IsDynamic)
                {
                    if (currData != null)
                    {
                        isVisible = (bool)currData.GetProperty(visibilityIndicator.Property);
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