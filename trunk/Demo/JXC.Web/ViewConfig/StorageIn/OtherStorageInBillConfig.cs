/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 15:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.Web.ViewConfig.BasicData
{
    internal class OtherStorageInBillConfig : WebViewConfig<OtherStorageInBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("其它入库单").HasDelegate(StorageInBill.CodeProperty);

            View.HasDetailColumnsCount(2);

            using (View.OrderProperties())
            {
                View.Property(StorageInBill.CodeProperty).HasLabel("商品入库编号").ShowIn(ShowInWhere.All);
                View.Property(StorageInBill.TotalMoneyProperty).HasLabel("金额").ShowIn(ShowInWhere.ListDetail);

                View.Property(OtherStorageInBill.StorageProperty).HasLabel("收入仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(OtherStorageInBill.SupplierProperty).HasLabel("发货单位").ShowIn(ShowInWhere.ListDetail)
                    .UseDataSource(EntityDataSources.Suppliers);//只下拉获取经销商的信息

                View.Property(StorageInBill.DateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageInBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}