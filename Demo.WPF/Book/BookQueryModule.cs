using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF;
using OEA.MetaModel.View;

namespace Demo.WPF
{
    public class BookQueryModule : CallbackTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            return UIModel.AggtBlocks.GetDefinedBlocks("书籍查询界面");
        }
    }
}