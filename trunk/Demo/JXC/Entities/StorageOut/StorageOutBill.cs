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
    public abstract class StorageOutBill : JXCEntity
    {
        public static readonly RefProperty<Storage> StorageRefProperty =
            P<StorageOutBill>.RegisterRef(e => e.Storage, ReferenceType.Normal);
        public int StorageId
        {
            get { return this.GetRefId(StorageRefProperty); }
            set { this.SetRefId(StorageRefProperty, value); }
        }
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageRefProperty); }
            set { this.SetRefEntity(StorageRefProperty, value); }
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

        protected override void OnDelete()
        {
            base.OnDelete();

            //由于本类没有映射数据表，所以在删除的时候需要删除下面的数据
            using (var db = this.CreateDb())
            {
                db.Delete(typeof(StorageOutBillItem), db.Query(typeof(StorageOutBillItem))
                    .Constrain(StorageOutBillItem.StorageOutBillRefProperty).Equal(this.Id)
                    );
            }
        }
    }

    [Serializable]
    public abstract class StorageOutBillList : JXCEntityList
    {
        protected void QueryBy(TimeSpanCriteria criteria)
        {
            this.QueryDb(q =>
            {
                q.Constrain(StorageOutBill.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(StorageOutBill.DateProperty).LessEqual(criteria.To);
            });
        }
    }

    public abstract class StorageOutBillRepository : EntityRepository
    {
        protected StorageOutBillRepository() { }
    }

    internal class StorageOutBillConfig : EntityConfig<StorageOutBill>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }
    }
}