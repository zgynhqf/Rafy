///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20120418
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20120418
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OEA;
//using System.Transactions;
//using OEA.Library;
//using hxy.Common;

//namespace JXC
//{
//    [Serializable]
//    public class AddOtherStorageOutBillService : AddService
//    {
//        protected override Result ExecuteCore()
//        {
//            var storageOut = this.Item as OtherStorageOutBill;
//            if (storageOut == null) throw new ArgumentNullException("storageIn");

//            var repo = RF.Create<OrderStorageInBill>();
//            using (var tran = new TransactionScope())
//            {
//                repo.Save(ref storageOut);

//                //修改库存
//                foreach (StorageOutBillItem item in storageOut.StorageOutBillItemList)
//                {
//                    item.Product.StorageAmount -= item.Amount;
//                    RF.Save(item.Product);
//                }

//                //提交事务
//                tran.Complete();
//            }

//            this.NewId = storageOut.Id;

//            return true;
//        }
//    }
//}