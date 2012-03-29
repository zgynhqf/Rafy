/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Automation;
using OEA.Module.WPF.Controls;
using OEA.Utils;
using SimpleCsla;
using SimpleCsla.Core;
using SimpleCsla.Wpf;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 列表选择属性编辑器
    /// 
    /// 此类使用 ListObjectView 作为实现的核心，构造了一个 ComboListControl 容器来呈现 ListObjectView 中的 Control。
    /// </summary>
    public class LookupListPropertyEditor : WPFPropertyEditor
    {
        #region 私有字段 与 构造函数

        private ComboListControl _cmbList;

        private IListObjectView _listView;

        protected LookupListPropertyEditor() { }

        #endregion

        #region 公共属性

        /// <summary>
        /// 手动设置下拉的数据源
        /// </summary>
        public EntityList DataSourse
        {
            get { return this._listView.Data; }
            set { this._listView.Data = value; }
        }

        #endregion

        #region 事件

        /// <summary>
        /// 属性变化前发生此事件。
        /// </summary>
        public event EventHandler ReferenceEntityChanging;

        private void OnReferenceEntityChanging()
        {
            var handler = this.ReferenceEntityChanging;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region 控件生成

        /// <summary>
        /// 生成等待控件
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement CreateLabelElement()
        {
            var panel = base.CreateLabelElement() as Panel;

            //生成busy控件
            var loader = this._listView.DataLoader as ViewDataLoader;
            var busy = AutoUIHelper.CreateBusyControl(loader);
            panel.Children.Add(busy);

            return panel;
        }

        /// <summary>
        /// 生成下拉框控件
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement CreateEditingElement()
        {
            var refTypeMeta = this.PropertyViewInfo.ReferenceViewInfo.RefTypeDefaultView;

            //创建 ListObjectView
            this._listView = AutoUI.ViewFactory.CreateListObjectView(refTypeMeta, true);
            if (this.IsMultiSelection) { this._listView.CheckingMode = CheckingMode.CheckingRow; }
            this._listView.CurrentObjectChanged += On_ListView_CurrentObjectChanged;

            //创建 ComboListControl : 使用下拉控件显示
            this._cmbList = new ComboListControl(this._listView, this)
            {
                Name = this.PropertyViewInfo.Name
            };
            this._cmbList.DropDownOpened += On_ComboDataGrid_DropDownOpened;
            this._cmbList.TextPath = refTypeMeta.TitleProperty.Name;

            this.BindElementReadOnly(this._cmbList, ComboBox.IsReadOnlyProperty);

            this.ResetBinding(this._cmbList);

            //自动化
            this.SetAutomationElement(this._cmbList);

            return this._cmbList;
        }

        /// <summary>
        /// 由于直接对ComboBox.Text直接赋值，导致TextBinding Detach，所以需要手动新增绑定
        /// </summary>
        protected override void ResetBinding(FrameworkElement editingControl)
        {
            //设置绑定模式，不允许从Text修改到原对象（bug #18098）
            var bindingMode = BindingMode.OneWay;
            var bindingPath = string.Empty;

            var refInfo = this.PropertyViewInfo.ReferenceViewInfo;
            if (string.IsNullOrEmpty(refInfo.RefEntityProperty))
            {
                bindingPath = this.PropertyViewInfo.Name;
                if (this.PropertyCanWrite) { bindingMode = BindingMode.TwoWay; }
            }
            else
            {
                bindingPath = refInfo.TitlePath();
            }

            editingControl.SetBinding(ComboBox.TextProperty, new Binding()
            {
                Mode = bindingMode,
                Path = new PropertyPath(bindingPath)
            });
        }

        #endregion

        #region 处理并适配 内部组合对象 中的事件。

        /// <summary>
        /// 表示用户点击界面引起此事件。
        /// 如果不是，则表示 框架其它类 引发的选择事件。
        /// 
        /// http://ipm.grandsoft.com.cn/issues/243370
        /// </summary>
        private bool SelectedByUser
        {
            get { return this._cmbList.SelectedByUser; }
            set { this._cmbList.SelectedByUser = value; }
        }

        private void On_ListView_CurrentObjectChanged(object sender, EventArgs e)
        {
            using (this.TryEnterProcess(ProcessSource.ListViewCurrentObjectChanged))
            {
                if (this.CurrentProcess == ProcessSource.ListViewCurrentObjectChanged) { this.SyncSelectionToValue(); }
            }
        }

        private void On_ComboDataGrid_DropDownOpened(object sender, EventArgs e)
        {
            if (this.IsReadonly)
            {
                this._cmbList.IsDropDownOpen = false;
                return;
            }

            if (this._cmbList.CurrentProgress != CLCProgress.LazyFiltering)
            {
                //控件被下拉时，自动加载数据，并定位到当前对象。
                using (this.TryEnterProcess(ProcessSource.DropdownOpend))
                {
                    this.EnsureDataLoaded(true);

                    this.SyncValueToSelection();
                }
            }
        }

        /// <summary>
        /// 在设置值时，更新下拉框中的值
        /// </summary>
        protected override void OnPropertyValueChanged()
        {
            using (this.TryEnterProcess(ProcessSource.PropertyValueChanged))
            {
                base.OnPropertyValueChanged();

                //如果当前流程是由 CurrentObject 改变引起的，所以不需要再对它进行定位了。
                if (this.CurrentProcess == ProcessSource.PropertyValueChanged) { this.SyncValueToSelection(); }
            }
        }

        #endregion

        #region 同步 Value 和 Selection

        private const string MULTI_SELECTION_SPLITTER = ";";

        /// <summary>
        /// 根据当前的值（PropertyValue），找到并定位到当前对象
        /// </summary>
        private void SyncValueToSelection()
        {
            var items = this._listView.Data;
            if (items == null || items.Count == 0) return;

            //找到值对应的数据项
            var selectedItems = new List<Entity>();
            if (this.Context.CurrentObject != null && this.PropertyViewInfo != null)
            {
                //根据设置的 SelectedValuePath 来对比属性值 this.PropertyValue，如果相同，则找到对应的CurrentObject
                var targetValue = this.PropertyValue;
                if (this.IsMultiSelection && targetValue != null)
                {
                    var values = targetValue.ToString().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var selectedValue in values)
                    {
                        AddToListBySelectedValue(items, selectedItems, selectedValue);
                    }
                }
                else
                {
                    AddToListBySelectedValue(items, selectedItems, targetValue);
                }
            }

            //定位 SelectedObjects
            try
            {
                this.SelectedByUser = false;

                var selectedObjects = this._listView.SelectedObjects;

                selectedObjects.Clear();

                foreach (var item in selectedItems) { selectedObjects.Add(item); }
            }
            finally
            {
                this.SelectedByUser = true;
            }
        }

        private void AddToListBySelectedValue(IList items, List<Entity> selectedItems, object targetValue)
        {
            var selectedItem = items.OfType<Entity>().FirstOrDefault(item =>
                 object.Equals(targetValue, this.GetSelectedValue(item))
                 );
            if (selectedItem != null) selectedItems.Add(selectedItem);
        }

        /// <summary>
        /// 根据选中的对象设置CurrentObject相关的值。
        /// </summary>
        private void SyncSelectionToValue()
        {
            if (this.IsMultiSelection)
            {
                var result = string.Empty;

                foreach (Entity item in this._listView.SelectedObjects)
                {
                    if (result.Length == 0)
                        result += this.GetSelectedValue(item);
                    else
                        result += MULTI_SELECTION_SPLITTER + this.GetSelectedValue(item);
                }

                //赋值给this.PropertyValue
                this.PropertyValue = result;
            }
            else
            {
                var selecteEntity = this._listView.Current as Entity;
                if (selecteEntity != null)
                {
                    var curObj = this.Context.CurrentObject;
                    if (curObj != null)
                    {
                        //设置 View.CurrentObject 指定的 refProperty 属性为选中的对象
                        var refProperty = this.PropertyViewInfo.ReferenceViewInfo.RefEntityProperty;
                        if (!string.IsNullOrEmpty(refProperty))
                        {
                            this.OnReferenceEntityChanging();
                            curObj.SetPropertyValue(refProperty, selecteEntity);
                        }
                    }

                    //赋值给this.PropertyValue
                    this.PropertyValue = this.GetSelectedValue(selecteEntity);
                }
            }
        }

        #endregion

        #region Load Data

        private void SelectFirstObject()
        {
            var items = this._listView.Data;
            if (items != null && items.Count > 0)
            {
                this._listView.Current = items[0] as Entity;
            }
        }

        /// <summary>
        /// 从指定的属性中获取数据列表。
        /// </summary>
        private void RefreshPropertyDataSource()
        {
            //如果设定了DataSourceProperty，则从这个属性中获取下拉对象集合
            var refInfo = this.PropertyViewInfo.ReferenceViewInfo;
            var dataSourceProperty = refInfo.DataSourceProperty;
            if (string.IsNullOrWhiteSpace(dataSourceProperty)) throw new InvalidOperationException("string.IsNullOrWhiteSpace(dataSourceProperty) must be false.");

            var values = this.Context.CurrentObject.GetPropertyValue(dataSourceProperty) as EntityList;
            if (values != this._listView.Data)
            {
                try
                {
                    this.SelectedByUser = false;

                    this._listView.Data = values;
                }
                finally
                {
                    this.SelectedByUser = true;
                }

                //如果有RootPID的值，则控件只显示Parent下面的子节点
                var rootPId = refInfo.RootPIdProperty;
                if (!string.IsNullOrEmpty(rootPId))
                {
                    var rootPIdValue = this.Context.CurrentObject.GetPropertyValue<int?>(rootPId);

                    this._listView.BindData(rootPIdValue);
                }
            }
        }

        /// <summary>
        /// 加载数据
        /// 
        /// 如果已经初始化CslaDataProvider，则从里面获取数据。
        /// 否则表示
        /// </summary>
        /// <param name="async">
        /// 是否使用异步模式
        /// </param>
        private void EnsureDataLoaded(bool async)
        {
            if (this.IsUseLocalData())
            {
                //本地时，每次都检测属性的值是否是最新的。
                this.RefreshPropertyDataSource();
            }
            else
            {
                if (this._listView.Data == null)
                {
                    if (async)
                    {
                        this._listView.DataLoader.LoadDataAsync(() =>
                        {
                            using (this.TryEnterProcess(ProcessSource.AsyncDataLoaded))
                            {
                                this.SyncValueToSelection();
                            }
                        });
                    }
                    else
                    {
                        this._listView.Data = RF.Create(this._listView.EntityType).GetAll();
                    }
                }
            }
        }

        #endregion

        #region 私有方法

        private bool IsMultiSelection
        {
            get { return this.PropertyViewInfo.ReferenceViewInfo.SelectionMode == ReferenceSelectionMode.Multiple; }
        }

        private bool IsUseLocalData()
        {
            return !string.IsNullOrEmpty(this.PropertyViewInfo.ReferenceViewInfo.DataSourceProperty);
        }

        /// <summary>
        /// 根据SelectedValuePath指定的值，获取目标属性值
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private object GetSelectedValue(Entity entity)
        {
            string selectedValuePath = this.PropertyViewInfo.ReferenceViewInfo.SelectedValuePath;
            if (string.IsNullOrEmpty(selectedValuePath)) { return entity.Id; }

            return entity.GetPropertyValue(selectedValuePath);
        }

        #endregion

        #region 当前流程鉴别器

        /// <summary>
        /// 当前流程的“源头”
        /// 
        /// 由于一个外部事件，可能引发此类中的多个方法。
        /// 设计了这个属性，是为了在方法体内区别：该方法是直接被外部调用而触发，还是被其它方法间接触发。
        /// </summary>
        private ProcessSource CurrentProcess
        {
            get { return this._processIndicator.ProcessSource; }
        }

        private IDisposable TryEnterProcess(ProcessSource process)
        {
            return this._processIndicator.EnterProcess(process);
        }

        private ProcessSourceIndicator<ProcessSource> _processIndicator = new ProcessSourceIndicator<ProcessSource>();

        private enum ProcessSource
        {
            None,

            ListViewCurrentObjectChanged,

            PropertyValueChanged,

            DropdownOpend,

            AsyncDataLoaded
        }

        #endregion
    }
}