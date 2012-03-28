using System;
using System.Reflection;

namespace OEA.ORM.sqlserver
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
