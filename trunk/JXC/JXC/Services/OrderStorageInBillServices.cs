/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120418
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120418
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using System.Transactions;
using OEA.Library;
using hxy.Common;

namespace JXC
{
    [Serializable]
    public class AddOrderStorageInBillService : AddService
    {
        protected override Result ExecuteCore()
        {
            var storageIn = this.Item as OrderStorageInBill;
            if (storageIn == null) throw new ArgumentNullException("storageIn");

            var repo = RF.Create<OrderStorageInBill>();
            using (var tran = RF.TransactionScope(repo))
            {
                repo.Save(ref storageIn);

                //修改对应的采购订单
                var order = storageIn.Order;
                foreach (StorageInBillItem item in storageIn.StorageInItemList)
                {
                    var orderItem = order.PurchaseOrderItemList.Cast<PurchaseOrderItem>()
                        .FirstOrDefault(e => e.ProductId == item.ProductId);
                    if (orderItem == null)
                    {
                        return string.Format("采购订单中没有这一项商品：{0}，入库失败。", item.Product.MingCheng);
                    }
                    if (orderItem.AmountLeft < item.Amount)
                    {
                        return string.Format("超出采购订单中商品“{0}”的当前剩余数目：{1}。", item.Product.MingCheng, orderItem.AmountLeft);
                    }

                    orderItem.AmountLeft -= item.Amount;
                }
                if (order.TotalAmountLeft == 0)
                {
                    order.StorageInStatus = OrderStorageInStatus.Completed;
                }
                RF.Save(order);

                //修改库存
                foreach (StorageInBillItem item in storageIn.StorageInItemList)
                {
                    item.Product.StorageAmount += item.Amount;
                    RF.Save(item.Product);
                }

                //提交事务
                tran.Complete();
            }

            this.NewId = storageIn.Id;

            return true;
        }
    }

    //[Serializable]
    //public class DeleteOrderStorageInBillService : DeleteService
    //{
    //    protected override Result ExecuteCore()
    //    {
    //        var orderId = this.ItemId;
    //        var repo = RF.Create<OrderStorageInBill>();

    //        //var osibRepo = RF.Concreate<OrderStorageInBillRepository>();

    //        //var storageInList = osibRepo.GetByOrderId(orderId);
    //        //if (storageInList.Count > 0)
    //        //{
    //        //    this.Result = "该定单已经生成入库单，不直接删除。";
    //        //    return;
    //        //}

    //        //var order = RF.Create<PurchaseOrder>().GetById(orderId);
    //        //order.MarkDeleted();
    //        //RF.Save(order);

    //        return true;
    //    }
    //}
}