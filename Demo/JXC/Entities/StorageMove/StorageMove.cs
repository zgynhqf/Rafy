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
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(TimeSpanCriteria))]
    public partial class StorageMove : JXCEntity
    {
        #region 构造函数

        public StorageMove() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected StorageMove(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty StorageFromIdProperty =
            P<StorageMove>.RegisterRefId(e => e.StorageFromId, ReferenceType.Normal);
        public int StorageFromId
        {
            get { return (int)this.GetRefId(StorageFromIdProperty); }
            set { this.SetRefId(StorageFromIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageFromProperty =
            P<StorageMove>.RegisterRef(e => e.StorageFrom, StorageFromIdProperty);
        public Storage StorageFrom
        {
            get { return this.GetRefEntity(StorageFromProperty); }
            set { this.SetRefEntity(StorageFromProperty, value); }
        }

        public static readonly IRefIdProperty StorageToIdProperty =
            P<StorageMove>.RegisterRefId(e => e.StorageToId, ReferenceType.Normal);
        public int StorageToId
        {
            get { return (int)this.GetRefId(StorageToIdProperty); }
            set { this.SetRefId(StorageToIdProperty, value); }
        }
        public static readonly RefEntityProperty<Storage> StorageToProperty =
            P<StorageMove>.RegisterRef(e => e.StorageTo, StorageToIdProperty);
        public Storage StorageTo
        {
            get { return this.GetRefEntity(StorageToProperty); }
            set { this.SetRefEntity(StorageToProperty, value); }
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
    }

    [Serializable]
    public partial class StorageMoveList : JXCEntityList
    {
    }

    public partial class StorageMoveRepository : JXCEntityRepository
    {
        protected StorageMoveRepository() { }

        protected EntityList FetchBy(TimeSpanCriteria criteria)
        {
            return this.QueryList(q =>
            {
                q.Constrain(StorageMove.DateProperty).GreaterEqual(criteria.From)
                    .And().Constrain(StorageMove.DateProperty).LessEqual(criteria.To);
            });
        }
    }

    internal class StorageMoveConfig : EntityConfig<StorageMove>
    {
        protected override void AddValidations(IValidationDeclarer rules)
        {
            rules.AddRule(StorageMove.CodeProperty, new RequiredRule());
            rules.AddRule(StorageMove.UserProperty, new RequiredRule());
            rules.AddRule(StorageMove.StorageToProperty, new HandlerRule
            {
                Handler = (e, args) =>
                {
                    var move = e as StorageMove;
                    if (move.StorageToId == move.StorageFromId)
                    {
                        args.BrokenDescription = "出货仓库和入货仓库不能是同一个仓库".Translate();
                    }
                }
            });
            rules.AddRule(new HandlerRule
            {
                Handler = (e, args) =>
                {
                    var move = e as StorageMove;
                    var children = move.StorageMoveItemList;
                    if (children.Count == 0)
                    {
                        args.BrokenDescription = "没有需要调拔的商品项。".Translate();
                        return;
                    }

                    foreach (StorageMoveItem item in move.StorageMoveItemList)
                    {
                        if (item.Amount <= 0)
                        {
                            args.BrokenDescription = "商品项数量必须是正数。".Translate();
                            return;
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