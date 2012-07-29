using System;

namespace OEA.ORM
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class IDAttribute : Attribute { }
}