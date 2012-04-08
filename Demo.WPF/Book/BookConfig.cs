using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;
using Demo.WPF.Commands;

namespace Demo.WPF
{
    class BookWPFConfig : EntityConfig<Book>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.UseWPFCommands(typeof(BookSearchCommand));
            View.DetailPanelType = typeof(BookDetailPanel);
            View.DetailLabelWidth = 120;
        }
    }
}