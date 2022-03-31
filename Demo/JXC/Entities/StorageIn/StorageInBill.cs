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
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    [RootEntity]
    public abstract class StorageInBill : JXCEntity
    {
        public static readonly IRefIdProperty StorageIdProperty =
            P<StorageInBill>.RegisterRefId(e => e.StorageId, ReferenceType.Normal);
        public int StorageId
        {
            get { return (int)this.GetRefId(StorageIdProperty); }
            set { this.SetRefId(StorageIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageProperty =
            P<StorageInBill>.RegisterRef(e => e.Storage, StorageIdProperty);
        public Storage Storage
        {
            get { return this.GetRefEntity(StorageProperty); }
            set { this.SetRefEntity(StorageProperty, value); }
        }

        public static readonly ListProperty<StorageInBillItemList> StorageInItemListProperty = P<StorageInBill>.RegisterList(e => e.StorageInItemList);
        public StorageInBillItemList StorageInItemList
        {
            get { return this.GetLazyList(StorageInItemListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<StorageInBill>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<double> TotalMoneyProperty = P<StorageInBill>.Register(e => e.TotalMoney);
        public double TotalMoney
        {
            get { return this.GetProperty(TotalMoneyProperty); }
            set { this.SetProperty(TotalMoneyProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<StorageInBill>.Register(e => e.Date);
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<string> CommentProperty = P<StorageInBill>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        protected override void OnRoutedEvent(object sender, EntityRoutedEventArgs e)
        {
            if (e.Event == StorageInBillItem.PriceChangedEvent || e.Event == StorageInBillItemList.ListChangedEvent)
            {
                this.TotalMoney = this.StorageInItemList.Sum(poi => (poi as StorageInBillItem).View_TotalPrice);
            }
        }

        public static readonly Property<string> StorageNameROProperty = P<StorageInBill>.RegisterReadOnly(
            e => e.StorageNameRO, e => (e as StorageInBill).GetStorageNameRO(), StorageProperty);
        /// <summary>
        /// 仓库名称（报表使用）
        /// </summary>
        public string StorageNameRO
        {
            get { return this.GetProperty(StorageNameROProperty); }
        }
        private string GetStorageNameRO()
        {
            return this.Storage.Name;
        }
    }

    public abstract class StorageInBillList : JXCEntityList { }

    public abstract class StorageInBillRepository : JXCEntityRepository
    {
        protected StorageInBillRepository() { }

        [RepositoryQuery]
        public virtual StorageInBillList GetBy(TimeSpanCriteria criteria)
        {
            throw new NotSupportedException();
            //return (this.DataProvider as StorageInBillDataProvider).GetBy(criteria);
        }
    }

    [DataProviderFor(typeof(StorageInBillRepository))]
    public class StorageInBillDataProvider : JXCEntityDataProvider
    {
        public StorageInBillDataProvider()
        {
            this.DataSaver = new StorageInBillSaver();
        }

        public StorageInBillList GetBy(TimeSpanCriteria criteria)
        {
            var f = QueryFactory.Instance;
            var t = f.Table(this.Repository);
            var q = f.Query(
                selection: t.Star(),
                from: t,
                where: f.And(
                    t.Column(StorageInBill.DateProperty).GreaterEqual(criteria.From),
                    t.Column(StorageInBill.DateProperty).LessEqual(criteria.To.AddDays(1d))
                )
            );
            return (StorageInBillList)this.QueryData(q);
        }

        private class StorageInBillSaver : RdbDataSaver
        {
            protected override void Submit(SubmitArgs e)
            {
                base.Submit(e);

                if (e.Action == SubmitAction.Delete)
                {
                    //由于本外键关系没有级联，所以在删除的时候需要删除下面的数据
                    this.DeleteRef(e.Entity, StorageInBillItem.StorageInBillProperty);
                }
            }
        }
    }

    internal class StorageInBillConfig : EntityConfig<StorageInBill>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(StorageInBill.CodeProperty, new RequiredRule());
            rules.AddRule(new HandlerRule
            {
                Handler = (e, args) =>
                {
                    var po = e as StorageInBill;
                    if (po.StorageInItemList.Count == 0)
                    {
                        args.BrokenDescription = "至少需要一个商品项。".Translate();
                    }
                    else
                    {
                        foreach (StorageInBillItem item in po.StorageInItemList)
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
        }

        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}