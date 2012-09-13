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
using OEA.ManagedProperty;

namespace OEA.Library
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
        /// 一个集合列表，由 N 个引用属性和 1 个一般值类型属性组成。
        /// 表示从第一个引用属性开始的一个引用链条，一直到最后一个值属性对应的值。例如：
        /// D.CRef, C.BRef, B.ARef, A.Name 这样的一个集合表示以下冗余路径：D->C->B->A.Name
        /// </param>
        public RedundantPath(params IProperty[] pathes)
        {
            this.ValueProperty = pathes[pathes.Length - 1];
            this.RefPathes = new ReadOnlyCollection<IRefProperty>(
                pathes.Take(pathes.Length - 1).Select(p => p as IRefProperty).ToArray()
                );
        }

        /// <summary>
        /// 冗余属性
        /// </summary>
        public IProperty Redundancy { get; internal set; }

        /// <summary>
        /// 所有引用属性路径
        /// </summary>
        public ReadOnlyCollection<IRefProperty> RefPathes { get; private set; }

        /// <summary>
        /// 最终的值属性
        /// </summary>
        public IProperty ValueProperty { get; private set; }

        private string DebuggerDisplay
        {
            get
            {
                var res = "RedundantPath : " + this.Redundancy.OwnerType.Name + "." + this.Redundancy.Name
                    + "，冗余路径：" + this.Redundancy.OwnerType.Name;
                foreach (var refProperty in this.RefPathes)
                {
                    res += "." + refProperty.RefEntityType.Name;
                }
                res += "." + ValueProperty.Name;
                return res;
            }
        }
    }
}
