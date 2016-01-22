/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel.View;
using Rafy.Reflection;

namespace Rafy.Web.ClientMetaModel
{
    internal static class ServerTypeHelper
    {
        internal static ServerType GetServerType(Type propertyType)
        {
            var st = new ServerType { RuntimeType = propertyType };

            if (propertyType == typeof(string))
            {
                st.IsNullable = true;
                st.Name = SupportedServerType.String;
            }
            else if (propertyType == typeof(int)) { st.Name = SupportedServerType.Int32; }
            else if (propertyType == typeof(long)) { st.Name = SupportedServerType.Int64; }
            else if (propertyType.IsEnum) { st.Name = SupportedServerType.Enum; }
            else if (propertyType == typeof(double)) { st.Name = SupportedServerType.Double; }
            else if (propertyType == typeof(bool)) { st.Name = SupportedServerType.Boolean; }
            else if (propertyType == typeof(DateTime)) { st.Name = SupportedServerType.DateTime; }
            else if (propertyType == typeof(Guid)) { st.Name = SupportedServerType.Guid; }
            else if (TypeHelper.IsNullable(propertyType))
            {
                var innerType = TypeHelper.IgnoreNullable(propertyType);
                st = GetServerType(innerType);
                st.IsNullable = true;
                return st;
            }
            else
            {
                st.Name = SupportedServerType.Unknown;
                return st;
            }

            st.JSType = GetClientType(st);

            return st;
        }

        private static JavascriptType GetClientType(ServerType type)
        {
            switch (type.Name)
            {
                case SupportedServerType.String:
                case SupportedServerType.Enum:
                    return JavascriptType.String;
                case SupportedServerType.Int32:
                case SupportedServerType.Int64:
                    return JavascriptType.Int;
                case SupportedServerType.Double:
                    return JavascriptType.Float;
                case SupportedServerType.Boolean:
                    return JavascriptType.Boolean;
                case SupportedServerType.DateTime:
                    return JavascriptType.Date;
                case SupportedServerType.Guid:
                    return JavascriptType.Reference;
                default:
                    throw new NotSupportedException();
            }
        }

        internal static string GetColumnXType(ServerType type)
        {
            switch (type.JSType)
            {
                case JavascriptType.Int:
                case JavascriptType.Float:
                    return "numbercolumn";
                case JavascriptType.Date:
                    return "datecolumn";
                case JavascriptType.Boolean:
                    return "booleancolumn";
                case JavascriptType.String:
                case JavascriptType.Reference:
                default:
                    return null;
            }
        }

        internal static FieldConfig GetTypeEditor(EntityPropertyViewMeta property)
        {
            var type = GetServerType(property.PropertyMeta.PropertyType);

            switch (type.JSType)
            {
                case JavascriptType.Int:
                case JavascriptType.Float:
                    return new FieldConfig
                    {
                        xtype = "numberfield"
                    };
                case JavascriptType.Date:
                    return new FieldConfig
                    {
                        xtype = "datefield"
                    };
                case JavascriptType.Boolean:
                    return new FieldConfig
                    {
                        xtype = "checkbox"
                    };
                case JavascriptType.Reference:
                    throw new InvalidOperationException("请调用 CreateComboList 方法。");
                case JavascriptType.String:
                    if (type.Name == SupportedServerType.Enum)
                    {
                        return new EnumBoxConfig(type.RuntimeType);
                    }
                    else
                    {
                        return new TextFieldConfig
                        {
                            allowBlank = type.IsNullable
                        };
                    }
                default:
                    return null;
            }
        }

        internal static ComboListConfig CreateComboList(EntityPropertyViewMeta property)
        {
            var comboList = new ComboListConfig();

            comboList.model = ClientEntities.GetClientName(property.SelectionViewMeta.SelectionEntityType);

            var title = property.SelectionViewMeta.RefTypeDefaultView.TitleProperty;
            if (title != null) { comboList.displayField = title.Name; }

            var dsp = property.SelectionViewMeta.DataSourceProperty;
            if (dsp != null) { comboList.dataSourceProperty = dsp.Name; }

            return comboList;
        }

        internal static string GetClientFieldTypeName(ServerType type)
        {
            if (type.JSType == JavascriptType.Reference)
            {
                return "string";
            }

            return type.JSType.ToString().ToLower();
        }
    }
}