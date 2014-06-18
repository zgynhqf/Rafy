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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using JXC.WPF.Templates;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;
using Rafy.WPF.Command;

namespace JXC.Commands
{
    [Command(ImageName = "Add.bmp", Label = "添加采购订单", ToolTip = "添加一个采购订单",
        GroupType = CommandGroupType.Edit, Gestures = "F2")]
    public class AddPurchaseOrder : AddBill
    {
        public AddPurchaseOrder()
        {
            this.Service = ServiceFactory.Create<AddPurchaseOrderService>();
            this.Template = new AddOrderBillTemplate();
        }

        protected override void OnServiceInvoking(DetailLogicalView detailView, CancelEventArgs e)
        {
            base.OnServiceInvoking(detailView, e);

            var entity = detailView.Current as PurchaseOrder;
            if (entity.StorageInDirectly)
            {
                var btn = App.MessageBox.Show("您选择了直接入库，系统将会为本次采购自动生成一张相应的入库单。\r\n是否继续？".Translate(), MessageBoxButton.YesNo);
                if (btn == MessageBoxResult.No) e.Cancel = true;
            }
        }

        private class AddOrderBillTemplate : BillTemplate
        {
            protected override AggtBlocks DefineBlocks()
            {
                var blocks = base.DefineBlocks();

                blocks.Children.ReOrder(typeof(PurchaseOrderItem), typeof(PurchaseOrderAttachement));

                return blocks;
            }
        }
    }

    [Command(ImageName = "Delete.bmp", Label = "删除", ToolTip = "删除一个订单", GroupType = CommandGroupType.Edit)]
    class DeletePurchaseOrder : DeleteBill
    {
        public DeletePurchaseOrder()
        {
            this.Service = ServiceFactory.Create<DeletePurchaseOrderService>();
        }
    }

    [Command(Label = "入库完成", ToolTip = "标记该订单已入库完成", GroupType = CommandGroupType.Business)]
    class CompletePurchaseOrder : ListViewCommand
    {
        public override bool CanExecute(ListLogicalView view)
        {
            var e = view.Current as PurchaseOrder;
            return e != null && e.StorageInStatus == OrderStorageInStatus.Waiting;
        }

        public override void Execute(ListLogicalView view)
        {
            var order = view.Current as PurchaseOrder;

            var msg = string.Format("系统将会为采购单 {0} 剩余的所有商品自动入库，并生成相应的入库单。\r\n是否继续？".Translate(), order.Code);
            var btn = App.MessageBox.Show(msg, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (btn == MessageBoxResult.Yes)
            {
                var svc = ServiceFactory.Create<CompletePurchaseOrderService>();
                svc.OrderId = order.Id;
                svc.Invoke();

                App.MessageBox.Show(svc.Result.Message.Translate());

                if (svc.Result.Success) { view.DataLoader.ReloadDataAsync(); }
            }
        }
    }
}