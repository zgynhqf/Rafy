/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120413
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public class ProductCategory : JXCEntity
    {
        public static readonly Property<string> NameProperty = P<ProductCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class ProductCategoryList : JXCEntityList { }

    public class ProductCategoryRepository : EntityRepository
    {
        protected ProductCategoryRepository() { }
    }

    internal class ProductCategoryConfig : EntityConfig<ProductCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().MapProperties(
                ProductCategory.NameProperty
                );

            Meta.EnableCache();
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("商品类别").HasDelegate(ProductCategory.NameProperty);

            View.Property(ProductCategory.TreeCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.ListDropDown)
                .HasOrderNo(-1).Readonly();
            View.Property(ProductCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}