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
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Library;

namespace JXC
{
    [Serializable, NavigationQueryEntity]
    public class ProductNavigationCriteria : Criteria
    {
        public static readonly RefProperty<ProductCategory> ProductCategoryRefProperty =
            P<ProductNavigationCriteria>.RegisterRef(e => e.ProductCategory, ReferenceType.Normal);
        public int? ProductCategoryId
        {
            get { return this.GetRefNullableId(ProductCategoryRefProperty); }
            set { this.SetRefNullableId(ProductCategoryRefProperty, value); }
        }
        public ProductCategory ProductCategory
        {
            get { return this.GetRefEntity(ProductCategoryRefProperty); }
            set { this.SetRefEntity(ProductCategoryRefProperty, value); }
        }
    }

    internal class ProductNavigationCriteriaConfig : EntityConfig<ProductNavigationCriteria>
    {
        protected override void ConfigView()
        {
            View.DetailLabelWidth = 0;
            View.Property(ProductNavigationCriteria.ProductCategoryRefProperty)
                .HasLabel("商品类别").ShowIn(ShowInWhere.Detail)
                .UseEditor(WPFEditorNames.TiledList)
                .NavigationMeta = new NavigationPropertyMeta();
        }
    }
}