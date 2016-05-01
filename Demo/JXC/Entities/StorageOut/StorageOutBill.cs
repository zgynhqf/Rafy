using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public abstract class StorageOutBill : JXCEntity
    {
        #region 构造函数

        public StorageOutBill() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected StorageOutBill(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty StorageIdProperty =
            P<StorageOutBill>.RegisterRefId(e => e.StorageId, ReferenceType.Normal);
        public int StorageId
        {
            get { return (int)this.GetRefId(StorageIdProperty); }
            set { this.SetRefId(StorageIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageProperty =
            P<StorageOutBill>.RegisterRef(e => e.Storage, StorageIdProperty);
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageProperty); }
            set { this.SetRefEntity(StorageProperty, value); }
        }

        public static readonly ListProperty<StorageOutBillItemList> StorageOutBillItemListProperty = P<StorageOutBill>.RegisterList(e => e.StorageOutBillItemList);
        public StorageOutBillItemList StorageOutBillItemList
        {
            get { return this.GetLazyList(StorageOutBillItemListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<StorageOutBill>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<StorageOutBill>.Register(e => e.Date);
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<string> CommentProperty = P<StorageOutBill>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        public static readonly Property<int> TotalAmountProperty = P<StorageOutBill>.Register(e => e.TotalAmount);
        public int TotalAmount
        {
            get { return this.GetProperty(TotalAmountProperty); }
            set { this.SetProperty(TotalAmountProperty, value); }
        }

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == StorageOutBillItem.PriceChangedEvent || e.Event == StorageOutBillItemList.ListChangedEvent)
            {
                this.TotalAmount = this.StorageOutBillItemList.Sum(poi => (poi as StorageOutBillItem).Amount);
            }
        }
    }

    [Serializable]
    public abstract class StorageOutBillList : JXCEntityList { }

    public abstract class StorageOutBillRepository : JXCEntityRepository
    {
        protected StorageOutBillRepository() { }

        [RepositoryQuery]
        public virtual StorageOutBillList GetBy(TimeSpanCriteria criteria)
        {
            throw new NotSupportedException();
            //return (this.DataProvider as StorageInBillDataProvider).GetBy(criteria);
        }
    }

    [DataProviderFor(typeof(StorageOutBillRepository))]
    public class StorageOutBillDataProvider : JXCEntityDataProvider
    {
        public StorageOutBillDataProvider()
        {
            this.DataSaver = new StorageOutBillSaver();
        }

        public StorageOutBillList GetBy(TimeSpanCriteria criteria)
        {
            var f = QueryFactory.Instance;
            var t = f.Table(this.Repository);
            var q = f.Query(
                selection: t.Star(),
                from: t,
                where: f.And(
                    t.Column(StorageOutBill.DateProperty).GreaterEqual(criteria.From),
                    t.Column(StorageOutBill.DateProperty).LessEqual(criteria.To)
                )
            );
            return (StorageOutBillList)this.QueryData(q);
        }

        private class StorageOutBillSaver : RdbDataSaver
        {
            protected override void Submit(SubmitArgs e)
            {
                base.Submit(e);

                if (e.Action == SubmitAction.Delete)
                {
                    //由于本外键关系没有级联，所以在删除的时候需要删除下面的数据
                    this.DeleteRef(e.Entity, StorageOutBillItem.StorageOutBillProperty);
                }
            }
        }
    }

    internal class StorageOutBillConfig : EntityConfig<StorageOutBill>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}