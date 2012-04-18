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
using OEA.MetaModel.View;

namespace JXC
{
    public static class MetaExt
    {
        /// <summary>
        /// 设置为单据模板
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static EntityViewMeta IsBill(this EntityViewMeta meta)
        {
            meta.ClearWPFCommands(false);
            meta.UseWPFCommands(
                "JXC.Commands.AddBill",
                "JXC.Commands.EditBill",
                "JXC.Commands.DeleteBill",
                "JXC.Commands.ShowBill"
                );
            meta.UseWPFCommands(WPFCommandNames.Refresh);

            return meta;
        }

        public static EntityPropertyViewMeta ShowMemoInDetail(this EntityPropertyViewMeta meta)
        {
            return meta.ShowInDetail(columnSpan: 2, height: 100)
                .UseEditor(WPFEditorNames.Memo);
        }
    }
}
