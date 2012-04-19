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
using OEA.Library;
using hxy.Common;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC.Commands
{
    [Command(ImageName = "Delete.bmp", Label = "删除", ToolTip = "删除一个订单", GroupType = CommandGroupType.Edit)]
    class DeletePurchaseOrder : DeleteBill
    {
        public DeletePurchaseOrder()
        {
            this.Service = new DeletePurchaseOrderService();
        }
    }
}
