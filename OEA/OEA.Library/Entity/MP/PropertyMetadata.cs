/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// OEA 中的属性元数据都从这个类继承下来。
    /// </summary>
    /// <typeparam name="TPropertyType"></typeparam>
    public class PropertyMetadata<TPropertyType> : ManagedPropertyMetadata<TPropertyType>, IPropertyMetadata
    {
        public PropertyMetadata()
        {
            this.DefaultValue = CreateDefaultValue();
        }

        private static TPropertyType CreateDefaultValue()
        {
            var propertyType = typeof(TPropertyType);
            object defaultValue = default(TPropertyType);

            //处理DateTime类型的默认值为当天。
            //注意：目前不支持只支持到天，不支持到时间。
            if (propertyType == typeof(DateTime) && (DateTime)defaultValue == default(DateTime))
            {
                defaultValue = DateTime.Today;
            }
            else if (propertyType == typeof(DateRange) && defaultValue == null)
            {
                defaultValue = new DateRange()
                {
                    BeginValue = DateTime.Now,
                    EndValue = DateTime.Now
                };
            }
            else if (propertyType == typeof(string) && defaultValue == null)
            {
                defaultValue = string.Empty;
            }
            else if (propertyType == typeof(NumberRange) && defaultValue == null)
            {
                defaultValue = new NumberRange();
            }

            return (TPropertyType)defaultValue;
        }
    }
}