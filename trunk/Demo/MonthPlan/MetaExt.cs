/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121102 16:03
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121102 16:03
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel.View;

namespace MP
{
    public static class MetaExt
    {
        public static WPFEntityPropertyViewMeta ShowMemoInDetail(this WPFEntityPropertyViewMeta meta)
        {
            return meta.ShowInDetail(height: 100, columnSpan: 2)
                .UseEditor(WPFEditorNames.Memo);
        }

        public static WPFEntityPropertyViewMeta UseMemoEditor(this WPFEntityPropertyViewMeta meta)
        {
            return meta.UseEditor(WPFEditorNames.Memo);
        }
    }
}