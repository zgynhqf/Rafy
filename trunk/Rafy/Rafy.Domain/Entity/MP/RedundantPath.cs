/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120808 17:26
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120808 17:26
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 冗余属性路径
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class RedundantPath
    {
        /// <summary>
        /// 冗余属性路径
        /// </summary>
        /// <param name="pathes">
        /// 此数组内只接受两个类型：IProperty，ConcreteProperty。
        /// 
        /// 一个集合列表，由 N 个引用属性和 1 个一般值类型属性组成。
        /// 表示从第一个引用属性开始的一个引用链条，一直到最后一个值属性对应的值。例如：
        /// D.CRef, C.BRef, B.ARef, A.Name 这样的一个集合表示以下冗余路径：D->C->B->A.Name
        /// </param>
        public RedundantPath(params object[] pathes)
        {
            this.ValueProperty = ConvertParameter(pathes[pathes.Length - 1]);

            this.RefPathes = new ReadOnlyCollection<ConcreteProperty>(
                pathes.Take(pathes.Length - 1)
                .Select(p => ConvertParameter(p))
                .ToArray()
                );
        }

        /// <summary>
        /// 冗余属性
        /// </summary>
        public ConcreteProperty Redundancy { get; internal set; }

        /// <summary>
        /// 所有引用属性路径。
        /// 
        /// 注意，第一个引用属性，必须和冗余属性同在一个实体类型中。
        /// 
        /// 注意，此集合中直接存储的是引用 Id 属性。
        /// </summary>
        public ReadOnlyCollection<ConcreteProperty> RefPathes { get; private set; }

        /// <summary>
        /// 最终的值属性
        /// </summary>
        public ConcreteProperty ValueProperty { get; private set; }

        private string DebuggerDisplay
        {
            get
            {
                var res = "RedundantPath : " + this.Redundancy.FullName
                    + "，冗余路径：" + this.GetPathExpression();
                return res;
            }
        }

        public string GetPathExpression()
        {
            var res = this.Redundancy.Owner.Name;
            foreach (var refProperty in this.RefPathes)
            {
                res += "." + (refProperty.Property as IRefIdProperty).RefEntityType.Name;
            }
            res += "." + ValueProperty.Property.Name;
            return res;
        }

        private static ConcreteProperty ConvertParameter(object pathParameter)
        {
            var property = pathParameter as IProperty;
            if (property != null)
            {
                var refProperty = property as IRefProperty;
                if (refProperty != null) { return new ConcreteProperty(refProperty.RefIdProperty); }

                return new ConcreteProperty(property);
            }

            var concreteProperty = pathParameter as ConcreteProperty;
            if (concreteProperty != null) { return concreteProperty; }

            throw new InvalidOperationException(string.Format("RedundantPath 构造函数中的参数必须是 IProperty 类型或者 ConcreteProperty 类型，参数 {0} 不符合规定。", pathParameter));
        }
    }
}