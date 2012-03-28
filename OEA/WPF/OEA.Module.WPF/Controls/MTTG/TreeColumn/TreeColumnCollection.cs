using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    public class TreeColumnCollection : GridTreeViewColumnCollection
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (TreeColumn item in e.NewItems) { item.Owner = this.Owner.TreeGrid; }
            }

            base.OnCollectionChanged(e);
        }

        internal void UpdateVisibilities(object data)
        {
            foreach (TreeColumn column in this.ToArray()) { column.UpdateVisibility(data); }
            foreach (TreeColumn column in this.UnvisibleColumns.ToArray()) { column.UpdateVisibility(data); }
        }
    }
}