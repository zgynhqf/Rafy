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

        IQuery Order(string column, bool ascending);
        IQuery Order(IManagedProperty property, bool ascending);

        //add by zhoujg 构造语句时选取指定列.考虑附件类似字段不需要获取,加入columns
        System.Collections.Generic.IList<string> Columns { get; set; }

        IConstraint Get(int index);
    }
}
