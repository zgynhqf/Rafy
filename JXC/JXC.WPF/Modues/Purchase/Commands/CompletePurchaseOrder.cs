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
            App.MessageBox.Show("暂时还没有完成本功能，待入库操作完成后再添加。");
        }
    }
}
