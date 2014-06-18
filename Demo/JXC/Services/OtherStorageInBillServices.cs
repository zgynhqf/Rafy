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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using System.Transactions;
using Rafy.Domain;

namespace JXC
{
    [Serializable]
    [Contract, ContractImpl]
    public class AddOtherStorageInBillService : AddService
    {
        protected override Result ExecuteCore()
        {
            var storageIn = this.Item as StorageInBill;
            if (storageIn == null) throw new ArgumentNullException("storageIn");

            var repo = RF.Concrete<OrderStorageInBillRepository>();
            using (var tran = RF.TransactionScope(repo))
            {
                repo.Save(storageIn);

                //修改所在仓库库存
                var storage = storageIn.Storage;
                foreach (StorageInBillItem item in storageIn.StorageInItemList)
                {
                    var product = item.Product;

                    //同时修改该仓库的数量，以及商品的数量
                    var storageProduct = storage.FindOrCreateItem(product);

                    storageProduct.Amount += item.Amount;
                    product.StorageAmount += item.Amount;
                    RF.Save(product);
                }
                RF.Save(storage);

                //提交事务
                tran.Complete();
            }

            this.NewId = storageIn.Id;

            return true;
        }
    }
}