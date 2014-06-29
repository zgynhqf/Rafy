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
using System.Windows.Data;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;
using Rafy;
using Rafy.Reflection;
using Rafy.ManagedProperty;
using Rafy.Utils;

namespace Rafy.WPF
{
    /// <summary>
    /// TreeColumn Factory
    /// </summary>
    public class TreeColumnFactory : NamedTypeFactory<TreeColumn>
    {
        private PropertyEditorFactory _propertyEditorFactory;

        public TreeColumnFactory(PropertyEditorFactory propertyEditorFactory)
        {
            if (propertyEditorFactory == null) throw new ArgumentNullException("propertyEditorFactory");
            this._propertyEditorFactory = propertyEditorFactory;

            //默认的列类型和column编辑器的映射关系
            this.SetDictionary(new Dictionary<string, Type>() 
            { 
                { WPFEditorNames.Memo, typeof(MemoTreeColumn) },
                { WPFEditorNames.Boolean, typeof(BooleanTreeColumn) },
                //{ WPFEditorNames.Enum, typeof(ComboBoxTreeColumn) },
                //{ WPFEditorNames.EntitySelection_DropDown, typeof(ComboBoxTreeColumn) },
                //{ WPFEditorNames.DateTime, typeof(DateTreeColumn) },
                //{ WPFEditorNames.Time, typeof(DateTreeColumn) },
                //{ WPFEditorNames.Date, typeof(DateTreeColumn) },
            });
        }

        /// <summary>
        /// 生成GridTreeView列对象
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public TreeColumn Create(WPFEntityPropertyViewMeta meta)
        {
            //根据editorName生成对应的Column
            var treeColumn = this.CreateInstance(meta.GetEditorNameOrDefault()) ?? new CommonTreeColumn();
            treeColumn.Meta = meta;

            var editor = this._propertyEditorFactory.Create(meta, true);
            treeColumn.Editor = editor;

            //使用 PropertyEditor 来生成 Binding 的原因是：
            //如果是下拉框、则不能直接使用默认的绑定方案。
            treeColumn.Binding = CreateBindingByEditor(editor);

            treeColumn.HeaderLabel = (meta.Label ?? meta.Name).Translate();
            treeColumn.PropertyName = meta.Name;
            treeColumn.SortingProperty = meta.DisplayPath();
            treeColumn.DisplayTextBlockStyle = TypeHelper.IsNumber(TypeHelper.IgnoreNullable(meta.PropertyMeta.Runtime.PropertyType)) ?
                RafyResources.TreeColumn_TextBlock_Number : RafyResources.TreeColumn_TextBlock;

            //宽度
            if (meta.GridWidth.HasValue)
            {
                treeColumn.Width = meta.GridWidth.Value;
            }

            return treeColumn;
        }

        private static Binding CreateBindingByEditor(PropertyEditor editor)
        {
            var binding = editor.CreateBindingInternal();

            var property = editor.Meta;

            var propertyMeta = property.PropertyMeta;
            if (!string.IsNullOrEmpty(property.StringFormat))
            {
                binding.StringFormat = property.StringFormat;
            }
            else
            {
                //时间类型的属性，使用默认的格式化。
                var mp = propertyMeta.ManagedProperty;
                if (mp.PropertyType == typeof(DateTime))
                {
                    var meta = mp.GetMeta(propertyMeta.Owner.EntityType) as IPropertyMetadata;
                    switch (meta.DateTimePart)
                    {
                        case DateTimePart.DateTime:
                            break;
                        case DateTimePart.Date:
                            binding.StringFormat = "d";
                            break;
                        case DateTimePart.Time:
                            binding.StringFormat = "t";
                            break;
                        default:
                            break;
                    }
                }
            }

            return binding;
        }
    }
}
