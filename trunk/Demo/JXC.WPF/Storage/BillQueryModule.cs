using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using JXC.WPF.Templates;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace JXC.WPF
{
    public class BillQueryModule : UITemplate
    {
        public BillQueryModule()
        {
            this.EntityType = typeof(TimeSpanCriteria);
        }

        protected override AggtBlocks DefineBlocks()
        {
            var blocks = new AggtBlocks
            {
                Layout = new LayoutMeta(typeof(BillQueryLayout)),
                MainBlock = new ConditionBlock(this.EntityType),
                Surrounders =
                {
                    new SurrounderBlock(typeof(PurchaseOrder), QueryLogicalView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OrderStorageInBill), QueryLogicalView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OrderStorageInBill), QueryLogicalView.ResultSurrounderType)
                    {
                        KeyLabel = "采购入库单 - 报表",
                        BlockType = BlockType.Report,
                    },
                    new SurrounderBlock(typeof(OtherStorageInBill), QueryLogicalView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OtherStorageOutBill), QueryLogicalView.ResultSurrounderType),
                    new SurrounderBlock(typeof(StorageMove), QueryLogicalView.ResultSurrounderType),
                }
            };

            return blocks;
        }

        protected override void OnBlocksDefined(AggtBlocks blocks)
        {
            //TimeSpanCriteria 默认是横向排列的，需要修改此数据
            blocks.MainBlock.ViewMeta.AsWPFView().UseDetailAsHorizontal(false);
            foreach (var sur in blocks.Surrounders)
            {
                ModuleBase.MakeBlockReadonly(sur);
            }

            base.OnBlocksDefined(blocks);
        }

        protected override void OnUIGenerated(ControlResult ui)
        {
            ui.MainView.CastTo<QueryLogicalView>().TryExecuteQuery();

            base.OnUIGenerated(ui);
        }
    }
}