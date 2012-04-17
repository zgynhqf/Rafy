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
            return meta.ShowInDetail(columnSpan: 2, height: 200)
                .UseEditor(WPFEditorNames.Memo);
        }
    }
}
