/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace OEA.MetaModel.Attributes
{
    /// <summary>
    /// 关联属性会为对象生成一个明细信息
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AssociationAttribute : Attribute
    {
        public AssociationAttribute() : this(0) { }

        public AssociationAttribute(double orderNo)
        {
            this.OrderNo = orderNo;
        }

        /// <summary>
        /// 用于排序属性
        /// </summary>
        public double OrderNo { get; set; }

        /// <summary>
        /// 是否递归生成它的子对象
        /// </summary>
        public bool SuppressRecurUI { get; set; }

        /// <summary>
        /// 是否生成工具条
        /// </summary>
        public bool SuppressToolbar { get; set; }
    }
}