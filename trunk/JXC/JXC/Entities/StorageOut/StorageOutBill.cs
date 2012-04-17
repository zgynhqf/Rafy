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
    }

    [Serializable]
    public abstract class StorageOutBillList : JXCEntityList { }

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