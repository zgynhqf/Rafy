///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：???
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 ???
// * 分情况处理 SelectionDataGrid 的选择项变更事件 胡庆访 20110810
// * 
//*******************************************************/

//using System;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using OEA.Module.WPF.Controls;
//using OEA.Library;
//using System.Collections;

//namespace OEA.Module.WPF.Editors
//{
//    /// <summary>
//    /// 表格编辑器
//    /// 
//    /// 目前使用的控件是 OEA 中的 SelectionDataGrid
//    /// </summary>
//    public class GridEditor : SelectorEditor
//    {
//        public GridEditor(IListEditorContext context)
//            : base(context) { }

//        public new SelectionDataGrid Control
//        {
//            get { return base.Control as SelectionDataGrid; }
//        }

//        protected override void OnContextDataChanged()
//        {
//            base.OnContextDataChanged();

//            var grid = this.Control;

//            if (grid.Items.Count > 0)
//            {
//                var firstData = grid.Items[0];
//                //设置属性编辑器Visible
//                foreach (var col in grid.Columns)
//                {
//                    var column = col as OpenDataGridColumn;
//                    if (column != null)
//                    {
//                        column.UpdateVisibility(firstData);
//                    }
//                }
//            }

//            grid.IsReadOnly = this.IsReadOnly;
//        }

//        public override void RefreshControl()
//        {
//            try
//            {
//                this.SuppressEventReporting = true;

//                var oldCur = this.CurrentObject;

//                this.NotifyContextDataChanged();

//                if (oldCur != null) { this.CurrentObject = oldCur; }
//            }
//            finally
//            {
//                this.SuppressEventReporting = false;
//            }
//        }

//        public override IList SelectedObjects
//        {
//            get { return this.Control.SelectedObjects; }
//        }

//        public override CheckingMode CheckingMode
//        {
//            get { return this.Control.CheckingMode; }
//            set { this.Control.CheckingMode = value; }
//        }

//        #region 分情况处理 SelectionChanged 事件

//        protected override void OnSelectorChanged(Selector oldValue, Selector newValue)
//        {
//            base.OnSelectorChanged(oldValue, newValue);

//            var oldSDG = oldValue as SelectionDataGrid;
//            if (oldSDG != null)
//            {
//                oldSDG.CheckChanged -= On_SelectionDataGrid_CheckChanged;
//            }

//            var newSDG = newValue as SelectionDataGrid;
//            if (newSDG != null && this.Context.EventReporter != null)
//            {
//                newSDG.CheckChanged += On_SelectionDataGrid_CheckChanged;
//            }
//        }

//        /// <summary>
//        /// 如果在 CheckingRow 状态中，则 SelectionChanged 事件无意义，应该直接屏蔽。
//        /// 此时，CheckChanged 会自动生效，On_SelectionDataGrid_CheckChanged 事件起作用。
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        protected virtual void On_SelectionDataGrid_CheckChanged(object sender, CheckChangedEventArgs e)
//        {
//            if (this.SuppressEventReporting) return;

//            var newItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as Entity : null;
//            var oldItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as Entity : null;
//            var args = new SelectedEntityChangedEventArgs(newItem, oldItem);
//            this.Context.EventReporter.NotifySelectedItemChanged(sender, args);

//            e.Handled = true;
//        }

//        protected override void On_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            //如果在 CheckingRow 状态中，则 SelectionChanged 事件无意义，应该直接屏蔽。
//            //此时，CheckChanged 会自动生效，On_SelectionDataGrid_CheckChanged 事件起作用。
//            if (this.Control.CheckingMode != CheckingMode.CheckingRow)
//            {
//                base.On_Selector_SelectionChanged(sender, e);
//            }

//            //显示此行记录
//            if (e.AddedItems.Count > 0)
//            {
//                var item = e.AddedItems[0];

//                try
//                {
//                    var grid = this.Control;
//                    grid.UpdateLayout();
//                    grid.ScrollIntoView(item);
//                }
//                catch { }
//            }
//        }

//        #endregion
//    }
//}