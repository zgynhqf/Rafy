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
        public static EntityPropertyViewMeta ShowMemoInDetail(this EntityPropertyViewMeta meta)
        {
            return meta.ShowInDetail(height: 100, columnSpan: 2)
                .UseEditor(WPFEditorNames.Memo);
        }
    }
}
