/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120412
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120412
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain;
using Rafy;
using Rafy.ManagedProperty;

namespace JXC
{
    [QueryEntity, Serializable]
    public class ProductNavigationCriteria : Criteria
    {
        public static readonly IRefIdProperty ProductCategoryIdProperty =
            P<ProductNavigationCriteria>.RegisterRefId(e => e.ProductCategoryId, ReferenceType.Normal);
        public int ProductCategoryId
        {
            get { return (int)this.GetRefId(ProductCategoryIdProperty); }
            set { this.SetRefId(ProductCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<ProductCategory> ProductCategoryProperty =
            P<ProductNavigationCriteria>.RegisterRef(e => e.ProductCategory, ProductCategoryIdProperty);
        public ProductCategory ProductCategory
        {
            get { return this.GetRefEntity(ProductCategoryProperty); }
            set { this.SetRefEntity(ProductCategoryProperty, value); }
        }

        public static readonly Property<bool> IncludeSubProperty = P<ProductNavigationCriteria>.Register(e => e.IncludeSub, true);
        public bool IncludeSub
        {
            get { return this.GetProperty(IncludeSubProperty); }
            set { this.SetProperty(IncludeSubProperty, value); }
        }
    }
}