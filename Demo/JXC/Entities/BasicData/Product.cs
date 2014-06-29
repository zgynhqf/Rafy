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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Caching;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.RBAC;
using Rafy.RBAC.Security;
using Rafy.Utils;

namespace JXC
{
    /// <summary>
    /// 产品
    /// </summary>
    [RootEntity, Serializable]
    [NavigationQueryType(typeof(ProductNavigationCriteria))]
    public partial class Product : JXCEntity
    {
        #region 构造函数

        public Product() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Product(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty ProductCategoryIdProperty =
            P<Product>.RegisterRefId(e => e.ProductCategoryId, ReferenceType.Normal);
        public int ProductCategoryId
        {
            get { return (int)this.GetRefId(ProductCategoryIdProperty); }
            set { this.SetRefId(ProductCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<ProductCategory> ProductCategoryProperty =
            P<Product>.RegisterRef(e => e.ProductCategory, ProductCategoryIdProperty);
        public ProductCategory ProductCategory
        {
            get { return this.GetRefEntity(ProductCategoryProperty); }
            set { this.SetRefEntity(ProductCategoryProperty, value); }
        }

        public static readonly IRefIdProperty SupplierIdProperty =
            P<Product>.RegisterRefId(e => e.SupplierId, ReferenceType.Normal);
        public int SupplierId
        {
            get { return (int)this.GetRefId(SupplierIdProperty); }
            set { this.SetRefId(SupplierIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientInfo> SupplierProperty =
            P<Product>.RegisterRef(e => e.Supplier, SupplierIdProperty);
        public ClientInfo Supplier
        {
            get { return this.GetRefEntity(SupplierProperty); }
            set { this.SetRefEntity(SupplierProperty, value); }
        }

        public static readonly IRefIdProperty OperatorIdProperty =
            P<Product>.RegisterRefId(e => e.OperatorId, ReferenceType.Normal);
        public int OperatorId
        {
            get { return (int)this.GetRefId(OperatorIdProperty); }
            set { this.SetRefId(OperatorIdProperty, value); }
        }
        public static readonly RefEntityProperty<User> OperatorProperty =
            P<Product>.RegisterRef(e => e.Operator, OperatorIdProperty);
        public User Operator
        {
            get { return this.GetRefEntity(OperatorProperty); }
            set { this.SetRefEntity(OperatorProperty, value); }
        }

        public static readonly ListProperty<ProductAttachementList> ProductAttachementListProperty = P<Product>.RegisterList(e => e.ProductAttachementList);
        public ProductAttachementList ProductAttachementList
        {
            get { return this.GetLazyList(ProductAttachementListProperty); }
        }

        public static readonly Property<string> BarcodeProperty = P<Product>.Register(e => e.Barcode);
        public string Barcode
        {
            get { return this.GetProperty(BarcodeProperty); }
            set { this.SetProperty(BarcodeProperty, value); }
        }

        public static readonly Property<byte[]> PictureProperty = P<Product>.Register(e => e.Picture);
        public byte[] Picture
        {
            get { return this.GetProperty(PictureProperty); }
            set { this.SetProperty(PictureProperty, value); }
        }

        public static readonly Property<string> BianMaProperty = P<Product>.Register(e => e.BianMa);
        public string BianMa
        {
            get { return this.GetProperty(BianMaProperty); }
            set { this.SetProperty(BianMaProperty, value); }
        }

        public static readonly Property<string> MingChengProperty = P<Product>.Register(e => e.MingCheng);
        public string MingCheng
        {
            get { return this.GetProperty(MingChengProperty); }
            set { this.SetProperty(MingChengProperty, value); }
        }

        public static readonly Property<string> GuiGeProperty = P<Product>.Register(e => e.GuiGe);
        public string GuiGe
        {
            get { return this.GetProperty(GuiGeProperty); }
            set { this.SetProperty(GuiGeProperty, value); }
        }

        public static readonly Property<string> PingPaiProperty = P<Product>.Register(e => e.PingPai);
        public string PingPai
        {
            get { return this.GetProperty(PingPaiProperty); }
            set { this.SetProperty(PingPaiProperty, value); }
        }

        public static readonly Property<double> CaiGouDanjiaProperty = P<Product>.Register(e => e.CaiGouDanjia);
        public double CaiGouDanjia
        {
            get { return this.GetProperty(CaiGouDanjiaProperty); }
            set { this.SetProperty(CaiGouDanjiaProperty, value); }
        }

        public static readonly Property<double> XiaoShouDanJiaProperty = P<Product>.Register(e => e.XiaoShouDanJia);
        public double XiaoShouDanJia
        {
            get { return this.GetProperty(XiaoShouDanJiaProperty); }
            set { this.SetProperty(XiaoShouDanJiaProperty, value); }
        }

        public static readonly Property<double> XiaoShouJia_1Property = P<Product>.Register(e => e.XiaoShouJia_1);
        public double XiaoShouJia_1
        {
            get { return this.GetProperty(XiaoShouJia_1Property); }
            set { this.SetProperty(XiaoShouJia_1Property, value); }
        }

        public static readonly Property<double> XiaoShouJia_2Property = P<Product>.Register(e => e.XiaoShouJia_2);
        public double XiaoShouJia_2
        {
            get { return this.GetProperty(XiaoShouJia_2Property); }
            set { this.SetProperty(XiaoShouJia_2Property, value); }
        }

        public static readonly Property<double> XiaoShouJia_3Property = P<Product>.Register(e => e.XiaoShouJia_3);
        public double XiaoShouJia_3
        {
            get { return this.GetProperty(XiaoShouJia_3Property); }
            set { this.SetProperty(XiaoShouJia_3Property, value); }
        }

        public static readonly Property<string> BeiZhuProperty = P<Product>.Register(e => e.BeiZhu);
        public string BeiZhu
        {
            get { return this.GetProperty(BeiZhuProperty); }
            set { this.SetProperty(BeiZhuProperty, value); }
        }

        public static readonly Property<DateTime> OperateTimeProperty = P<Product>.Register(e => e.OperateTime);
        public DateTime OperateTime
        {
            get { return this.GetProperty(OperateTimeProperty); }
            set { this.SetProperty(OperateTimeProperty, value); }
        }

        public static readonly Property<int> StorageAmountProperty = P<Product>.Register(e => e.StorageAmount);
        /// <summary>
        /// 商品的所有仓库的总数。
        /// 这个值其实是一个冗余的值。
        /// </summary>
        public int StorageAmount
        {
            get { return this.GetProperty(StorageAmountProperty); }
            set { this.SetProperty(StorageAmountProperty, value); }
        }

        #region 三个时间使用三种不同的属性编辑器

        public static readonly Property<DateTime> Time1Property = P<Product>.Register(e => e.Time1, new PropertyMetadata<DateTime>
        {
            DateTimePart = DateTimePart.Date
        });
        public DateTime Time1
        {
            get { return this.GetProperty(Time1Property); }
            set { this.SetProperty(Time1Property, value); }
        }

        public static readonly Property<DateTime> Time2Property = P<Product>.Register(e => e.Time2);
        public DateTime Time2
        {
            get { return this.GetProperty(Time2Property); }
            set { this.SetProperty(Time2Property, value); }
        }

        public static readonly Property<DateTime> Time3Property = P<Product>.Register(e => e.Time3, new PropertyMetadata<DateTime>
        {
            DateTimePart = DateTimePart.Time
        });
        public DateTime Time3
        {
            get { return this.GetProperty(Time3Property); }
            set { this.SetProperty(Time3Property, value); }
        }

        #endregion
    }

    [Serializable]
    public partial class ProductList : JXCEntityList
    {
    }

    public partial class ProductRepository : JXCEntityRepository
    {
        protected ProductRepository() { }

        public Product GetByBarcode(string barcode)
        {
            return this.FetchFirst(barcode);
        }

        /// <summary>
        /// 导航面板查询
        /// </summary>
        /// <param name="criteria"></param>
        protected EntityList FetchBy(ProductNavigationCriteria criteria)
        {
            return this.QueryList(q =>
            {
                //q.JoinRef(Product.ProductCategoryIdProperty));

                //if (criteria.IncludeSub)
                //{
                //    q.Constrain(ProductCategory.TreeIndexProperty).StartWith(criteria.ProductCategory.TreeCode);
                //}
                //else
                //{
                //    q.Constrain(ProductCategory.TreeIndexProperty).Equal(criteria.ProductCategory.TreeCode);
                //}

                q.Constrain(Product.ProductCategoryIdProperty).Equal(criteria.ProductCategoryId);

                if (criteria.IncludeSub)
                {
                    criteria.ProductCategory.TreeChildren.EachNode(child =>
                    {
                        q.Or().Constrain(Product.ProductCategoryIdProperty).Equal(child.Id);
                        return false;
                    });
                }
            });
        }

        protected EntityList FetchBy(string barcode)
        {
            return this.QueryList(q => q.Constrain(Product.BarcodeProperty).Equal(barcode));
        }
    }

    internal class ProductConfig : EntityConfig<Product>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.Property(Product.PictureProperty).MapColumn().IsNullable();
            Meta.Property(Product.SupplierIdProperty).MapColumn().HasColumnName("ClientInfoId");
            Meta.Property(Product.OperatorIdProperty).MapColumn().HasColumnName("UserId");

            //基础数据启用分布式缓存
            Meta.EnableClientCache();
        }
    }
}