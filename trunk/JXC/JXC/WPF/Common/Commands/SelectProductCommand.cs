using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Module.WPF;
using OEA.MetaModel.View;
using OEA.Library;
using JXC.WPF;
using OEA.Module.WPF.Controls;
using System.Windows;
using OEA.WPF.Command;
using OEA.MetaModel.Attributes;

namespace JXC.Commands
{
    [Command(Label = "选择商品", GroupType = CommandGroupType.Edit)]
    public class SelectProductCommand : LookupSelectAddCommand
    {
        public SelectProductCommand()
        {
            this.TargetEntityType = typeof(Product);
            this.RefProperty = ProductRefItem.ProductRefProperty;
            this.Template = new UITemplate();

            //选择商品界面不需要显示附件
            this.Template.BlocksDefined += (o, e) =>
            {
                e.Blocks.Children.Clear();
            };
        }

        protected UITemplate Template;

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

            (result as ProductRefItem).Amount = 1;

            return result;
        }

        protected override void Complete(ListObjectView view)
        {
            if (this._firstSelection != null)
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
        }

        #endregion
    }
}