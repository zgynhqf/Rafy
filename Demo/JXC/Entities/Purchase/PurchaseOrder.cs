/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
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
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;


namespace JXC
{
    /// <summary>
    /// 采购订单
    /// </summary>
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(ClientTimeSpanCriteria))]
    public partial class PurchaseOrder : JXCEntity
    {
        #region 构造函数

        public PurchaseOrder() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected PurchaseOrder(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty SupplierIdProperty =
            P<PurchaseOrder>.RegisterRefId(e => e.SupplierId, ReferenceType.Normal);
        public int SupplierId
        {
            get { return (int)this.GetRefId(SupplierIdProperty); }
            set { this.SetRefId(SupplierIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientInfo> SupplierProperty =
            P<PurchaseOrder>.RegisterRef(e => e.Supplier, SupplierIdProperty);
        public ClientInfo Supplier
        {
            get { return this.GetRefEntity(SupplierProperty); }
            set { this.SetRefEntity(SupplierProperty, value); }
        }

        public static readonly IRefIdProperty StorageIdProperty =
            P<PurchaseOrder>.RegisterRefId(e => e.StorageId, ReferenceType.Normal);
        public int? StorageId
        {
            get { return (int?)this.GetRefNullableId(StorageIdProperty); }
            set { this.SetRefNullableId(StorageIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageProperty =
            P<PurchaseOrder>.RegisterRef(e => e.Storage, StorageIdProperty);
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageProperty); }
            set { this.SetRefEntity(StorageProperty, value); }
        }

        #endregion

        #region 子属性

        public static readonly ListProperty<PurchaseOrderItemList> PurchaseOrderItemListProperty = P<PurchaseOrder>.RegisterList(e => e.PurchaseOrderItemList);
        public PurchaseOrderItemList PurchaseOrderItemList
        {
            get { return this.GetLazyList(PurchaseOrderItemListProperty); }
        }

        public static readonly ListProperty<PurchaseOrderAttachementList> PurchaseOrderAttachementListProperty = P<PurchaseOrder>.RegisterList(e => e.PurchaseOrderAttachementList);
        public PurchaseOrderAttachementList PurchaseOrderAttachementList
        {
            get { return this.GetLazyList(PurchaseOrderAttachementListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<PurchaseOrder>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<PurchaseOrder>.Register(e => e.Date);
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<DateTime> PlanStorageInDateProperty = P<PurchaseOrder>.Register(e => e.PlanStorageInDate);
        public DateTime PlanStorageInDate
        {
            get { return this.GetProperty(PlanStorageInDateProperty); }
            set { this.SetProperty(PlanStorageInDateProperty, value); }
        }

        public static readonly Property<double> TotalMoneyProperty = P<PurchaseOrder>.Register(e => e.TotalMoney);
        public double TotalMoney
        {
            get { return this.GetProperty(TotalMoneyProperty); }
            set { this.SetProperty(TotalMoneyProperty, value); }
        }

        public static readonly Property<bool> StorageInDirectlyProperty = P<PurchaseOrder>.Register(e => e.StorageInDirectly, new PropertyMetadata<bool>
        {
            PropertyChangedCallBack = (o, e) => (o as PurchaseOrder).OnStorageInDirectlyChanged(e)
        });
        public bool StorageInDirectly
        {
            get { return this.GetProperty(StorageInDirectlyProperty); }
            set { this.SetProperty(StorageInDirectlyProperty, value); }
        }
        protected virtual void OnStorageInDirectlyChanged(ManagedPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue) { this.StorageInStatus = OrderStorageInStatus.Completed; }
        }

        public static readonly Property<string> CommentProperty = P<PurchaseOrder>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        public static readonly Property<OrderStorageInStatus> StorageInStatusProperty = P<PurchaseOrder>.Register(e => e.StorageInStatus, OrderStorageInStatus.Waiting);
        public OrderStorageInStatus StorageInStatus
        {
            get { return this.GetProperty(StorageInStatusProperty); }
            set { this.SetProperty(StorageInStatusProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> SupplierNameProperty = P<PurchaseOrder>.RegisterRedundancy(e => e.SupplierName,
            new RedundantPath(SupplierProperty, ClientInfo.NameProperty));
        public string SupplierName
        {
            get { return this.GetProperty(SupplierNameProperty); }
        }

        public static readonly Property<string> SupplierCategoryNameProperty = P<PurchaseOrder>.RegisterRedundancy(e => e.SupplierCategoryName,
            new RedundantPath(SupplierProperty, ClientInfo.ClientCategoryIdProperty, ClientCategory.NameProperty));
        public string SupplierCategoryName
        {
            get { return this.GetProperty(SupplierCategoryNameProperty); }
        }

        public static readonly Property<int> TotalAmountLeftProperty = P<PurchaseOrder>.RegisterReadOnly(e => e.TotalAmountLeft, e => (e as PurchaseOrder).GetTotalAmountLeft());
        public int TotalAmountLeft
        {
            get { return this.GetProperty(TotalAmountLeftProperty); }
        }
        private int GetTotalAmountLeft()
        {
            return this.PurchaseOrderItemList.Cast<PurchaseOrderItem>().Sum(e => e.AmountLeft);
        }

        public static readonly Property<ClientInfoList> SupplierDataSourceProperty = P<PurchaseOrder>.RegisterReadOnly(e => e.SupplierDataSource, e => (e as PurchaseOrder).GetSupplierDataSource());
        public ClientInfoList SupplierDataSource
        {
            get { return this.GetProperty(SupplierDataSourceProperty); }
        }
        private ClientInfoList GetSupplierDataSource()
        {
            return RF.Concrete<ClientInfoRepository>().GetSuppliers();
        }

        #endregion

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == PurchaseOrderItem.PriceChangedEvent || e.Event == PurchaseOrderItemList.ListChangedEvent)
            {
                this.TotalMoney = this.PurchaseOrderItemList.Sum(poi => (poi as PurchaseOrderItem).View_TotalPrice);
            }
        }
    }

    public enum OrderStorageInStatus
    {
        [Label("等待入库")]
        Waiting,
        [Label("完全入库")]
        Completed,
    }

    [Serializable]
    public partial class PurchaseOrderList : JXCEntityList { }

    public partial class PurchaseOrderRepository : JXCEntityRepository
    {
        protected PurchaseOrderRepository() { }

        [RepositoryQuery]
        public virtual PurchaseOrderList GetByStatus(OrderStorageInStatus status)
        {
            return (PurchaseOrderList)this.QueryList(q => q.Constrain(PurchaseOrder.StorageInStatusProperty).Equal(status));
        }

        [RepositoryQuery]
        public virtual PurchaseOrderList GetBy(ClientTimeSpanCriteria criteria)
        {
            return (PurchaseOrderList)this.QueryList(q =>
            {
                q.Constrain(PurchaseOrder.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(PurchaseOrder.DateProperty).LessEqual(criteria.To);

                if (criteria.ClientInfoId.HasValue)
                {
                    q.And().Constrain(PurchaseOrder.SupplierIdProperty).Equal(criteria.ClientInfoId.Value);
                }
            });
        }

        [RepositoryQuery]
        public virtual PurchaseOrderList GetBy(TimeSpanCriteria criteria)
        {
            return (PurchaseOrderList)this.QueryList(q =>
            {
                q.Constrain(PurchaseOrder.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(PurchaseOrder.DateProperty).LessEqual(criteria.To);
            });
        }
    }

    internal class PurchaseOrderConfig : EntityConfig<PurchaseOrder>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(PurchaseOrder.CodeProperty, new RequiredRule());
            rules.AddRule(new HandlerRule
            {
                Handler = (e, args) =>
                {
                    var po = e as PurchaseOrder;
                    if (po.PurchaseOrderItemList.Count == 0)
                    {
                        args.BrokenDescription = "订单至少需要一个订单项。".Translate();
                    }
                    else
                    {
                        foreach (PurchaseOrderItem item in po.PurchaseOrderItemList)
                        {
                            if (item.View_TotalPrice <= 0)
                            {
                                args.BrokenDescription = "商品项金额应该是正数。".Translate();
                                return;
                            }
                        }
                    }
                }
            });
            rules.AddRule(PurchaseOrder.StorageProperty, new HandlerRule
            {
                Handler = (e, args) =>
                {
                    var po = e as PurchaseOrder;
                    if (po.StorageInDirectly && !po.StorageId.HasValue)
                    {
                        args.BrokenDescription = "请选择需要入库的仓库。".Translate();
                    }
                }
            });
        }

        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}