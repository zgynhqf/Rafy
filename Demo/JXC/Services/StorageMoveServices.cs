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
    public class AddStorageMoveService : AddService
    {
        protected override Result ExecuteCore()
        {
            var repo = RF.ResolveInstance<StorageRepository>();

            var storageMove = this.Item as StorageMove;
            using (var tran = RF.TransactionScope(repo))
            {
                var storageFrom = storageMove.StorageFrom;
                var StorageTo = storageMove.StorageTo;

                //修改两个仓库对应项的库存
                foreach (StorageMoveItem item in storageMove.StorageMoveItemList)
                {
                    var product = item.Product;

                    var storageProductFrom = storageFrom.FindOrCreateItem(product);
                    if (storageProductFrom.Amount < item.Amount)
                    {
                        return string.Format(
                            "商品 {0} 在仓库 {1} 中的库存量不够，目前数量为 {2}，出库失败。",
                            product.MingCheng, storageFrom.Name, storageProductFrom.Amount
                            );
                    }

                    var storageProductTo = StorageTo.FindOrCreateItem(product);

                    storageProductFrom.Amount -= item.Amount;
                    storageProductTo.Amount += item.Amount;
                }

                RF.Save(storageFrom);
                RF.Save(StorageTo);
                RF.Save(storageMove);

                //提交事务
                tran.Complete();
            }

            this.NewId = storageMove.Id;

            return true;
        }
    }
}