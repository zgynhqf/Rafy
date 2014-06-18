using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using JXC.WPF;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Command;
using Rafy.WPF.Controls;

namespace JXC.Commands
{
    [Command(Label = "选择商品", GroupType = CommandGroupType.Edit)]
    public class SelectProductCommand : LookupSelectAddCommand
    {
        public SelectProductCommand()
        {
            this.RefProperty = ProductRefItem.ProductIdProperty;
            this.Template = new ProductModule();

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
            return this.Template.CreateUI();
        }

        #region 选择商品后，直接定位到数量上。

        public override void Execute(ListLogicalView view)
        {
            this._firstSelection = null;

            base.Execute(view);
        }

        private Entity _firstSelection;

        protected override Entity AddSelection(ListLogicalView view, Entity selected)
        {
            var result = base.AddSelection(view, selected);

            this._firstSelection = this._firstSelection ?? result;

            (result as ProductRefItem).Amount = 1;

            return result;
        }

        protected override void Complete(ListLogicalView view)
        {
            if (this._firstSelection != null)
            {
                view.RefreshControl();

                view.Current = this._firstSelection;

                var treeGrid = view.Control.CastTo<RafyTreeGrid>();
                var amountColumn = treeGrid.FindColumnByProperty(ProductRefItem.AmountProperty);

                var row = treeGrid.FindRow(this._firstSelection);
                if (row != null)
                {
                    var cell = row.ScrollToCell(amountColumn);
                    treeGrid.TryEditRow(row, cell);
                }
            }
        }

        #endregion
    }
}