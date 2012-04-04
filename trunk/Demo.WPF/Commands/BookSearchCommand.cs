using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;
using OEA.Module.WPF;
using OEA.Module.WPF.CommandAutoUI;
using OEA.MetaModel.View;

namespace Demo.WPF.Commands
{
    [Command(Label = "过滤书籍", UIAlgorithm = typeof(GenericItemAlgorithm<TextBoxButtonItemGenerator>))]
    public class BookSearchCommand : ListViewCommand
    {
        public override void Execute(ListObjectView view)
        {
            var txt = this.TryGetCustomParams<string>(CommandCustomParams.TextBox);

            view.Filter = e => (e as Book).Name.Contains(txt);
        }
    }
}