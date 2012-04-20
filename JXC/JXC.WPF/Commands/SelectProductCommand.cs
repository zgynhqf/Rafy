using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF.Command;
using OEA;
using OEA.Module.WPF;
using OEA.MetaModel.View;
using OEA.Library;
using JXC.WPF;
using OEA.Module.WPF.Controls;
using System.Windows;

namespace JXC.Commands
{
    public abstract class SelectProductCommand : LookupSelectAddCommand
    {
        public SelectProductCommand()
        {
            this.TargetEntityType = typeof(Product);
            this.RefProperty = ProductRefItem.ProductRefProperty;
            this.Template = new ProductSelectionUI();
        }

        protected CustomTemplate Template;

        protected override ControlResult GenerateSelectionUI()
        {
            if (this.Template == null) throw new ArgumentNullException("this.Template");
            return this.Template.CreateUI(this.TargetEntityType);
        }

        #region 选择商品后，直接定位到数量上。

        public override void Execute(ListObjectView view)
        {
            this._firstSelection = null;

            base.Execute(view);
        }

        private Entity _firstSelection;

        protected override Entity AddSelection(ListObjectView view, Entity selected)
        {
            var result = base.AddSelection(view, selected);

            this._firstSelection = this._firstSelection ?? result;

            return result;
        }

        protected override void Complete(ListObjectView view)
        {
            view.RefreshControl();

            view.Current = this._firstSelection;

            var treeGrid = view.Control.CastTo<MultiTypesTreeGrid>();
            var amountColumn = treeGrid.Columns.FindByProperty(ProductRefItem.AmountProperty);
            var row = treeGrid.GetRow(this._firstSelection);

            row.UpdateLayout();
            var cell = row.GetCell(amountColumn);
            treeGrid.TryEditCell(cell);
        }

        #endregion
    }

    public class ProductSelectionUI : CustomTemplate { }
}