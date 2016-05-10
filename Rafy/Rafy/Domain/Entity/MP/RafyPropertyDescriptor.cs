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
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 属性描述器
    /// 
    /// Rafy 引用属性和子属性的值的获取与设置，不是直接返回 GetProperty 的内部原生属性值，
    /// 而是通过 GetLazyRef().NullableId、GetLazyList 返回值。
    /// 
    /// 同时，由于只能是界面使用 RafyPropertyDescriptor 这个类型来操作属性值，
    /// 所以还需要把枚举值转换为字符串。
    /// </summary>
    internal class RafyPropertyDescriptor : ManagedPropertyDescriptor
    {
        internal RafyPropertyDescriptor(IManagedProperty property) : base(property) { }

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
                if (mp is IRefIdProperty)
                {
                    return entity.GetRefNullableId(mp as IRefIdProperty);
                }
                if (mp is IRefEntityProperty)
                {
                    return entity.GetRefEntity(mp as IRefEntityProperty);
                }
            }

            if (mp is IListProperty)
            {
                return entity.GetLazyList(mp as IListProperty);
            }

            var value = entity.GetProperty(mp);

            //枚举值在为界面返回值时，应该返回 Label
            if (value != null && TypeHelper.IsEnumNullable(mp.PropertyType))
            {
                value = EnumViewModel.EnumToLabel((Enum)value).Translate();
            }

            return value;
        }

        public override void SetValue(object component, object value)
        {
            var mp = this.Property;
            var entity = component as Entity;

            if (mp is IRefIdProperty)
            {
                entity.SetRefNullableId(mp as IRefIdProperty, value, ManagedPropertyChangedSource.FromUIOperating);
                return;
            }

            //枚举值在为界面返回值时，应该返回 Label
            if (value != null)
            {
                var enumType = TypeHelper.IgnoreNullable(mp.PropertyType);
                if (enumType.IsEnum)
                {
                    value = EnumViewModel.Parse((value as string).TranslateReverse(), enumType);
                }
            }

            //使用 PropertyDescriptor 来进行操作属性时，都是作为 UI 层操作。
            entity.SetProperty(mp, value, ManagedPropertyChangedSource.FromUIOperating);
        }
    }
}
