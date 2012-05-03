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

namespace OEA.Web.ClientMetaModel
{
    public static class ServerTypeHelper
    {
        public static ServerType GetServerType(Type fieldType)
        {
            var st = new ServerType { RuntimeType = fieldType };

            if (fieldType == typeof(string))
            {
                st.IsNullable = true;
                st.Name = SupportedServerType.String;
            }
            else if (fieldType == typeof(int)) { st.Name = SupportedServerType.Int32; }
            else if (fieldType.IsEnum) { st.Name = SupportedServerType.Enum; }
            else if (fieldType == typeof(double)) { st.Name = SupportedServerType.Double; }
            else if (fieldType == typeof(bool)) { st.Name = SupportedServerType.Boolean; }
            else if (fieldType == typeof(DateTime)) { st.Name = SupportedServerType.DateTime; }
            else if (fieldType == typeof(Guid)) { st.Name = SupportedServerType.Guid; }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var innerType = fieldType.GetGenericArguments()[0];
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

        public static string GetColumnXType(ServerType type)
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

        public static FieldConfig GetTypeEditor(ServerType type)
        {
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
                    return new FieldConfig
                    {
                        xtype = "combolist"
                    };
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

        public static string GetClientFieldTypeName(ServerType type)
        {
            if (type.JSType == JavascriptType.Reference)
            {
                return "string";
            }

            return type.JSType.ToString().ToLower();
        }
    }
}