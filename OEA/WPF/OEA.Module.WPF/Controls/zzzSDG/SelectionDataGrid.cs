///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110810
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.1.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100510
// * 使用 IsChecked 附加属性来实现 CheckBox 多选。 胡庆访 20110810
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using System.Windows.Input;
//using System.Windows.Data;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Controls;
//using OEA.Module.WPF.Controls;
//using OEA.MetaModel;
using OEA.MetaModel.View;
//using System.Diagnostics;
//using OEA.Module.WPF;
//using System.Collections;
//using System.Collections.ObjectModel;
//using System.Collections.Specialized;
//using System.Windows.Controls.Primitives;

//namespace OEA.Module.WPF.Controls
//{
//    /// <summary>
//    /// 一个可以使用 CheckBox 进行多选的 DataGrid
//    /// 
//    /// 点击 DataGridCell 中的任意地点都会引发 DataGrid 自己处理整行的 IsSelected 状态，导致其它行处于非选择状态。
//    /// 所以这里不再使用 DataGrid.SelectionChanged 事件，即：
//    ///     打开选择功能的表格后，DataGrid.SelecteItems 及 SelectionChanged 事件都不再可用，
//    ///     而是应该使用本对象定义的 SelectedObjects 及 CheckChanged 事件。
//    /// 
//    /// 实现方案：
//    /// 一、重点关注以下三个绑定/消息通知：
//    ///     1. CheckBox 的 IsChecked 属性和 DataGridRow 的附加属性 (SelectionDataGrid.IsChecked) 使用 Binding 进行双向绑定。
//    ///     2. DataGridRow.(SelectionDataGrid.IsChecked) 和一个表示当前所有选中项的私有变量 _selectedObjects 使用代码进行双向绑定：
//    ///         IsChecked 到 _selectedObjects 使用附加属性的 PropertyChanged 事件进行同步。
//    ///         _selectedObjects 到 IsChecked 使用集合的 CollectionChanged 事件进行同步。
//    ///     3. 最后，_selectedObjects 集合在变化时，转换为一个 CheckChanged 事件用于通知外部。
//    /// 二、整个过程完全抛弃 DataGrid.SelectionChanged,DataGridRow.IsSelected 等机制。（原因如上所说，无法抑制 DataGridCell 的设置选择行为。）
//    /// 三、选择附加属性来作为 CheckBox.IsChecked 属性和 _selectedObjects 集合之间双向绑定的中间者，原因在于：
//    ///     1. CheckBox 的事件监听不好控制，两个属性双向绑定后，直接在附加属性的变更回调中写代码更加易读。
//    ///     2. 控件外部可以随时通过获取/设置某一行的此附加属性来控制
//    /// </summary>
//    public class SelectionDataGrid : DataGrid, ISelectableListControl
//    {
//        #region SelectionCheckBoxStyle

//        private static Style _selectionCheckBoxStyle;

//        private static Style SelectionCheckBoxStyle
//        {
//            get
//            {
//                //以下代码类似于 DataGridCheckBoxColumn 中的 DefaultElementStyle。
//                if (_selectionCheckBoxStyle == null)
//                {
//                    var style = new Style(typeof(CheckBox), Application.Current.TryFindResource(typeof(CheckBox)) as Style);

//                    //显式设置 Checkbox 的测试点击状态为 true。
//                    style.Setters.Add(new Setter(UIElement.IsHitTestVisibleProperty, true));
//                    style.Setters.Add(new Setter(UIElement.FocusableProperty, true));

//                    style.Seal();

//                    _selectionCheckBoxStyle = style;
//                }
//                return _selectionCheckBoxStyle;
//            }
//        }

//        #endregion

//        #region DataGridRow.DataGridOwnerProperty

//        private static readonly DependencyProperty DataGridOwnerProperty = DependencyProperty.RegisterAttached(
//            "DataGridOwner", typeof(SelectionDataGrid), typeof(SelectionDataGrid),
//            new PropertyMetadata(DataGridOwnerPropertyChanged)
//            );

//        /// <summary>
//        /// 获取某行所属的表格控件
//        /// </summary>
//        /// <param name="element"></param>
//        /// <returns></returns>
//        public static SelectionDataGrid GetDataGridOwner(DataGridRow element)
//        {
//            return (SelectionDataGrid)element.GetValue(DataGridOwnerProperty);
//        }

//        /// <summary>
//        /// 设置某行所属的表格控件
//        /// </summary>
//        /// <param name="element"></param>
//        /// <param name="value"></param>
//        private static void SetDataGridOwner(DataGridRow element, SelectionDataGrid value)
//        {
//            element.SetValue(DataGridOwnerProperty, value);
//        }

//        private static void DataGridOwnerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var value = (SelectionDataGrid)e.NewValue;
//        }

//        #endregion

//        #region DataGridRow.IsCheckedProperty

//        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached(
//            "IsChecked", typeof(bool), typeof(SelectionDataGrid),
//            new PropertyMetadata(IsCheckedPropertyChanged)
//            );

//        /// <summary>
//        /// 获取某行的选中状态
//        /// </summary>
//        /// <param name="element"></param>
//        /// <returns></returns>
//        public static bool GetIsChecked(DataGridRow element)
//        {
//            return (bool)element.GetValue(IsCheckedProperty);
//        }

//        /// <summary>
//        /// 设置某行的选中状态
//        /// </summary>
//        /// <param name="element"></param>
//        /// <param name="value"></param>
//        public static void SetIsChecked(DataGridRow element, bool value)
//        {
//            element.SetValue(IsCheckedProperty, value);
//        }

//        private static void IsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var value = (bool)e.NewValue;

//            var row = d as DataGridRow;

//            var @this = GetDataGridOwner(row);
//            if (@this.CheckingMode != CheckingMode.CheckingRow)
//            {
//                throw new NotSupportedException("只能 CheckingRow 模式下已经打开选择功能的 SelectionDataGrid 才能设置此属性！");
//            }

//            if (value)
//            {
//                @this._selectedObjects.Add(row.Item);
//            }
//            else
//            {
//                @this._selectedObjects.Remove(row.Item);
//            }
//        }

//        #endregion

//        #region CheckingModeProperty

//        public static readonly DependencyProperty CheckingModeProperty = DependencyProperty.Register(
//            "CheckingMode", typeof(CheckingMode), typeof(SelectionDataGrid),
//            new PropertyMetadata(CheckingMode.None, CheckingModePropertyChanged)
//            );

//        /// <summary>
//        /// “Check选择” 模式
//        /// </summary>
//        public CheckingMode CheckingMode
//        {
//            get { return (CheckingMode)this.GetValue(CheckingModeProperty); }
//            set { this.SetValue(CheckingModeProperty, value); }
//        }

//        private static void CheckingModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            var grid = d as SelectionDataGrid;
//            var value = (CheckingMode)e.NewValue;
//            if (value != CheckingMode.None)
//            {
//                if (grid._selectionColumn == null)
//                {
//                    grid.EnableSelect();
//                }

//                grid.ResetBindingByMode();
//            }
//            else
//            {
//                grid.DisableSelect();
//            }
//        }

//        #endregion

//        #region 选择模式的切换

//        private SelectedItemsCollection _selectedObjects;

//        private DataGridCheckBoxColumn _selectionColumn;

//        /// <summary>
//        /// 在 CheckingMode 被打开的模式下，用于选择的 CheckBox 列。
//        /// </summary>
//        public DataGridCheckBoxColumn SelectionColumn
//        {
//            get { return this._selectionColumn; }
//        }

//        /// <summary>
//        /// 当前已经选择的对象列表
//        /// </summary>
//        public IList SelectedObjects
//        {
//            get
//            {
//                if (this.CheckingMode == CheckingMode.CheckingRow) { return this._selectedObjects; }

//                return base.SelectedItems;
//            }
//        }

//        private void EnableSelect()
//        {
//            this._selectionColumn = new DataGridCheckBoxColumn()
//            {
//                Header = "选择",
//                EditingElementStyle = SelectionCheckBoxStyle.BasedOn,
//                ElementStyle = SelectionCheckBoxStyle
//            };
//            this.Columns.Insert(0, this._selectionColumn);

//            this._selectedObjects = new SelectedItemsCollection(this);
//            this._selectedObjects.CollectionChanged += On_SelectedItems_CollectionChanged;
//        }

//        private void DisableSelect()
//        {
//            if (this._selectionColumn != null)
//            {
//                this.Columns.Remove(this._selectionColumn);

//                this._selectionColumn = null;
//                this._selectedObjects = null;
//                this._selectedObjects.CollectionChanged -= On_SelectedItems_CollectionChanged;
//            }
//        }

//        #endregion

//        #region CheckChangedEvent

//        private static RoutedEvent CheckChangedEvent = EventManager.RegisterRoutedEvent(
//            "CheckChanged", RoutingStrategy.Bubble, typeof(CheckChangedEventHandler), typeof(SelectionDataGrid)
//            );

//        /// <summary>
//        /// 某一项被选择或者反选时发生此事件
//        /// </summary>
//        public event CheckChangedEventHandler CheckChanged
//        {
//            add { this.AddHandler(CheckChangedEvent, value); }
//            remove { this.RemoveHandler(CheckChangedEvent, value); }
//        }

//        #endregion

//        protected override void OnLoadingRow(DataGridRowEventArgs e)
//        {
//            base.OnLoadingRow(e);

//            var row = e.Row;

//            //在行中记录父控件指针
//            SetDataGridOwner(row, this);

//            //初始化选中状态。
//            if (this.CheckingMode == CheckingMode.CheckingRow)
//            {
//                if (this._selectedObjects.Contains(row.Item)) { SetIsChecked(row, true); }
//            }
//        }

//        /// <summary>
//        /// 根据绑定模式不同使用不同的绑定
//        /// </summary>
//        private void ResetBindingByMode()
//        {
//            var mode = this.CheckingMode;
//            if (mode != CheckingMode.None)
//            {
//                //两种不同的模式
//                if (mode == CheckingMode.CheckingViewModel)
//                {
//                    this.SelectionUnit = DataGridSelectionUnit.FullRow;
//                    this._selectionColumn.Binding = new Binding(PropertyConvention.IsSelected);
//                }
//                else
//                {
//                    this.SelectionUnit = DataGridSelectionUnit.Cell;
//                    this._selectionColumn.Binding = new Binding
//                    {
//                        Path = new PropertyPath(SelectionDataGrid.IsCheckedProperty),
//                        RelativeSource = new RelativeSource
//                        {
//                            Mode = RelativeSourceMode.FindAncestor,
//                            AncestorType = typeof(DataGridRow)
//                        }
//                    };
//                }
//            }
//        }

//        /// <summary>
//        /// 两个职责：
//        /// 1. 同步到 DataGridRow.(SelectionDataGrid.IsChecked) 属性上；
//        /// 2. 引发 CheckChanged 事件通知外部。
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void On_SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {
//            //同步控件的状态
//            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
//            {
//                if (e.Action == NotifyCollectionChangedAction.Reset)
//                {
//                    this.CheckItems(this.ItemsSource, false);
//                }
//                else
//                {
//                    this.CheckItems(e.NewItems, true);
//                    this.CheckItems(e.OldItems, false);
//                }
//            }

//            //Bubble Event
//            this.RaiseEvent(new CheckChangedEventArgs()
//            {
//                RoutedEvent = CheckChangedEvent,
//                AddedItems = e.NewItems == null ? new List<object>() : e.NewItems,
//                RemovedItems = e.OldItems == null ? new List<object>() : e.OldItems,
//            });
//        }

//        private void CheckItems(IEnumerable list, bool isChecked)
//        {
//            if (list != null)
//            {
//                foreach (var newItem in list)
//                {
//                    var row = this.ItemContainerGenerator.ContainerFromItem(newItem) as DataGridRow;
//                    if (row != null) SetIsChecked(row, isChecked);
//                }
//            }
//        }

//        #region private class SelectedItemsCollection

//        /// <summary>
//        /// 一个不会添加重复数据的集合
//        /// </summary>
//        private class SelectedItemsCollection : ObservableCollection<object>
//        {
//            private ItemsControl _owner;

//            public SelectedItemsCollection(ItemsControl owner)
//            {
//                this._owner = owner;
//            }

//            protected override void InsertItem(int index, object item)
//            {
//                if (this.CanAdd(item)) { base.InsertItem(index, item); }
//            }

//            protected override void SetItem(int index, object item)
//            {
//                if (this.CanAdd(item)) { base.SetItem(index, item); }
//            }

//            private bool CanAdd(object item)
//            {
//                return !this.Contains(item) &&
//                    this._owner.ItemsSource.Cast<object>().Contains(item);
//            }
//        }

//        #endregion

//        #region 临时代码比较复杂，有空应该整理下

//        /// <summary>
//        /// 当前正在编辑的行中的单元
//        /// </summary>
//        private DataGridCell _editingCellInRow;

//        ///// <summary>
//        ///// 选择的时候，直接开始编辑。
//        ///// 并且如果是checkbox时，则改变选中状态。
//        ///// </summary>
//        ///// <param name="e"></param>
//        //protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
//        //{
//        //    base.OnSelectedCellsChanged(e);

//        //    //如果是checkbox，直接换值
//        //    var column = this.CurrentColumn as DataGridCheckBoxColumn;
//        //    if ((column != null) && (null != CurrentItem) && (null != column.Binding))
//        //    {
//        //        var propertyName = (column.Binding as Binding).Path.Path;
//        //        var currentItem = this.CurrentItem;
//        //        var oldValue = currentItem.GetPropertyValue(propertyName);
//        //        var newValue = !(bool)oldValue;
//        //        currentItem.SetPropertyValue(propertyName, newValue);
//        //    }
//        //}

//        protected override void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
//        {
//            base.OnCellEditEnding(e);

//            Debug.WriteLine("End Editing");
//            //选择另一行，把_editingCellInRow清空
//            //因为MS会自动把这行Commit
//            this._editingCellInRow = null;
//            //this._editingControl = null;
//            Debug.WriteLine("Changd another row, MS DataGrid control committed the old row automatically.");
//        }

//        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
//        {
//            #region Debug

//            //Debug.WriteLine("========================= Start =========================" + DateTime.Now.Ticks);
//            //var oldCelll = e.OldFocus as DataGridCell;
//            //var newCell = e.NewFocus as DataGridCell;

//            //if (e.OldFocus != null)
//            //{
//            //    Debug.WriteLine("OldFocus:" + e.OldFocus.GetType().Name);
//            //}
//            //if (oldCelll != null)
//            //{
//            //    Debug.WriteLine(oldCelll.IsEditing);
//            //}
//            //if (e.NewFocus != null)
//            //{
//            //    Debug.WriteLine("NewFocus:" + e.NewFocus.GetType().Name);
//            //}
//            //if (newCell != null)
//            //{
//            //    Debug.WriteLine(newCell.IsEditing);
//            //}
//            //Debug.WriteLine("========================= End =========================");

//            #endregion

//            base.OnLostKeyboardFocus(e);

//            //避免在第一次存储_editingCell的时候就调用Commit
//            bool firstTime = false;

//            #region 在进入编辑状态时执行。

//            //如果是DataGridCell，并且正在编辑状态，
//            //则这时候焦点就是从DataGridCell到TextBox等编辑控件移动的过程中。
//            var oldCell = e.OldFocus as DataGridCell;
//            if (oldCell != null && oldCell.IsEditing)
//            {
//                ////目前只有专门为下拉框进行特殊处理了。
//                //if ((e.NewFocus is LookupListPropertyEditorControl) == false)
//                //{
//                this._editingCellInRow = oldCell;
//                //this._editingControl = e.NewFocus as Visual;//可能并不是Visual，所以有可能为null
//                firstTime = true;
//                Debug.WriteLine("Start editing a cell in a row.");
//                //}
//            }

//            #endregion

//            if (firstTime == false)
//            {
//                #region 以下判断当编辑完成后，失去焦点时执行。

//                //是否提交
//                bool commit = false;

//                //计算是还需要commit
//                //如果正在编辑
//                if (this._editingCellInRow != null)
//                {
//                    //如果是聚焦自己的空白区域，可以提交。
//                    if (this == e.NewFocus)
//                    {
//                        commit = true;
//                    }

//                    if (e.NewFocus != null)
//                    {
//                        var newFocus = e.NewFocus as DependencyObject;
//                        //如果焦点已经移出这个DataGrid，则也需要提交。
//                        //不过这里需要加上额外的PopupRoot的判断，因为弹出的下拉框，它也不是DataGrid的逻辑子节点
//                        var cell = newFocus.GetVisualParent<DataGridCell>();
//                        if (cell == null || this.Columns.All(c => c != cell.Column))
//                        {
//                            //弹出框里面的元素的逻辑根节点，是一个Internal的类：PopupRoot
//                            var root = newFocus.GetVisualRoot();
//                            if (root.GetType().Name != "PopupRoot")
//                            {
//                                commit = true;
//                            }
//                        }
//                    }

//                    //var control = e.NewFocus as DependencyObject;
//                    //如果control不是grid中的控件，则可能需要commit
//                    //if (control != null && this.IsAncestorOf(control) == false)
//                    //{
//                    //下拉选择的控件里面，如果还有DataGrid的话，
//                    //上面的两个判断也会通过（不知道为什么编辑控件里面的控件不是Grid的Desendant。），但是这时候也不能进行commit操作。

//                    //这里是为LookupListPropertyEditorControl中的子控件写的特定的代码，可能会发生bug：
//                    //当用户在这个grid编辑时，直接点击其它的grid中的DataGridCell或Button，这时候commit就不会发生，EditLevel就会出错不匹配。
//                    //if ((control is Button) == false &&
//                    //    (control is DataGridCell) == false
//                    //    )
//                    //{
//                    //    commit = true;
//                    //}
//                    //}
//                }

//                if (commit)
//                {
//                    Debug.WriteLine("Commit editing cell.");
//                    this.CommitEdit(DataGridEditingUnit.Row, true);
//                    this._editingCellInRow = null;
//                }

//                #endregion
//            }
//        }

//        //protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
//        //{
//        //    #region Debug

//        //    //Debug.WriteLine("Gottinggggggggggggggggggggggggggggggggggggggggggggggggg Begin");
//        //    //var oldCell = e.OldFocus as DataGridCell;
//        //    //var newCell = e.NewFocus as DataGridCell;

//        //    //if (e.OldFocus != null)
//        //    //{
//        //    //    Debug.WriteLine("OldFocus:" + e.OldFocus.GetType().FullName);
//        //    //}
//        //    //if (oldCell != null)
//        //    //{
//        //    //    Debug.WriteLine(oldCell.IsEditing);
//        //    //}
//        //    //if (e.NewFocus != null)
//        //    //{
//        //    //    Debug.WriteLine("NewFocus:" + e.NewFocus.GetType().FullName);
//        //    //}
//        //    //if (newCell != null)
//        //    //{
//        //    //    Debug.WriteLine(newCell.IsEditing);
//        //    //}

//        //    //Debug.WriteLine("Gottinggggggggggggggggggggggggggggggggggggggggggggggggg End"); 

//        //    #endregion

//        //    base.OnGotKeyboardFocus(e);
//        //} 

//        #endregion
//    }

//    /// <summary>
//    /// 选择项变更事件处理类型
//    /// </summary>
//    /// <param name="sender"></param>
//    /// <param name="e"></param>
//    public delegate void CheckChangedEventHandler(object sender, CheckChangedEventArgs e);

//    /// <summary>
//    /// 选择项变更事件参数
//    /// </summary>
//    public class CheckChangedEventArgs : RoutedEventArgs
//    {
//        /// <summary>
//        /// 新选择的数据项
//        /// </summary>
//        public IList AddedItems { get; internal set; }

//        /// <summary>
//        /// 取消选择的数据项
//        /// </summary>
//        public IList RemovedItems { get; internal set; }
//    }
//}

/////// <summary>
/////// ms-help://MS.VSCC.v90/MS.MSDNQTR.v90.en/wpf_conceptual/html/b1a64b61-14be-4d75-b89a-5c67bebb2c7b.htm
/////// Hit Testing in the Visual Layer
/////// </summary>
////public class GridCheckBoxColumn : DataGridCheckBoxColumn
////{
////    protected override void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
////    {
////        base.CancelCellEdit(editingElement, uneditedValue);
////    }
////    protected override bool CommitCellEdit(FrameworkElement editingElement)
////    {
////        var result = base.CommitCellEdit(editingElement);
////        return result;
////    }
////    protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
////    {
////        var result = base.GenerateEditingElement(cell, dataItem);
////        return result;
////    }
////    protected override void OnBindingChanged(BindingBase oldBinding, BindingBase newBinding)
////    {
////        base.OnBindingChanged(oldBinding, newBinding);
////    }
////    protected override object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
////    {
////        var result = base.PrepareCellForEdit(editingElement, editingEventArgs);
////        return result;
////    }
////    protected override bool OnCoerceIsReadOnly(bool baseValue)
////    {
////        var result = base.OnCoerceIsReadOnly(baseValue);
////        result = true;
////        return result;
////    }
////    //protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
////    //{
////    //    CheckBox checkBox = (cell != null) ? (cell.Content as CheckBox) : null;
////    //    if (checkBox == null)
////    //    {
////    //        checkBox = new CheckBox();
////    //    }

////    //    checkBox.IsThreeState = IsThreeState;

////    //    var style = new Style();
////    //    //style.Setters.Add(this.ElementStyle.Setters[0]);
////    //    style.Setters.Add(this.ElementStyle.Setters[1]);
////    //    style.Setters.Add(this.ElementStyle.Setters[2]);
////    //    style.Setters.Add(this.ElementStyle.Setters[3]);
////    //    checkBox.Style = style;

////    //    BindingBase binding = Binding;
////    //    if (binding != null)
////    //    {
////    //        BindingOperations.SetBinding(checkBox, CheckBox.IsCheckedProperty, binding);
////    //    }
////    //    else
////    //    {
////    //        BindingOperations.ClearBinding(checkBox, CheckBox.IsCheckedProperty);
////    //    }

////    //    return checkBox;
////    //}
////}