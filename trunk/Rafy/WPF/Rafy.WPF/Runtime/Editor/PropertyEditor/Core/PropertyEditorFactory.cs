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
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Reflection;
using Rafy.Utils;
using Rafy;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 属性编辑器工厂
    /// </summary>
    public class PropertyEditorFactory : NamedTypeFactory<PropertyEditor>
    {
        public PropertyEditorFactory()
        {
            this.SetDictionary(new Dictionary<string, Type>() { 
                { WPFEditorNames.Int32, typeof(IntegerUpDownPropertyEditor)},
                //{ WPFEditorNames.Int32, typeof(NumbericPropertyEditor)},//暂时不用。
                { WPFEditorNames.Double, typeof(DoubleUpDownPropertyEditor)},
                { WPFEditorNames.IntegerUpDown, typeof(IntegerUpDownPropertyEditor)},
                { WPFEditorNames.NumberRange, typeof(NumberRangePropertyEditor)},
                { WPFEditorNames.Boolean, typeof(BooleanPropertyEditor)},
                { WPFEditorNames.String, typeof(StringPropertyEditor) },
                { WPFEditorNames.Enum, typeof(EnumPropertyEditor) },
                { WPFEditorNames.Memo, typeof(MemoPropertyEditor)},
                { WPFEditorNames.DateTime, typeof(DateTimePropertyEditor)},
                { WPFEditorNames.Date, typeof(DatePropertyEditor)},
                { WPFEditorNames.Time, typeof(TimePropertyEditor)},
                { WPFEditorNames.DateRange, typeof(DateRangePropertyEditor)},
                { WPFEditorNames.LookDetail, typeof(LookDetailPropertyEditor)},
                { WPFEditorNames.EntitySelection_DropDown, typeof(LookupListPropertyEditor)},
                { WPFEditorNames.EntitySelection_Popup, typeof(PopupSearchListPropertyEditor)},
                { WPFEditorNames.EntitySelection_TiledList, typeof(TiledListReferenceEditor)},
            });
        }

        /// <summary>
        /// 直接生成编辑某个实体某个属性的属性编辑器
        /// </summary>
        /// <typeparam name="TCurrentEntity"></typeparam>
        /// <param name="currentObject"></param>
        /// <param name="property">属性表达式</param>
        /// <returns></returns>
        public PropertyEditor Create<TCurrentEntity>(Expression<Func<TCurrentEntity, object>> property, bool forList)
        {
            var vm = UIModel.Views.CreateBaseView(typeof(TCurrentEntity));
            var pvm = vm.Property(ExpressionHelper.GetProperty(property)) as WPFEntityPropertyViewMeta;
            if (pvm == null) throw new InvalidOperationException("没有找到这个属性。");

            return this.Create(pvm, forList);
        }

        /// <summary>
        /// 为某个属性生成指定类型的属性编辑器
        /// </summary>
        /// <typeparam name="TPropertyEditor">指定使用这种编辑器类型</typeparam>
        /// <param name="propertyInfo"></param>
        /// <param name="forList"></param>
        /// <returns></returns>
        public TPropertyEditor Create<TPropertyEditor>(WPFEntityPropertyViewMeta propertyInfo, bool forList = true)
            where TPropertyEditor : PropertyEditor
        {
            return this.Create(typeof(TPropertyEditor), propertyInfo, forList) as TPropertyEditor;
        }

        /// <summary>
        /// 为某个属性生成指定类型的属性编辑器
        /// </summary>
        /// <param name="editorType">指定使用这种编辑器类型</param>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public PropertyEditor Create(Type editorType, WPFEntityPropertyViewMeta propertyInfo, bool forList)
        {
            var result = Activator.CreateInstance(editorType, true) as PropertyEditor;

            this.InitPropertyEditor(result, propertyInfo, forList);

            return result;
        }

        /// <summary>
        /// 根据属性元数据中指定的 EditorName 生成属性编辑器
        /// </summary>
        /// <param name="property"></param>
        /// <param name="forList"></param>
        /// <returns></returns>
        public PropertyEditor Create(WPFEntityPropertyViewMeta property, bool forList)
        {
            var result = this.CreateInstance(property.GetEditorNameOrDefault(), true) ??
                this.CreateInstance(WPFEditorNames.String, true);

            this.InitPropertyEditor(result, property, forList);

            return result;
        }

        protected void InitPropertyEditor(PropertyEditor result, WPFEntityPropertyViewMeta propertyInfo, bool forList)
        {
            result._context.IsForList = forList;

            result.Initialize(propertyInfo);

            result.EnsureControlsCreated();

            this.OnInstanceCreated(result);
        }
    }
}