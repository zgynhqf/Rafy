/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:38
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:38
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 属性基类。
    /// </summary>
    public abstract class Property : EOMObject, IProperty
    {
        /// <summary>
        /// 该属性所属的实体类型
        /// </summary>
        public EntityType Parent { get; internal set; }

        //public abstract string Name { get; }

        //public abstract string Type { get; }

        internal abstract string GetName();

        internal abstract string GetPropertyType();

        #region IProperty 成员

        string IProperty.Name
        {
            get { return this.GetName(); }
        }

        string IProperty.PropertyType
        {
            get { return this.GetPropertyType(); }
        }

        #endregion

        public override string ToString()
        {
            return this.GetPropertyType() + " " + this.GetName();
        }
    }
}