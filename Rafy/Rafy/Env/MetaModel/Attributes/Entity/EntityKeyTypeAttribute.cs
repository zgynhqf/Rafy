/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20210828
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20210828 07:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.MetaModel.Attributes
{
    public class EntityKeyTypeAttribute : Attribute
    {
        public string KeyTypeName { get; set; }

        public static Type GetKeyType(Type entityType)
        {
            var attri = Get(entityType);
            switch (attri.KeyTypeName)
            {
                case "System.Int32":
                    return typeof(int);
                case "System.Int64":
                    return typeof(long);
                case "System.String":
                    return typeof(string);
                case "System.Guid":
                    return typeof(Guid);
                case "System.Object":
                    return typeof(Object);
                default:
                    break;
            }
            return typeof(int);
        }

        private static EntityKeyTypeAttribute Get(Type entityType)
        {
            return entityType.GetCustomAttributes(typeof(EntityKeyTypeAttribute), true)[0] as EntityKeyTypeAttribute;
        }
    }
}