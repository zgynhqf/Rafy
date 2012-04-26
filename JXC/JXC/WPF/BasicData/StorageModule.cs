using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JXC.WPF.Templates;
using OEA.MetaModel.View;
using OEA.Library;

namespace JXC.WPF
{
    public class StorageModule : ModuleBase
    {
        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();

            //把所有孩子块上的非查询型命令都删除
            foreach (var child in blocks.Children)
            {
                MakeBlockReadonly(child);
            }

            return blocks;
        }

        protected override void OnItemCreated(Entity entity)
        {
            base.OnItemCreated(entity);

            var code = RF.Concreate<AutoCodeInfoRepository>().GetOrCreateAutoCode<Storage>();
            var p = entity as Storage;
            p.Code = code;
        }
    }
}
