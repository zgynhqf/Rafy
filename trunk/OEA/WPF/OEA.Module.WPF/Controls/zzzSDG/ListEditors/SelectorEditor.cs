///*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110401
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100401
// * 
//*******************************************************/

//using System;
//using System.Collections;
//using System.ComponentModel;
//using System.Linq;
//using System.Windows.Controls;
//using System.Windows.Controls.Primitives;
//using System.Windows.Data;
//using System.Windows.Input;
//using OEA.Library;
//using OEA.MetaModel;
using OEA.MetaModel.View;
//using System.Windows;
//using OEA.Module.WPF.Controls;

//namespace OEA.Module.WPF.Editors
//{
//    /// <summary>
//    /// 此类可作为所有 WPF 多选编辑器的基类
//    /// </summary>
//    public abstract class SelectorEditor : ListEditor
//    {
//        public SelectorEditor(IListEditorContext context)
//            : base(context) { }

//        public new Selector Control
//        {
//            get { return base.Control as Selector; }
//        }

//        internal override void SetControl(FrameworkElement value)
//        {
//            var newSelector = value as Selector;
//            if (newSelector == null) throw new ArgumentException("value 必须继承自 Selector。");

//            var oldSelector = this.Control;

//            base.SetControl(newSelector);

//            this.OnSelectorChanged(oldSelector, newSelector);
//        }

//        /// <summary>
//        /// 当前选中的Object
//        /// </summary>
//        public override object CurrentObject
//        {
//            get
//            {
//                return this.Control != null ? this.Control.SelectedItem : null;
//            }
//            set
//            {
//                var selector = this.Control;

//                if (selector == null) { throw new InvalidOperationException("Control is null"); }

//                //如果value不在数据列表中，则忽略此次设置。
//                if (value != null &&
//                    selector.ItemsSource.OfType<object>().All(o => o != value)
//                    )
//                {
//                    return;
//                }

//                selector.SelectedItem = value;
//            }
//        }

//        public override IList SelectedObjects
//        {
//            get
//            {
//                var selector = this.Control;

//                var multiSelector = selector as MultiSelector;
//                if (multiSelector != null) return multiSelector.SelectedItems;

//                var listbox = selector as ListBox;
//                if (listbox != null) return listbox.SelectedItems;

//                return new object[] { selector.SelectedItem };
//            }
//        }

//        public override void SelectAll()
//        {
//            var selector = this.Control;

//            var multiSelector = selector as MultiSelector;
//            if (multiSelector != null)
//            {
//                multiSelector.SelectAll();
//                return;
//            }

//            var listbox = selector as ListBox;
//            if (listbox != null)
//            {
//                listbox.SelectAll();
//                return;
//            }

//            throw new NotSupportedException();
//        }

//        protected bool SuppressEventReporting { get; set; }

//        protected virtual void OnSelectorChanged(Selector oldValue, Selector newValue)
//        {
//            //如果之前已经手工为View设置了数据源，这里需要把数据进行同步。
//            this.OnContextDataChanged();

//            if (oldValue != null)
//            {
//                oldValue.SelectionChanged -= On_Selector_SelectionChanged;
//                oldValue.MouseDoubleClick -= On_Selector_MouseDoubleClick;
//            }

//            if (this.Context.EventReporter != null)
//            {
//                //触发View的SelectionChanged和MouseDoubleClick事件
//                newValue.SelectionChanged += On_Selector_SelectionChanged;
//                newValue.MouseDoubleClick += On_Selector_MouseDoubleClick;
//            }

//            ////http://127.0.0.1:47873/help/0-4028/ms.help?method=page&id=B792C740-CF2B-4DA8-8BA8-3D2E5A821874&product=VS&productVersion=100&topicVersion=100&locale=EN-US&topicLocale=EN-US&embedded=true
//            ////How to: Improve the Performance of a TreeView
//            //VirtualizingStackPanel.SetIsVirtualizing(this._grid, true);
//            //VirtualizingStackPanel.SetVirtualizationMode(this._grid, VirtualizationMode.Recycling);
//        }

//        protected virtual void On_Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
//        {
//            if (this.SuppressEventReporting) return;

//            var newItem = e.AddedItems.Count > 0 ? e.AddedItems[0] as Entity : null;
//            var oldItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] as Entity : null;
//            var args = new SelectedEntityChangedEventArgs(newItem, oldItem);
//            this.Context.EventReporter.NotifySelectedItemChanged(sender, args);

//            e.Handled = true;
//        }

//        protected virtual void On_Selector_MouseDoubleClick(object sender, MouseButtonEventArgs e)
//        {
//            if (this.SuppressEventReporting) return;

//            this.Context.EventReporter.NotifyMouseDoubleClick(sender, e);
//        }

//        protected override void OnContextDataChanged()
//        {
//            //如果手工设置了数据源，会到达这步，而这时控件可能还没有生成。
//            var selector = this.Control;
//            if (selector == null) return;

//            var viewData = this.Context.Data;

//            if (viewData == null)
//            {
//                selector.ItemsSource = null;
//                return;
//            }

//            ICollectionView collectionView = null;

//            var groups = this.Context.EntityViewInfo.GroupDescriptions;

//            //是否需要过滤视图的数据
//            var hasFilter = viewData as IHasViewFilter;
//            var supportGrouping = groups != null && groups.Count > 0;
//            if (hasFilter != null)
//            {
//                collectionView = SortableList.CreateViewForEntityList(viewData, hasFilter.ViewModelFilter, supportGrouping);
//            }
//            else
//            {
//                collectionView = SortableList.CreateViewForEntityList(viewData, supportGrouping);
//            }

//            selector.GroupStyle.Clear();

//            // && viewData.OfType<object>().Count() <= 400)//如果数据量大于400条，则不分组
//            if (collectionView != null && viewData != null)
//            {
//                if (groups != null && groups.Count > 0)
//                {
//                    foreach (string propertyName in groups)
//                    {
//                        collectionView.GroupDescriptions.Add(new PropertyGroupDescription(propertyName));
//                    }

//                    ResourcesHelper.SetGroupStyle(selector);
//                }
//            }

//            selector.UpdateLayout();   //强制等待更新，执行dg.CurrentItem赋值会空引用错误

//            collectionView.MoveCurrentTo(null);  //默认不选择记录
//            selector.ItemsSource = collectionView;
//        }
//    }
//}
