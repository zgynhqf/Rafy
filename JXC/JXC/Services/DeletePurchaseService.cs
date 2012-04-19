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
    public class DeletePurchaseService : DeleteService
    {
        protected override Result ExecuteCore()
        {
            var orderId = this.ItemId;
            var osibRepo = RF.Concreate<OrderStorageInBillRepository>();

            var storageInList = osibRepo.GetByOrderId(orderId);
            if (storageInList.Count > 0)
            {
                return "该定单已经有相应的入库记录，不直接删除。";
            }

            var order = RF.Create<PurchaseOrder>().GetById(orderId);
            order.MarkDeleted();
            RF.Save(order);

            return true;
        }
    }
}