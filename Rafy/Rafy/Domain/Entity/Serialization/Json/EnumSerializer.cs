/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20231104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.Net Standard 2.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20231104 15:47
 * 
*******************************************************/

using Rafy.Domain.Serialization.Json;
using Rafy.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.Serialization.Json
{
    public class EnumSerializer
    {
        /// <summary>
        /// 如果传入的值是一个枚举值，则将其按指定的模式转换。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static object ConvertEnumValue(object value, EnumSerializationMode mode)
        {
            if (value != null && value.GetType().IsEnum)
            {
                switch (mode)
                {
                    case EnumSerializationMode.String:
                        value = value.ToString();
                        break;
                    case EnumSerializationMode.EnumLabel:
                        value = EnumViewModel.EnumToLabel((Enum)value) ?? value.ToString();
                        break;
                    default:
                        break;
                }
            }

            return value;
        }
    }
}
