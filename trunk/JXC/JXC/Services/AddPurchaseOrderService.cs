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

namespace JXC
{
    [Serializable]
    public class AddPurchaseOrderService : Service
    {
        [ServiceInput]
        public PurchaseOrder Item { get; set; }

        [ServiceOutput]
        public int NewId { get; set; }

        protected override void Execute()
        {
            var po = this.Item;
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
                        TotalMoney = po.TotalMoney,
                        Date = DateTime.Now,
                    };
                    foreach (PurchaseOrderItem item in po.PurchaseOrderItemList)
                    {
                        storageIn.StorageInItemList.Add(new StorageInItem
                        {
                            ProductId = item.ProductId,
                            Amount = item.Amount
                        });
                    }
                    RF.Save(storageIn);

                    //修改库存
                    foreach (PurchaseOrderItem item in po.PurchaseOrderItemList)
                    {
                        item.Product.StorageAmount += item.Amount;
                        RF.Save(item.Product);
                    }
                }

                //提交事务
                tran.Complete();
            }

            this.NewId = po.Id;
        }
    }
}