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
using JXC.Commands;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.WPF.ViewConfig.BasicData
{
    internal class OrderStorageInBillConfig : WPFViewConfig<OrderStorageInBill>
    {
        protected override void ConfigView()
        {
            View.DomainName("采购入库单").HasDelegate(StorageInBill.CodeProperty);

            View.UseReport("采购入库单报表统计.rdlc");

            View.HasDetailColumnsCount(2);

            View.UseCommands(
                typeof(AddOrderStorageInBill),
                //typeof(DeleteStorageInBill),
                typeof(ShowBill),
                WPFCommandNames.Refresh
                );

            using (View.OrderProperties())
            {
                View.Property(StorageInBill.CodeProperty).HasLabel("商品入库编号").ShowIn(ShowInWhere.All);
                View.Property(StorageInBill.TotalMoneyProperty).HasLabel("总金额").ShowIn(ShowInWhere.ListDetail).Readonly();

                View.Property(OrderStorageInBill.OrderProperty).HasLabel("商品订单").ShowIn(ShowInWhere.ListDetail)
                    .UseDataSource(OrderStorageInBill.PurchaseOrderDataSourceProperty);
                View.Property(OrderStorageInBill.StorageProperty).HasLabel("收入仓库").ShowIn(ShowInWhere.ListDetail);
                View.Property(OrderStorageInBill.View_SupplierNameProperty).HasLabel("供应商").ShowIn(ShowInWhere.ListDetail);

                View.Property(StorageInBill.DateProperty).HasLabel("入库日期").ShowIn(ShowInWhere.ListDetail);
                View.Property(StorageInBill.CommentProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}