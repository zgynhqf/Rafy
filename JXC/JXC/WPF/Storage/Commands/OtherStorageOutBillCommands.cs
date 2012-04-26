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
using JXC.WPF.Templates;
using OEA;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;
using OEA.WPF.Command;
using OEA.Library;

namespace JXC.Commands
{
    [Command(ImageName = "Add.bmp", Label = "出库", ToolTip = "添加一个出库单", GroupType = CommandGroupType.Edit)]
    public class AddOtherStorageOutBill : AddBill
    {
        public AddOtherStorageOutBill()
        {
            this.Service = new AddOtherStorageOutBillService();
        }
    }
}