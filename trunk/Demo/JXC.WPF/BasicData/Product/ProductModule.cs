/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using JXC.WPF.Templates;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.View;
using Rafy.RBAC.Security;
using Rafy.WPF;

namespace JXC.WPF
{
    public class ProductModule : ModuleBase
    {
        public ProductModule()
        {
            this.EntityType = typeof(Product);
        }

        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();

            //商品和附件位置 7 3 开。
            blocks.Layout.ParentChildProportion = new ParentChildProportion(7, 3);

            return blocks;
        }

        protected override void OnItemCreated(Entity entity)
        {
            base.OnItemCreated(entity);

            var code = RF.Concrete<AutoCodeInfoRepository>().GetOrCreateAutoCode<Product>();

            var p = entity as Product;
            p.BianMa = code;
            p.OperateTime = DateTime.Now;
            var identity = RafyEnvironment.Identity as RafyIdentity;
            if (identity != null) { p.Operator = identity.User; }
        }
    }
}