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
    public class AddOtherStorageInBillService : AddService
    {
        protected override Result ExecuteCore()
        {
            var storageIn = this.Item as StorageInBill;
            if (storageIn == null) throw new ArgumentNullException("storageIn");

            var repo = RF.Create<OrderStorageInBill>();
            using (var tran = RF.TransactionScope(repo))
            {
                repo.Save(ref storageIn);

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
}