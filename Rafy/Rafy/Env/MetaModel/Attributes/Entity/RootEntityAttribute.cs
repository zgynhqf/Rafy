using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 所有聚合根实体类型，都应该标记这个属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RootEntityAttribute : EntityAttribute { }
}