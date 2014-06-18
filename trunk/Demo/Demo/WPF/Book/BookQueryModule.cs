using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.WPF;
using Rafy.MetaModel.View;

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