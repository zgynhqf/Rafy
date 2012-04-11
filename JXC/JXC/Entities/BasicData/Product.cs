//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OEA.Library;
//using OEA.MetaModel;
//using OEA.MetaModel.Attributes;
//using OEA.MetaModel.View;

//namespace JXC
//{
//    [RootEntity, Serializable]
//    public class Product : JXCEntity
//    {
//        public static readonly Property<string> BianMaProperty = P<Product>.Register(e => e.BianMa);
//        public string BianMa
//        {
//            get { return this.GetProperty(BianMaProperty); }
//            set { this.SetProperty(BianMaProperty, value); }
//        }

//        public static readonly Property<string> MingChengProperty = P<Product>.Register(e => e.MingCheng);
//        public string MingCheng
//        {
//            get { return this.GetProperty(MingChengProperty); }
//            set { this.SetProperty(MingChengProperty, value); }
//        }
//    }

//    [Serializable]
//    public class ProductList : JXCEntityList { }

//    public class ProductRepository : EntityRepository
//    {
//        protected ProductRepository() { }
//    }

//    internal class ProductConfig : EntityConfig<Product>
//    {
//        protected override void ConfigMeta()
//        {
//            base.ConfigMeta();

//            Meta.MapTable().HasColumns(
//                Product.NameProperty
//                );
//        }

//        protected override void ConfigView()
//        {
//            base.ConfigView();

//            View.HasLabel("商品").HasTitle(Product.NameProperty);

//            View.Property(Product.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
//        }
//    }
//}