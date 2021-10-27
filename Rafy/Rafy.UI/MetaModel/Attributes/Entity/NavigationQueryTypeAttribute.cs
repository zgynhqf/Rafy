using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 对该类进行导航查询的条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class NavigationQueryTypeAttribute : Attribute
    {
        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="queryType">导航查询的条件对象的类型</param>
        public NavigationQueryTypeAttribute(Type queryType)
        {
            QueryType = queryType;
        }

        /// <summary>
        /// 导航查询的条件对象的类型
        /// </summary>
        public Type QueryType { get; private set; }
    }
}
