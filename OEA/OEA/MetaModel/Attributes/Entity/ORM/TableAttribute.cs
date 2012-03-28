using System;

namespace OEA.MetaModel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        public TableAttribute()
        {
            this.SupprtMigrating = true;
        }

        public TableAttribute(string name)
        {
            this.Name = name;
            this.SupprtMigrating = true;
        }

        public string Name { get; set; }

        public bool SupprtMigrating { get; set; }
    }
}