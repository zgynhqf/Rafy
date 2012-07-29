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
        private DateTimePart _dateTimePart;

        public PropertyMetadata()
        {
            this.DefaultValue = CreateDefaultValue();
        }

        /// <summary>
        /// 如果当前是一个时间类型，则这个属性表示该时间类型正在被使用的部分。
        /// </summary>
        public DateTimePart DateTimePart
        {
            get { return this._dateTimePart; }
            set
            {
                this._dateTimePart = value;

                //如果不是可空类型的 DateTime，则需要重新计算它的默认值。
                if (this.DefaultValue != null)
                {
                    switch (value)
                    {
                        case DateTimePart.Date:
                            this.DefaultValue = (TPropertyType)(object)DateTime.Today;
                            break;
                        case DateTimePart.DateTime:
                        case DateTimePart.Time:
                            this.DefaultValue = (TPropertyType)(object)DateTime.Now;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private static TPropertyType CreateDefaultValue()
        {
            var propertyType = typeof(TPropertyType);
            object defaultValue = null;

            //处理DateTime类型的默认值为当天。
            //注意：目前不支持只支持到天，不支持到时间。
            if (propertyType == typeof(DateTime))
            {
                defaultValue = DateTime.Today;
            }
            else if (propertyType == typeof(DateRange))
            {
                defaultValue = new DateRange()
                {
                    BeginValue = DateTime.Now,
                    EndValue = DateTime.Now
                };
            }
            else if (propertyType == typeof(string))
            {
                defaultValue = string.Empty;
            }
            else if (propertyType == typeof(NumberRange))
            {
                defaultValue = new NumberRange();
            }
            else if (propertyType == typeof(byte[]))
            {
                defaultValue = new byte[0];
            }

            defaultValue = defaultValue ?? default(TPropertyType);

            return (TPropertyType)defaultValue;
        }
    }
}