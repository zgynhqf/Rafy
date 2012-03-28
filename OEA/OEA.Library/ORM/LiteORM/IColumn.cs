using System;

namespace OEA.ORM
{
    public interface IColumn
    {
        ITable Table { get; }
        string Name { get; }
        Type DataType { get; }

        //OEA
        object GetValue(object obj);
        void SetValue(object obj, object val);
    }
}
