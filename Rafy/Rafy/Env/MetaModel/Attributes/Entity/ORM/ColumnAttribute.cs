using System;

namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 标记此标签的属性，就算默认已经标记上 EntityPropertyAttribute。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        public ColumnAttribute() { }

        public ColumnAttribute(string name)
        {
            this.ColumnName = name;
        }

        public string ColumnName { get; set; }
    }
}
