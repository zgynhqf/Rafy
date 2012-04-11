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
        public static readonly Property<string> BianMaProperty = P<Storage>.Register(e => e.BianMa);
        public string BianMa
        {
            get { return this.GetProperty(BianMaProperty); }
            set { this.SetProperty(BianMaProperty, value); }
        }

        public static readonly Property<string> MingChengProperty = P<Storage>.Register(e => e.MingCheng);
        public string MingCheng
        {
            get { return this.GetProperty(MingChengProperty); }
            set { this.SetProperty(MingChengProperty, value); }
        }

        public static readonly Property<string> DiZhiProperty = P<Storage>.Register(e => e.DiZhi);
        public string DiZhi
        {
            get { return this.GetProperty(DiZhiProperty); }
            set { this.SetProperty(DiZhiProperty, value); }
        }

        public static readonly Property<string> FuZeRenProperty = P<Storage>.Register(e => e.FuZeRen);
        public string FuZeRen
        {
            get { return this.GetProperty(FuZeRenProperty); }
            set { this.SetProperty(FuZeRenProperty, value); }
        }

        public static readonly Property<string> QuYuProperty = P<Storage>.Register(e => e.QuYu);
        public string QuYu
        {
            get { return this.GetProperty(QuYuProperty); }
            set { this.SetProperty(QuYuProperty, value); }
        }

        public static readonly Property<bool> MoRenCangKuProperty = P<Storage>.Register(e => e.MoRenCangKu);
        public bool MoRenCangKu
        {
            get { return this.GetProperty(MoRenCangKuProperty); }
            set { this.SetProperty(MoRenCangKuProperty, value); }
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
            base.ConfigMeta();

            Meta.MapTable().HasColumns(
                Storage.BianMaProperty,
                Storage.MingChengProperty,
                Storage.DiZhiProperty,
                Storage.FuZeRenProperty,
                Storage.QuYuProperty,
                Storage.MoRenCangKuProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("仓库").HasTitle(Storage.MingChengProperty);

            View.Property(Storage.BianMaProperty).HasLabel("仓库编码").ShowIn(ShowInWhere.All);
            View.Property(Storage.MingChengProperty).HasLabel("仓库名称").ShowIn(ShowInWhere.All);
            View.Property(Storage.DiZhiProperty).HasLabel("仓库地址").ShowIn(ShowInWhere.All);
            View.Property(Storage.FuZeRenProperty).HasLabel("负责人").ShowIn(ShowInWhere.All);
            View.Property(Storage.QuYuProperty).HasLabel("仓库区域").ShowIn(ShowInWhere.All);
            View.Property(Storage.MoRenCangKuProperty).HasLabel("默认仓库").ShowIn(ShowInWhere.All);
        }
    }
}