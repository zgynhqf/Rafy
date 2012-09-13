/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110217
 * 
*******************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace OEA.Module.WPF.Controls
{
    public abstract class TreeGridColumn : GridTreeViewColumn
    {
        /// <summary>
        /// 对应的 TreeGrid 控件
        /// </summary>
        public TreeGrid Owner { get; internal set; }

        #region HeaderLabel DependencyProperty

        public static readonly DependencyProperty HeaderLabelProperty = DependencyProperty.Register(
            "HeaderLabel", typeof(string), typeof(TreeGridColumn)
            );

        public string HeaderLabel
        {
            get { return (string)this.GetValue(HeaderLabelProperty); }
            set { this.SetValue(HeaderLabelProperty, value); }
        }

        #endregion

        #region Binding DependencyProperty

        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register(
            "Binding", typeof(BindingBase), typeof(TreeGridColumn),
            new PropertyMetadata((d, e) => (d as TreeGridColumn).OnBindingChanged(e))
            );

        public BindingBase Binding
        {
            get { return (BindingBase)this.GetValue(BindingProperty); }
            set { this.SetValue(BindingProperty, value); }
        }

        protected virtual void OnBindingChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.HeaderStringFormat))
            {
                var value = (BindingBase)e.NewValue;
                value.StringFormat = this.HeaderStringFormat;
            }
        }

        #endregion

        #region SortingProperty DependencyProperty

        public static readonly DependencyProperty SortingPropertyProperty = DependencyProperty.Register(
            "SortingProperty", typeof(string), typeof(TreeGridColumn)
            );

        public string SortingProperty
        {
            get { return (string)this.GetValue(SortingPropertyProperty); }
            set { this.SetValue(SortingPropertyProperty, value); }
        }

        #endregion

        #region DisplayTextBlockStyle DependencyProperty

        public static readonly DependencyProperty DisplayTextBlockStyleProperty = DependencyProperty.Register(
            "DisplayTextBlockStyle", typeof(Style), typeof(TreeGridColumn)
            );

        public Style DisplayTextBlockStyle
        {
            get { return (Style)this.GetValue(DisplayTextBlockStyleProperty); }
            set { this.SetValue(DisplayTextBlockStyleProperty, value); }
        }

        #endregion

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
                var nextColumn = gridColumns[index] as TreeGridColumn;
                var nextCell = GridTreeViewRow.GetRowContainingElement(cell).GetCell(nextColumn);

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
            return false;
        }

        #endregion

        #region 生成控件的外部接口

        /// <summary>
        /// 生成一个元素集
        /// 
        /// 默认生成一个有 Border 的 TextBlock
        /// </summary>
        /// <returns></returns>
        public DataTemplate GenerateCellTemplate()
        {
            var cell = new FrameworkElementFactory(typeof(MTTGCell));
            var template = this.GenerateDisplayTemplateInCell();
            cell.AppendChild(template);

            var headerLabel = this.HeaderLabel;
            if (!string.IsNullOrEmpty(headerLabel))
            {
                cell.SetValue(AutomationProperties.NameProperty, headerLabel);
            }

            return new DataTemplate
            {
                VisualTree = cell
            };
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

            var textBinding = this.Binding;

            textBlock.SetBinding(TextBlock.TextProperty, textBinding);

            textBlock.SetValue(TextBlock.StyleProperty, this.DisplayTextBlockStyle);

            return textBlock;
        }

        /// <summary>
        /// 生成这个Column使用的编辑Element
        /// 未实现
        /// </summary>
        /// <returns></returns>
        protected abstract FrameworkElement GenerateEditingElementCore();

        #endregion

        internal protected virtual void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs) { }
    }
}