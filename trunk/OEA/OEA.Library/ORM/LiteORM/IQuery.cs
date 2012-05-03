using System.Collections;
using OEA.ManagedProperty;
using System;

namespace OEA.ORM
{
    public interface IQuery
    {
        Type EntityType { get; }

        bool HasOrdered { get; }

        IQuery And();
        IQuery Or();

        IQuery Constrain(string propertyName);
        IQuery Constrain(IManagedProperty property);
        IQuery Constrain(IQuery query);

        IQuery Equal(object val);
        IQuery NotEqual(object val);
        IQuery Greater(object val);
        IQuery GreaterEqual(object val);
        IQuery Less(object val);
        IQuery LessEqual(object val);
        IQuery Like(string val);

        IQuery In(IList values);
        IQuery NotIn(IList values);

        IQuery Order(string property, bool ascending);
        IQuery Order(IManagedProperty property, bool ascending);

        IConstraint Get(int index);
    }
}
