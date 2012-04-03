using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OEA.Command;
using OEA.Editors;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Audit;
using OEA.Module.WPF;
using OEA.Module.WPF.Controls;

namespace OEA.WPF.Command
{
    public abstract class ClientCommand<TParamater> : ClientCommand
        where TParamater : class
    {
        protected override bool CanExecuteCore(object param) { return this.CanExecute(param as TParamater); }

        protected override void ExecuteCore(object param) { this.Execute(param as TParamater); }

        public virtual bool CanExecute(TParamater view) { return true; }

        public abstract void Execute(TParamater view);
    }

    public abstract class ViewCommand : ClientCommand<ObjectView> { }

    public abstract class ListViewCommand : ClientCommand<ListObjectView>
    {
        protected static bool AnySelected(ListObjectView view)
        {
            return view.Current != null;
        }

        protected static bool HasData(ListObjectView view)
        {
            return view.Data != null;
        }
    }
}