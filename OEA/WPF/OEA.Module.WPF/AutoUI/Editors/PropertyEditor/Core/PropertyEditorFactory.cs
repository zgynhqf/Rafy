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
using System.Linq;
using System.Linq.Expressions;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Utils;
using OEA.Reflection;



namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 属性编辑器工厂
    /// </summary>
    public class PropertyEditorFactory : NamedTypeFactory<WPFPropertyEditor>
    {
        public PropertyEditorFactory()
        {
            this.SetDictionary(new Dictionary<string, Type>() { 
                { WPFEditorNames.Int32, typeof(NumericUpDownPropertyEditor)},
                //{ WPFEditorNames.Int32, typeof(NumbericPropertyEditor)},
                { WPFEditorNames.NumericUpDown, typeof(NumericUpDownPropertyEditor)},
                { WPFEditorNames.NumberRange, typeof(NumberRangePropertyEditor)},
                { WPFEditorNames.Boolean, typeof(BooleanPropertyEditor)},
                { WPFEditorNames.String, typeof(StringPropertyEditor) },
                { WPFEditorNames.Enum, typeof(EnumPropertyEditor) },
                { WPFEditorNames.LookupDropDown, typeof(LookupListPropertyEditor)},
                { WPFEditorNames.Memo, typeof(MemoPropertyEditor)},
                { WPFEditorNames.Date, typeof(DatePropertyEditor)},
                { WPFEditorNames.DateRange, typeof(DateRangePropertyEditor)},
                { WPFEditorNames.LookDetail, typeof(LookDetailPropertyEditor)},
                { WPFEditorNames.PopupSearchList, typeof(PopupSearchListPropertyEditor)},
            });
        }

        /// <summary>
        /// 直接生成编辑某个实体某个属性的属性编辑器
        /// </summary>
        /// <typeparam name="TCurrentEntity"></typeparam>
        /// <param name="currentObject"></param>
        /// <param name="property">属性表达式</param>
        /// <returns></returns>
        public WPFPropertyEditor Create<TCurrentEntity>(Expression<Func<TCurrentEntity, object>> property)
        {
            var vm = UIModel.Views.CreateDefaultView(typeof(TCurrentEntity));
            var pvm = vm.Property(ExpressionHelper.GetProperty(property));
            if (pvm == null) throw new InvalidOperationException("没有找到这个属性。");

            return this.Create(pvm);
        }

        /// <summary>
        /// 为某个属性生成指定类型的属性编辑器
        /// </summary>
        /// <typeparam name="TPropertyEditor">指定使用这种编辑器类型</typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public TPropertyEditor Create<TPropertyEditor>(EntityPropertyViewMeta propertyInfo)
            where TPropertyEditor : WPFPropertyEditor
        {
            return this.Create(typeof(TPropertyEditor), propertyInfo) as TPropertyEditor;
        }

        /// <summary>
        /// 为某个属性生成指定类型的属性编辑器
        /// </summary>
        /// <param name="editorType">指定使用这种编辑器类型</param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public WPFPropertyEditor Create(Type editorType, EntityPropertyViewMeta propertyInfo)
        {
            var result = Activator.CreateInstance(editorType, true) as WPFPropertyEditor;

            this.InitPropertyEditor(result, propertyInfo);

            return result;
        }

        /// <summary>
        /// 根据属性元数据中指定的 EditorName 生成属性编辑器
        /// </summary>
        /// <param name="context"></param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public WPFPropertyEditor Create(EntityPropertyViewMeta propertyInfo)
        {
            var result = this.CreateInstance(propertyInfo.EditorName, true) ??
                this.CreateInstance(WPFEditorNames.String, true);

            this.InitPropertyEditor(result, propertyInfo);

            return result;
        }

        protected void InitPropertyEditor(WPFPropertyEditor result, EntityPropertyViewMeta propertyInfo)
        {
            result.Initialize(propertyInfo);

            result.EnsureControlsCreated();

            this.OnInstanceCreated(new InstanceCreatedEventArgs<WPFPropertyEditor>(result));
        }
    }
}