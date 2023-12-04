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
using System.Reflection;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Reflection;
using Rafy.Utils;

namespace Rafy.WPF
{
    public static class Helper
    {
        /// <summary>
        /// 获取属性的编辑器类型
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static string GetEditorNameOrDefault(this WPFEntityPropertyViewMeta meta)
        {
            var value = meta.EditorName;

            //如果没有显式设置，则根据属性的类型来获取默认编辑器的名称。
            if (string.IsNullOrEmpty(value))
            {
                var epm = meta.PropertyMeta;
                if (epm.ReferenceInfo != null)
                {
                    return WPFEditorNames.EntitySelection_DropDown;
                }

                var propertyType = TypeHelper.IgnoreNullable(epm.Runtime.PropertyType);
                if (propertyType.IsEnum) { return WPFEditorNames.Enum; }

                if (propertyType == typeof(int))
                {
                    value = WPFEditorNames.Int32;
                }
                else if (propertyType == typeof(double))
                {
                    value = WPFEditorNames.Double;
                }
                else if (propertyType == typeof(string))
                {
                    value = WPFEditorNames.String;
                }
                else if (propertyType == typeof(bool))
                {
                    value = WPFEditorNames.Boolean;
                }
                else if (propertyType == typeof(DateTime))
                {
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
                }
                else if (propertyType == typeof(NumberRange))
                {
                    value = WPFEditorNames.NumberRange;
                }
                else if (propertyType == typeof(DateRange))
                {
                    value = WPFEditorNames.DateRange;
                }
                else
                {
                    value = propertyType.FullName;
                }
            }

            return value;
        }

        /// <summary>
        /// 根据SelectedValuePath指定的值，获取目标属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static object GetSelectedValue(this SelectionViewMeta rvi, Entity entity)
        {
            var selectedValuePath = rvi.SelectedValuePath ?? Entity.IdProperty;

            //如果是一个引用属性，则返回引用属性的 Id
            var refProperty = RefPropertyHelper.Find(selectedValuePath);
            if (refProperty != null)
            {
                return refProperty.Nullable ?
                    entity.GetRefNullableKey(refProperty) :
                    entity.GetRefKey(refProperty);
            }

            return entity.GetProperty(selectedValuePath);
        }

        /// <summary>
        /// 返回指定程序集中指定路径对应的 PackUri
        /// </summary>
        /// <param name="assemply"></param>
        /// <param name="path"></param>
        /// <param name="uriKind"></param>
        /// <returns></returns>
        public static Uri GetPackUri(Assembly assemply, string path, UriKind uriKind = UriKind.RelativeOrAbsolute)
        {
            if (!path.Contains("component"))
            {
                if (uriKind == UriKind.Absolute)
                {
                    path = string.Format("pack://application:,,,/{0};component/{1}", assemply.GetName().Name, path);
                }
                else
                {
                    path = string.Format("{0};component/{1}", assemply.GetName().Name, path);
                }
            }

            var uri = new Uri(path, uriKind);

            return uri;
        }
    }
}
