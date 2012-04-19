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
using hxy.Common;
using OEA.Library;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.WPF.Command;
using System.Windows;

namespace JXC.Commands
{
    [Command(Label = "入库完成", ToolTip = "标记该订单已入库完成", GroupType = CommandGroupType.Business)]
    class CompletePurchaseOrder : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            var e = view.Current as PurchaseOrder;
            return e != null && e.StorageInStatus == OrderStorageInStatus.Waiting;
        }

        public override void Execute(ListObjectView view)
        {
            var order = view.Current as PurchaseOrder;

            var msg = string.Format("系统将会为采购单 {0} 剩余的所有商品自动入库，并生成相应的入库单。\r\n是否继续？", order.Code);
            var btn = App.MessageBox.Show(msg, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (btn == MessageBoxResult.Yes)
            {
                var svc = new CompletePurchaseOrderService { OrderId = order.Id };
                svc.Invoke();

                App.MessageBox.Show(svc.Result.Message);

                if (svc.Result.Success) { view.DataLoader.ReloadDataAsync(); }
            }
        }
    }
}
