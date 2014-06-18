using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Demo
{
    [CompiledPropertyDeclarer]
    class BookExt
    {
        public static readonly Property<DateTime> StorageInDateProperty = P<Book>.RegisterExtension("StorageInDate", typeof(BookExt), DateTime.Now);
        public static DateTime GetStorageInDate(Book me)
        {
            return me.GetProperty(StorageInDateProperty);
        }
        public static void SetStorageInDate(Book me, DateTime value)
        {
            me.SetProperty(StorageInDateProperty, value);
        }
    }

    class BookExtConfig : EntityConfig<Book>
    {
        protected override void ConfigMeta()
        {
            Meta.Property(BookExt.StorageInDateProperty).MapColumn().HasColumnName("StorageInDate");
        }
    }

    class BookExtWPFConfig : WPFViewConfig<Book>
    {
        protected override void ConfigView()
        {
            View.Property(BookExt.StorageInDateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.List);
        }
    }
}
