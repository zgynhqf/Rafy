/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131214
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131214 21:50
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;

namespace Rafy.Domain
{
    /// <summary>
    /// 使用 IPropertyQuery 进行查询的参数。
    /// </summary>
    public class PropertyQueryArgs : EntityQueryArgsBase, IPropertySelectArgs
    {
        /// <summary>
        /// 对应的查询条件定义。
        /// </summary>
        public IPropertyQuery PropertyQuery { get; set; }

        //public override PagingInfo PagingInfo
        //{
        //    get
        //    {
        //        if (PropertyQuery == null) return PagingInfo.Empty;
        //        return PropertyQuery.PagingInfo;
        //    }
        //    set
        //    {
        //        EnsureQuery();
        //        PropertyQuery.Paging(value);
        //    }
        //}

        //public override void EagerLoad(IProperty property, Type propertyOwner)
        //{
        //    EnsureQuery();
        //    PropertyQuery.EagerLoad(property, propertyOwner);
        //}

        private void EnsureQuery()
        {
            if (PropertyQuery == null) throw new InvalidProgramException("请先设置 PropertyQuery 属性。");
        }
    }
}
