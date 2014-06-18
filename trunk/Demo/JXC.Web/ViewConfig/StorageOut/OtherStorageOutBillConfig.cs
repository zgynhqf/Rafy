/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 15:28
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.Web.ViewConfig.StorageOut
{
    internal class OtherStorageOutBillConfig : WebViewConfig<OtherStorageOutBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("其它出库单").HasDelegate(OtherStorageOutBill.CodeProperty);

            View.HasDetailColumnsCount(2);

            using (View.OrderProperties())
            {
                View.Property(StorageOutBill.CodeProperty).HasLabel("出库单编号").ShowIn(ShowInWhere.All);
                View.Property(StorageOutBill.DateProperty).HasLabel("出库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(OtherStorageOutBill.StorageProperty).HasLabel("发出仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(OtherStorageOutBill.CustomerProperty).HasLabel("收货单位").ShowIn(ShowInWhere.ListDetail)
                    .UseDataSource(EntityDataSources.Customers);//只显示客户，不显示供应商

                View.Property(StorageOutBill.TotalAmountProperty).HasLabel("总数量").ShowIn(ShowInWhere.ListDetail)
                    .Readonly();
                View.Property(StorageOutBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}