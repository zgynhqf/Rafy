/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 14:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JXC.Web.Templates;
using Rafy;
using Rafy.ComponentModel;
using Rafy.MetaModel;

namespace JXC.Web
{
    public class JXCWebPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            InitModules(app);
        }

        private static void InitModules(IApp app)
        {
            app.MetaCreating += (o, e) =>
            {
                var moduleJXC = CommonModel.Modules.AddRoot(new WebModuleMeta
                {
                    Label = "进销存系统示例",
                    Children =
                    {
                        new WebModuleMeta
                        {
                            Label = "基础数据",
                            Children =
                            {
                                //new WebModuleMeta{ Label = "计量单位", EntityType = typeof(Unit)},
                                new WebModuleMeta{ Label = "仓库管理", EntityType = typeof(Storage)},
                                new WebModuleMeta{ Label = "商品类别", EntityType = typeof(ProductCategory)},
                                new WebModuleMeta{ Label = "商品管理", EntityType = typeof(Product), ClientRuntime = "Jxc.ProductModule"},
                                new WebModuleMeta{ Label = "客户类别", EntityType = typeof(ClientCategory)},
                                new WebModuleMeta{ Label = "客户管理", EntityType = typeof(ClientInfo)},
                            }
                        },
                        new WebModuleMeta
                        {
                            Label = "采购管理",
                            Children =
                            {
                                new WebModuleMeta{ Label = "采购订单", EntityType = typeof(PurchaseOrder), ClientRuntime = "Jxc.PurchaseOrderModule", BlocksTemplate=typeof(ConditionQueryBlocksTemplate)},
                                new WebModuleMeta{ Label = "采购订单入库", EntityType = typeof(OrderStorageInBill)},
                            }
                        },
                        new WebModuleMeta
                        {
                            Label = "库存管理",
                            Children =
                            {
                                new WebModuleMeta{ Label = "其它入库", EntityType = typeof(OtherStorageInBill)},
                                new WebModuleMeta{ Label = "其它出库", EntityType = typeof(OtherStorageOutBill)},
                                new WebModuleMeta{ Label = "库存调拔", EntityType = typeof(StorageMove)},
                            }
                        },
                        new WebModuleMeta{ Label = "单据查询", EntityType = typeof(TimeSpanCriteria)},
                        new WebModuleMeta
                        {
                            Label = "系统管理",
                            Children =
                            {
                                new WebModuleMeta{ Label = "自动编码管理", EntityType = typeof(AutoCodeInfo)},
                            }
                        },
                    }
                });
            };
        }
    }
}