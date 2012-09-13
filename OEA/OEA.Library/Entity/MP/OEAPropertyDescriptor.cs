/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120904 10:15
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120904 10:15
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.Reflection;
using OEA.Utils;

namespace OEA.Library
{
    /// <summary>
    /// OEA 属性描述器
    /// 
    /// OEA 引用属性和子属性的值的获取与设置，不是直接返回 GetProperty 的内部原生属性值，
    /// 而是通过 GetLazyRef().NullableId、GetLazyList 返回值。
    /// 
    /// 同时，由于只能是界面使用 OEAPropertyDescriptor 这个类型来操作属性值，
    /// 所以还需要把枚举值转换为字符串。
    /// </summary>
    internal class OEAPropertyDescriptor : ManagedPropertyDescriptor
    {
        internal OEAPropertyDescriptor(IManagedProperty property) : base(property) { }

        public override bool IsReadOnly
        {
            get
            {
                var mp = this.Property;

                //列表属性不可设置
                if (mp is IListProperty) return true;

                return mp.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                var value = base.PropertyType;

                //枚举类型应该向界面层输出字符串
                if (TypeHelper.IsEnumNullable(value)) return typeof(string);

                return value;
            }
        }

        public override object GetValue(object component)
        {
            var mp = this.Property;
            var entity = component as Entity;

            if (mp is IRefProperty)
            {
                return entity.GetLazyRef(mp as IRefProperty).NullableId;
            }

            if (mp is IListProperty)
            {
                return entity.GetLazyList(mp as IListProperty);
            }

            var value = entity.GetProperty(mp);

            //枚举值在为界面返回值时，应该返回 Label
            if (value != null && TypeHelper.IsEnumNullable(mp.PropertyType))
            {
                value = OEA.Utils.EnumViewModel.EnumToLabel((Enum)value);
            }

            return value;
        }

        public override void SetValue(object component, object value)
        {
            var mp = this.Property;
            var entity = component as Entity;

            if (mp is IRefProperty)
            {
                entity.GetLazyRef(mp as IRefProperty).NullableId = (int?)value;
                return;
            }

            //枚举值在为界面返回值时，应该返回 Label
            if (value != null)
            {
                var enumType = TypeHelper.IgnoreNullable(mp.PropertyType);
                if (enumType.IsEnum)
                {
                    value = EnumViewModel.LabelToEnum(value as string, enumType);
                }
            }

            //使用 PropertyDescriptor 来进行操作属性时，都是作为 UI 层操作。
            entity.SetProperty(mp, value, ManagedPropertyChangedSource.FromUIOperating);
        }
    }
}
