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
using JXC.WPF.Templates;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace JXC.Commands
{
    [Command(ImageName = "Add.bmp", Label = "入库", ToolTip = "添加一个采购入库单", GroupType = CommandGroupType.Edit)]
    public class AddOrderStorageInBill : AddBill
    {
        public AddOrderStorageInBill()
        {
            this.Template = new OrderStoreageIn_CustomizedBy_ExtendView();
            this.Service = ServiceFactory.Create<AddOrderStorageInBillService>();
        }

        #region 定制视图 - ★ 三大实现方案 ★

        /*********************** 代码块解释 *********************************
         * 
         * 需求：
         * * 采购订单入库界面的商品单价是不能被修改的，因为它和订单中写的单价应该是一致的；
         * * 同时，这个界面不需要条码录入商品的功能。
         * 
         * 一般情况下，单块界面都是在实体默认界面配置中进行的。
         * 但是如果某个实体在一些地方需要显示得比较特殊的时候，就需要用到特殊视图的定制功能了。
         * 
         * 以下给出了几种不同的实现方案，用于修改聚合块中的某一块界面。
         * 
        **********************************************************************/

        /// <summary>
        /// 方案：运行时修改。
        /// 
        /// 生成控件后，对运行时 ListLogicalView 对象中的控件对象进行控制的方法。
        /// 
        /// 优点：
        /// * 方便
        /// 
        /// 缺点：
        /// * 只能修改一些运行时支持的属性。
        ///     例如，由于界面中的控件由于已经生成完毕，如果此时修改 Meta 的 Label 的话，该列的名称依然不会改变。
        /// * 不支持客户化
        /// * 不支持权限读取
        /// </summary>
        private class OrderStoreageIn_CustomizedBy_ListLogicalView : BillTemplate
        {
            protected override void OnUIGenerated(ControlResult ui)
            {
                base.OnUIGenerated(ui);

                var itemView = ui.MainView.GetChildView(typeof(StorageInBillItem));

                var treeGrid = itemView.Control.CastTo<RafyTreeGrid>();
                var column = treeGrid.FindColumnByProperty(StorageInBillItem.UnitPriceProperty);
                if (column != null)
                {
                    column.Meta.Readonly();

                    column.Meta.Label = "订单单价";//注意，此行代码将会无效
                }

                //只能把该按钮隐藏
                itemView.Commands[typeof(BarcodeSelectProduct)].IsVisible = false;
            }
        }

        /// <summary>
        /// 方案：生成期前直接修改元数据。
        /// 
        /// 优点：
        /// * 方便
        /// 
        /// 缺点：
        /// * 不支持客户化
        /// * 不支持权限读取
        /// </summary>
        private class OrderStoreageIn_CustomizedBy_BlocksView : BillTemplate
        {
            protected override AggtBlocks DefineBlocks()
            {
                var blocks = base.DefineBlocks();

                var vm = blocks.Children[typeof(StorageInBillItem)].MainBlock.ViewMeta;

                vm.Property(StorageInBillItem.UnitPriceProperty).HasLabel("订单单价").Readonly();
                vm.AsWPFView().RemoveCommands(typeof(BarcodeSelectProduct));

                return blocks;
            }
        }

        /// <summary>
        /// 方案：定义某实体的扩展视图。
        /// 
        /// 优点：
        /// * 支持客户化
        /// * 支持权限读取
        /// * 扩展视图实体配置，可读性与实体配置保持一致。并可把该实体的所有视图配置统一存放、管理。
        /// </summary>
        private class OrderStoreageIn_CustomizedBy_ExtendView : BillTemplate
        {
            protected override AggtBlocks DefineBlocks()
            {
                var blocks = base.DefineBlocks();

                blocks.Children[typeof(StorageInBillItem)].MainBlock.ExtendView = typeof(StorageInBillItemConfig);

                return blocks;
            }
        }
        private class StorageInBillItemConfig : WPFViewConfig<StorageInBillItem>
        {
            protected override bool IsExtendView
            {
                get { return true; }
            }

            protected override void ConfigView()
            {
                View.RemoveCommands(typeof(BarcodeSelectProduct));

                View.Property(StorageInBillItem.UnitPriceProperty).HasLabel("订单单价").Readonly();
            }
        }

        #endregion
    }

    //[Command(ImageName = "Delete.bmp", Label = "删除", ToolTip = "删除一个采购入库单", GroupType = CommandGroupType.Edit)]
    //public class DeleteStorageInBill : DeleteBill
    //{
    //    public DeleteStorageInBill()
    //    {
    //        this.Service = new DeleteOrderStorageInBillService();
    //    }
    //}
}