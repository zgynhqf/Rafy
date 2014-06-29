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
using Rafy;
using Rafy.ComponentModel;
using Rafy.MetaModel;
using Rafy.WPF;

namespace JXC.WPF
{
    public class JXCWPFPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            InitModules(app);

            AddNewPropertyEditors(app);

            InitClient(app);
        }

        private static void AddNewPropertyEditors(IApp app)
        {
            app.AllPluginsIntialized += (o, e) =>
            {
                AutoUI.BlockUIFactory.PropertyEditorFactory.Set("ImageSelector", typeof(ImagePropertyEditor));
            };
        }

        private static void InitModules(IApp app)
        {
            app.ModuleOperations += (o, e) =>
            {
                var moduleJXC = CommonModel.Modules.AddRoot(new WPFModuleMeta
                {
                    Label = "进销存系统示例",
                    Children =
                    {
                        new WPFModuleMeta
                        {
                            Label = "基础数据",
                            Children =
                            {
                                //new WPFModuleMeta{ Label = "计量单位", EntityType = typeof(Unit)},
                                new WPFModuleMeta{ Label = "仓库管理", EntityType = typeof(Storage), BlocksTemplate = typeof(StorageModule)},
                                new WPFModuleMeta{ Label = "商品类别", EntityType = typeof(ProductCategory)},
                                new WPFModuleMeta{ Label = "商品管理", EntityType = typeof(Product), BlocksTemplate= typeof(ProductModule)},
                                new WPFModuleMeta{ Label = "客户类别", EntityType = typeof(ClientCategory)},
                                new WPFModuleMeta{ Label = "客户管理", EntityType = typeof(ClientInfo)},
                            }
                        },
                        new WPFModuleMeta
                        {
                            Label = "采购管理",
                            Children =
                            {
                                new WPFModuleMeta{ Label = "采购订单", EntityType = typeof(PurchaseOrder), BlocksTemplate= typeof(PurchaseOrderModule)},
                                new WPFModuleMeta{ Label = "采购订单入库", EntityType = typeof(OrderStorageInBill), BlocksTemplate= typeof(OrderStorageInModule)},
                            }
                        },
                        new WPFModuleMeta
                        {
                            Label = "库存管理",
                            Children =
                            {
                                new WPFModuleMeta{ Label = "其它入库", EntityType = typeof(OtherStorageInBill), BlocksTemplate= typeof(OtherStorageInModule)},
                                new WPFModuleMeta{ Label = "其它出库", EntityType = typeof(OtherStorageOutBill), BlocksTemplate= typeof(OtherStorageOutModule)},
                                new WPFModuleMeta{ Label = "库存调拔", EntityType = typeof(StorageMove), BlocksTemplate= typeof(StorageMoveModule)},
                            }
                        },
                        new WPFModuleMeta{ Label = "单据查询", EntityType = typeof(TimeSpanCriteria), BlocksTemplate= typeof(BillQueryModule)},
                        new WPFModuleMeta
                        {
                            Label = "系统管理",
                            Children =
                            {
                                new WPFModuleMeta{ Label = "自动编码管理", EntityType = typeof(AutoCodeInfo)},
                            }
                        },
                    }
                });
            };
        }

        private static void InitClient(IApp app)
        {
            //var clientApp = app as IClientApp;
            //if (clientApp != null)
            //{
            //    clientApp.MainWindowLoaded += (o, e) =>
            //    {
            //        App.Current.OpenModuleOrAlert("商品管理");
            //        App.Current.OpenModuleOrAlert("采购订单");
            //        App.Current.OpenModuleOrAlert("采购订单入库");
            //        App.Current.OpenModuleOrAlert("其它入库");
            //        App.Current.OpenModuleOrAlert("其它出库");

            //        App.Current.OpenModuleOrAlert("商品管理");
            //    };
            //}
        }
    }
}