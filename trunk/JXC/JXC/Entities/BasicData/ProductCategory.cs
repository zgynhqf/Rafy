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
        #region 支持树型实体

        public static readonly Property<string> TreeCodeProperty = P<ProductCategory>.Register(e => e.TreeCode);
        [Column]
        public override string TreeCode
        {
            get { return GetProperty(TreeCodeProperty); }
            set { SetProperty(TreeCodeProperty, value); }
        }

        public static readonly Property<int?> TreePIdProperty = P<ProductCategory>.Register(e => e.TreePId);
        [Column]
        public override int? TreePId
        {
            get { return this.GetProperty(TreePIdProperty); }
            set { this.SetProperty(TreePIdProperty, value); }
        }

        public override bool SupportTree { get { return true; } }

        #endregion

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
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                ProductCategory.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("商品类别").HasTitle(ProductCategory.NameProperty);

            View.Property(ProductCategory.TreeCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All).Readonly(true);
            View.Property(ProductCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}