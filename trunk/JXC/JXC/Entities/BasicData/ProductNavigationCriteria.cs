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
    [QueryEntity, Serializable]
    public class ProductNavigationCriteria : Criteria
    {
        public static readonly RefProperty<ProductCategory> ProductCategoryRefProperty =
            P<ProductNavigationCriteria>.RegisterRef(e => e.ProductCategory, ReferenceType.Normal);
        public int ProductCategoryId
        {
            get { return this.GetRefId(ProductCategoryRefProperty); }
            set { this.SetRefId(ProductCategoryRefProperty, value); }
        }
        public ProductCategory ProductCategory
        {
            get { return this.GetRefEntity(ProductCategoryRefProperty); }
            set { this.SetRefEntity(ProductCategoryRefProperty, value); }
        }

        public static readonly Property<bool> IncludeSubProperty = P<ProductNavigationCriteria>.Register(e => e.IncludeSub, true);
        public bool IncludeSub
        {
            get { return this.GetProperty(IncludeSubProperty); }
            set { this.SetProperty(IncludeSubProperty, value); }
        }
    }

    internal class ProductNavigationCriteriaConfig : EntityConfig<ProductNavigationCriteria>
    {
        protected override void ConfigView()
        {
            View.UseWPFCommands(
                "JXC.Commands.RefreshProductNavigation",
                "JXC.Commands.OpenProductCategory"
                );

            View.Property(ProductNavigationCriteria.IncludeSubProperty)
                .HasLabel("包含下级").ShowInDetail()
                .FireNavigation();
            View.Property(ProductNavigationCriteria.ProductCategoryRefProperty)
                .HasLabel("商品类别").ShowInDetail(labelWidth: 0)
                .UseEditor(WPFEditorNames.TiledList)
                .FireNavigation();
        }
    }
}