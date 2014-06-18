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
using JXC.WPF.Templates;
using Rafy;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;
using Rafy.Domain;

namespace JXC.Commands
{
    [Command(ImageName = "Add.bmp", Label = "仓库调拔", ToolTip = "添加一个仓库调拔单", GroupType = CommandGroupType.Edit)]
    public class AddStorageMoveBill : AddBill
    {
        public AddStorageMoveBill()
        {
            this.Service = ServiceFactory.Create<AddStorageMoveService>();
        }
    }

    [Command(ImageName = "Add.bmp", Label = "选择出库商品", GroupType = CommandGroupType.Edit)]
    public class SelectStorageProduct : SelectProductCommand
    {
        public SelectStorageProduct()
        {
            this.Template.BlocksDefined += Template_BlocksDefined;
            this.Template.UIGenerated += Template_UIGenerated;
        }

        private void Template_BlocksDefined(object sender, BlocksDefinedEventArgs e)
        {
            //这里的选择商品界面不需要编辑功能
            var mainViewMeta = e.Blocks.MainBlock.ViewMeta;
            mainViewMeta.DisableEditing().AsWPFView().ClearCommands();
        }

        private void Template_UIGenerated(object sender, UIGeneratedEventArgs e)
        {
            //弹出的视图只显示出货仓库中有的商品
            var listView = e.UI.MainView as ListLogicalView;
            listView.Filter = entity =>
            {
                var from = (this._view.Parent.Current as StorageMove).StorageFrom;

                var product = entity as Product;

                var storageProduct = from.FindItem(product);

                //注意，这里为了方便起见，顺便把商品的数量都设置为仓库的数量
                if (storageProduct != null)
                {
                    product.StorageAmount = storageProduct.Amount;
                    return true;
                }

                return false;
            };
        }

        private ListLogicalView _view;

        public override bool CanExecute(ListLogicalView view)
        {
            //当选择了出库仓库时，才可以选择出库商品
            var move = view.Parent.Current as StorageMove;
            return move != null && move.StorageFromId != 0;
        }

        public override void Execute(ListLogicalView view)
        {
            this._view = view;

            base.Execute(view);
        }
    }
}