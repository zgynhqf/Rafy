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
    public class AddPurchaseOrderService : AddService
    {
        protected override Result ExecuteCore()
        {
            var po = this.Item as PurchaseOrder;
            var poRepo = RF.Concreate<PurchaseOrderRepository>();

            using (var tran = new TransactionScope())
            {
                poRepo.Save(ref po);

                //生成入库单
                if (po.StorageInDirectly)
                {
                    var storageIn = new OrderStorageInBill
                    {
                        Order = po,
                        Code = po.Code + " - 入库",
                        Date = DateTime.Now,
                        Comment = "系统生成"
                    };

                    //调用另外一个服务直接入库
                    var siService = new AddOrderStorageInBillService
                    {
                        Item = storageIn
                    };
                    siService.Invoke();

                    if (!siService.Result) { return siService.Result; }
                }

                //提交事务
                tran.Complete();
            }

            this.NewId = po.Id;

            return true;
        }
    }

    [Serializable]
    public class DeletePurchaseOrderService : DeleteService
    {
        protected override Result ExecuteCore()
        {
            var orderId = this.ItemId;
            var osibRepo = RF.Concreate<OrderStorageInBillRepository>();

            var storageInList = osibRepo.GetByOrderId(orderId);
            if (storageInList.Count > 0)
            {
                return "该定单已经有相应的入库记录，不能直接删除。";
            }

            var order = RF.Create<PurchaseOrder>().GetById(orderId);
            order.MarkDeleted();
            RF.Save(order);

            return true;
        }
    }

    [Serializable]
    public class CompletePurchaseOrderService : FlowService
    {
        [ServiceInput]
        public int OrderId { get; set; }

        protected override Result ExecuteCore()
        {
            var order = RF.Create<PurchaseOrder>().GetById(this.OrderId) as PurchaseOrder;
            if (order == null) throw new ArgumentNullException("order");

            if (order.StorageInStatus != OrderStorageInStatus.Waiting) { return "该订单已经完全入库，操作失败。"; }

            //生成入库单
            var storageIn = new OrderStorageInBill
            {
                Order = order,
                Code = order.Code + " - 入库",
                Date = DateTime.Now,
                Comment = "系统生成"
            };

            //调用另外一个服务直接入库
            var siService = new AddOrderStorageInBillService { Item = storageIn };
            siService.Invoke();
            return siService.Result;
        }
    }
}