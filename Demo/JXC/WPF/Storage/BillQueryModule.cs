using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JXC.WPF.Templates;
using OEA.MetaModel.View;
using OEA;
using OEA.Module.WPF;
using OEA.MetaModel;

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
                    new SurrounderBlock(typeof(PurchaseOrder), QueryObjectView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OrderStorageInBill), QueryObjectView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OrderStorageInBill), QueryObjectView.ResultSurrounderType)
                    {
                        KeyLabel = "采购入库单 - 报表",
                        BlockType = BlockType.Report,
                    },
                    new SurrounderBlock(typeof(OtherStorageInBill), QueryObjectView.ResultSurrounderType),
                    new SurrounderBlock(typeof(OtherStorageOutBill), QueryObjectView.ResultSurrounderType),
                    new SurrounderBlock(typeof(StorageMove), QueryObjectView.ResultSurrounderType),
                }
            };

            return blocks;
        }

        protected override void OnBlocksDefined(AggtBlocks blocks)
        {
            //TimeSpanCriteria 默认是横向排列的，需要修改此数据
            blocks.MainBlock.ViewMeta.DetailAsHorizontal = false;
            foreach (var sur in blocks.Surrounders)
            {
                ModuleBase.MakeBlockReadonly(sur);
            }

            base.OnBlocksDefined(blocks);
        }

        protected override void OnUIGenerated(ControlResult ui)
        {
            ui.MainView.CastTo<QueryObjectView>().TryExecuteQuery();

            base.OnUIGenerated(ui);
        }
    }
}