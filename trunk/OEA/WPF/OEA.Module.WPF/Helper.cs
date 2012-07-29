/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120602 20:35
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120602 20:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Reflection;

namespace OEA.Module.WPF
{
    internal static class Helper
    {
        /// <summary>
        /// 获取属性的编辑器类型
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static string GetEditorNameOrDefault(this EntityPropertyViewMeta meta)
        {
            var value = meta.EditorName;

            //如果没有显式设置，则根据属性的类型来获取默认编辑器的名称。
            if (string.IsNullOrEmpty(value))
            {
                var epm = meta.PropertyMeta;
                if (epm.ReferenceInfo != null)
                {
                    return WPFEditorNames.LookupDropDown;
                }

                var propertyType = TypeHelper.IgnoreNullable(epm.Runtime.PropertyType);
                if (propertyType.IsEnum) { return WPFEditorNames.Enum; }

                var typeFullName = propertyType.FullName;
                switch (typeFullName)
                {
                    case "System.Int32":
                        value = WPFEditorNames.Int32;
                        break;
                    case "System.Double":
                        value = WPFEditorNames.Double;
                        break;
                    case "System.String":
                        value = WPFEditorNames.String;
                        break;
                    case "System.Boolean":
                        value = WPFEditorNames.Boolean;
                        break;
                    case "System.DateTime":
                        var propertyMeta = meta.PropertyMeta;
                        var mpMeta = propertyMeta.ManagedProperty.GetMeta(propertyMeta.Owner.EntityType) as IPropertyMetadata;
                        switch (mpMeta.DateTimePart)
                        {
                            case DateTimePart.DateTime:
                                value = WPFEditorNames.DateTime;
                                break;
                            case DateTimePart.Date:
                                value = WPFEditorNames.Date;
                                break;
                            case DateTimePart.Time:
                                value = WPFEditorNames.Time;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "OEA.NumberRange":
                        value = WPFEditorNames.NumberRange;
                        break;
                    case "OEA.DateRange":
                        value = WPFEditorNames.DateRange;
                        break;
                    default:
                        value = typeFullName;
                        break;
                }
            }

            return value;
        }
    }
}
