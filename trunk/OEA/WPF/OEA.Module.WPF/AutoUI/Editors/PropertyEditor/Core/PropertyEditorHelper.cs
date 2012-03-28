/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110616
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110616
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows;
using System.Windows.Data;

namespace OEA.Module.WPF.Editors
{
    internal static class PropertyEditorHelper
    {
        /// <summary>
        /// 检测某个实体对象的某个实体属性是否可以只读。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public static bool CheckIsReadonly(Entity entity, EntityPropertyViewMeta property)
        {
            //类指明为只读
            if (property.IsReadonly || property.Owner.NotAllowEdit) { return true; }

            ////如果没有标记为检查某一个属性，则表示必须是只读对象。
            //var readonlyAttri = property.ReadonlyIndicator;
            //if (readonlyAttri.Status == PropertyReadonlyStatus.Readonly) { return true; }

            ////检测动态属性
            //if (readonlyAttri.Status == PropertyReadonlyStatus.Dynamic && entity != null)
            //{
            //    return entity.GetPropertyValue<bool>(readonlyAttri.PropertyName);
            //}

            return false;
        }

        public static void BindElementReadOnly(ReadonlyElement matrix, EntityPropertyViewMeta property)
        {
            //类指明为只读
            if (property.IsReadonly || property.Owner.NotAllowEdit)
            {
                matrix.SetReadonly();
                return;
            }

            ////如果没有标记为检查某一个属性，则表示必须是只读对象。
            //var readonlyAttri = property.ReadonlyIndicator;
            //if (readonlyAttri.Status == PropertyReadonlyStatus.Readonly)
            //{
            //    matrix.SetReadonly();
            //    return;
            //}

            ////绑定动态属性
            //if (readonlyAttri.Status == PropertyReadonlyStatus.Dynamic)
            //{
            //    var rb = new Binding(readonlyAttri.PropertyName);
            //    rb.Mode = BindingMode.OneWay;

            //    if (!matrix.ReadonlyValue) rb.Converter = new ReverseBooleanConverter();

            //    matrix.Element.SetBinding(matrix.Property, rb);
            //}
        }
    }

    internal class ReadonlyElement
    {
        public FrameworkElement Element;
        public DependencyProperty Property;
        public bool ReadonlyValue;

        public void SetReadonly()
        {
            this.Element.SetValue(this.Property, this.ReadonlyValue);
        }
    }
}
