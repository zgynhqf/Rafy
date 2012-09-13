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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using OEA.MetaModel.View;
using OEA.Module.WPF.Controls;
using OEA.Module.WPF.Editors;
using OEA.Reflection;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 类似GridColumnsFactory
    /// </summary>
    public class TreeColumnFactory : NamedTypeFactory<TreeColumn>
    {
        private PropertyEditorFactory _propertyEditorFactory;

        public TreeColumnFactory(PropertyEditorFactory propertyEditorFactory)
        {
            if (propertyEditorFactory == null) throw new ArgumentNullException("propertyEditorFactory");
            this._propertyEditorFactory = propertyEditorFactory;

            //默认的列类型和column编辑器的映射关系
            this.SetDictionary(new Dictionary<string, Type>() { 
                { WPFEditorNames.Enum, typeof(EnumTreeColumn) },
                { WPFEditorNames.EntitySelection_DropDown, typeof(LookupListTreeColumn)},
                { WPFEditorNames.Memo, typeof(MemoTreeColumn) },
                { WPFEditorNames.Boolean, typeof(BooleanTreeColumn) },
                { WPFEditorNames.DateTime, typeof(DateTreeColumn)},
                { WPFEditorNames.Time, typeof(DateTreeColumn)},
                { WPFEditorNames.Date, typeof(DateTreeColumn)},
            });
        }

        /// <summary>
        /// 生成GridTreeView列对象
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public TreeColumn Create(EntityPropertyViewMeta property)
        {
            //根据editorName生成对应的Column
            var treeColumn = this.CreateInstance(property.GetEditorNameOrDefault()) ?? new CommonTreeColumn();
            treeColumn.IntializeViewMeta(property, this._propertyEditorFactory);

            SetColumnHeader(treeColumn, property);

            treeColumn.CellTemplate = treeColumn.GenerateCellTemplate();
            treeColumn.SortingProperty = property.Name;

            if (TypeHelper.IsNumber(property.PropertyMeta.Runtime.PropertyType))
            {
                treeColumn.DisplayTextBlockStyle = OEAResources.TreeColumn_TextBlock_Number;
            }
            else
            {
                treeColumn.DisplayTextBlockStyle = OEAResources.TreeColumn_TextBlock;
            }

            //宽度
            if (property.GridWidth.HasValue)
            {
                treeColumn.Width = property.GridWidth.Value;
            }

            return treeColumn;
        }

        private static void SetColumnHeader(TreeColumn treeGridColumn, EntityPropertyViewMeta property)
        {
            string header = property.Label ?? property.Name;

            SetColumnHeaderString(treeGridColumn, header);

            //使用 PropertyEditor 来生成 Binding 的原因是：
            //如果是下拉框、则不能直接使用默认的绑定方案。
            treeGridColumn.HeaderStringFormat = property.StringFormat;
            treeGridColumn.Binding = treeGridColumn.Editor.CreateBindingInternal();
        }

        internal static void SetColumnHeaderString(TreeColumn treeGridColumn, string header)
        {
            //列头显示
            treeGridColumn.HeaderLabel = header;
            treeGridColumn.Header = new GridTreeViewColumnHeader
            {
                Content = header
            };
        }
    }
}
