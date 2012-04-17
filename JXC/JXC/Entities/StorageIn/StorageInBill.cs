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
    public abstract class StorageInBill : JXCEntity
    {
        public static readonly ListProperty<StorageInItemList> StorageInItemListProperty = P<StorageInBill>.RegisterList(e => e.StorageInItemList);
        public StorageInItemList StorageInItemList
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
    }

    [Serializable]
    public abstract class StorageInBillList : JXCEntityList { }

    public abstract class StorageInBillRepository : EntityRepository
    {
        protected StorageInBillRepository() { }
    }

    internal class StorageInBillConfig : EntityConfig<StorageInBill>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }
    }
}