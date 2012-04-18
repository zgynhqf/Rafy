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
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;

namespace JXC.Commands
{
    [Command(Label = "编辑", GroupType = CommandGroupType.Edit)]
    public class EditBill : ListViewCommand
    {
        public override bool CanExecute(ListObjectView view)
        {
            return view.Current != null;
        }

        public override void Execute(ListObjectView view) { }
    }
}