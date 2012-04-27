using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using JXC.Commands;

namespace JXC
{
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public class StorageMove : JXCEntity
    {
        public static readonly RefProperty<Storage> StorageFromRefProperty =
            P<StorageMove>.RegisterRef(e => e.StorageFrom, ReferenceType.Normal);
        public int StorageFromId
        {
            get { return this.GetRefId(StorageFromRefProperty); }
            set { this.SetRefId(StorageFromRefProperty, value); }
        }
        public Storage StorageFrom
        {
            get { return this.GetRefEntity(StorageFromRefProperty); }
            set { this.SetRefEntity(StorageFromRefProperty, value); }
        }

        public static readonly RefProperty<Storage> StorageToRefProperty =
            P<StorageMove>.RegisterRef(e => e.StorageTo, ReferenceType.Normal);
        public int StorageToId
        {
            get { return this.GetRefId(StorageToRefProperty); }
            set { this.SetRefId(StorageToRefProperty, value); }
        }
        public Storage StorageTo
        {
            get { return this.GetRefEntity(StorageToRefProperty); }
            set { this.SetRefEntity(StorageToRefProperty, value); }
        }

        public static readonly ListProperty<StorageMoveItemList> StorageMoveItemListProperty = P<StorageMove>.RegisterList(e => e.StorageMoveItemList);
        public StorageMoveItemList StorageMoveItemList
        {
            get { return this.GetLazyList(StorageMoveItemListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<StorageMove>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> UserProperty = P<StorageMove>.Register(e => e.User);
        public string User
        {
            get { return this.GetProperty(UserProperty); }
            set { this.SetProperty(UserProperty, value); }
        }

        public static readonly Property<DateTime> DateProperty = P<StorageMove>.Register(e => e.Date);
        public DateTime Date
        {
            get { return this.GetProperty(DateProperty); }
            set { this.SetProperty(DateProperty, value); }
        }

        public static readonly Property<string> CommentProperty = P<StorageMove>.Register(e => e.Comment);
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        protected override void AddValidations()
        {
            base.AddValidations();

            var rules = this.ValidationRules;
            rules.AddRule(CodeProperty, CommonRules.Required);
            rules.AddRule(UserProperty, CommonRules.Required);
            rules.AddRule(StorageToRefProperty, (e, args) =>
            {
                var move = e as StorageMove;
                if (move.StorageToId == move.StorageFromId)
                {
                    args.BrokenDescription = "出货仓库和入货仓库不能是同一个仓库";
                }
            });
            rules.AddRule((e, args) =>
            {
                var move = e as StorageMove;
                var children = move.StorageMoveItemList;
                if (children.Count == 0)
                {
                    args.BrokenDescription = "没有需要调拔的商品项。";
                    return;
                }

                foreach (StorageMoveItem item in move.StorageMoveItemList)
                {
                    if (item.Amount <= 0)
                    {
                        args.BrokenDescription = "商品项数量必须是正数。";
                        return;
                    }
                }
            });
        }
    }

    [Serializable]
    public class StorageMoveList : JXCEntityList
    {
        protected void QueryBy(TimeSpanCriteria criteria)
        {
            this.QueryDb(q =>
            {
                q.Constrain(StorageMove.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(StorageMove.DateProperty).LessEqual(criteria.To);
            });
        }
    }

    public class StorageMoveRepository : EntityRepository
    {
        protected StorageMoveRepository() { }
    }

    internal class StorageMoveConfig : EntityConfig<StorageMove>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();
        }

        protected override void ConfigView()
        {
            View.DomainName("库存调拔").HasDelegate(StorageMove.CodeProperty);

            View.ColumnsCountShowInDetail = 2;

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(AddStorageMoveBill),
                typeof(ShowBill),
                WPFCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(StorageMove.CodeProperty).HasLabel("调拔单编号").ShowIn(ShowInWhere.All);
                View.Property(StorageMove.UserProperty).HasLabel("发货人").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.DateProperty).HasLabel("发货日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.StorageFromRefProperty).HasLabel("出货仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.StorageToRefProperty).HasLabel("收货仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageMove.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}