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
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public class Storage : JXCEntity
    {
        public static readonly ListProperty<StorageProductList> StorageProductListProperty = P<Storage>.RegisterList(e => e.StorageProductList);
        public StorageProductList StorageProductList
        {
            get { return this.GetLazyList(StorageProductListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<Storage>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Storage>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> AddressProperty = P<Storage>.Register(e => e.Address);
        public string Address
        {
            get { return this.GetProperty(AddressProperty); }
            set { this.SetProperty(AddressProperty, value); }
        }

        public static readonly Property<string> ResponsiblePersonProperty = P<Storage>.Register(e => e.ResponsiblePerson);
        public string ResponsiblePerson
        {
            get { return this.GetProperty(ResponsiblePersonProperty); }
            set { this.SetProperty(ResponsiblePersonProperty, value); }
        }

        public static readonly Property<string> AreaProperty = P<Storage>.Register(e => e.Area);
        public string Area
        {
            get { return this.GetProperty(AreaProperty); }
            set { this.SetProperty(AreaProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Storage>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }

        public static readonly Property<int> TotalAmountProperty = P<Storage>.RegisterReadOnly(e => e.TotalAmount, e => (e as Storage).GetTotalAmount(), null);
        public int TotalAmount
        {
            get { return this.GetProperty(TotalAmountProperty); }
        }
        private int GetTotalAmount()
        {
            return this.StorageProductList.Cast<StorageProduct>().Sum(sp => sp.Amount);
        }
    }

    [Serializable]
    public class StorageList : JXCEntityList { }

    public class StorageRepository : EntityRepository
    {
        protected StorageRepository() { }
    }

    internal class StorageConfig : EntityConfig<Storage>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTable();

            Meta.EnableCache();
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("仓库").HasDelegate(Storage.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(Storage.CodeProperty).HasLabel("仓库编码").ShowIn(ShowInWhere.All);
                View.Property(Storage.NameProperty).HasLabel("仓库名称").ShowIn(ShowInWhere.All);
                View.Property(Storage.AddressProperty).HasLabel("仓库地址").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.ResponsiblePersonProperty).HasLabel("负责人").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.AreaProperty).HasLabel("仓库区域").ShowIn(ShowInWhere.ListDetail);
                View.Property(Storage.IsDefaultProperty).HasLabel("默认仓库").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}