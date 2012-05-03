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
using System.Reflection;

namespace OEA.ORM
{
    public class PropertyBridge : IDataBridge
    {
        private PropertyInfo field;

        public PropertyBridge(PropertyInfo field)
        {
            this.field = field;
        }

        public bool Readable
        {
            get { return field.CanRead; }
        }

        public bool Writeable
        {
            get { return field.CanWrite; }
        }

        public Type DataType
        {
            get { return field.PropertyType; }
        }

        public object Read(object obj)
        {
            return field.GetValue(obj, null);
        }

        public void Write(object obj, object val)
        {
            field.SetValue(obj, val, null);
        }
    }
}