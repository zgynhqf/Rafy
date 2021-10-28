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
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Automation;
using Rafy.WPF.Controls;
using Rafy.Utils;

namespace Rafy.WPF.Editors
{
    /// <summary>
    /// 列表选择属性编辑器
    /// 
    /// 此类使用 ListLogicalView 作为实现的核心，构造了一个 ComboListControl 容器来呈现 ListLogicalView 中的 Control。
    /// </summary>
    public class LookupListPropertyEditor : EntitySelectionPropertyEditor
    {
        #region 私有字段 与 构造函数

        private ComboListControl _cmbList;

        private ListLogicalView _listView
        {
            get { return this._cmbList.InnerListView; }
        }

        protected LookupListPropertyEditor() { }

        #endregion

        #region 公共属性/方法

        /// <summary>
        /// 手动设置下拉的数据源
        /// </summary>
        public ListLogicalView ListView
        {
            get { return this._listView; }
        }

        public void RefreshDataSource()
        {
            this.ListView.DataLoader.LoadDataAsync(() =>
            {
                this.SyncValueToSelection(this.ListView);
            });
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
            var refVI = this.Meta.SelectionViewMeta;
            var refTypeMeta = refVI.RefTypeDefaultView.AsWPFView();

            var title = refTypeMeta.TitleProperty;
            if (title == null) throw new InvalidProgramException(string.Format("{0} 没有设置代表属性，生成下拉控件时出错。", refTypeMeta.Name));

            //创建 ComboListControl : 使用下拉控件显示
            this._cmbList = new ComboListControl
            {
                RefViewMeta = refTypeMeta,
                IsMultiSelection = this.IsMultiSelection,
                Name = this.Meta.Name,
                DisplayProperties = this.Meta.SelectionViewMeta.ListDisplayProperties
            };
            this._cmbList.InnerListView.CheckItemsChanged += On_ListView_CurrentObjectChanged;
            this._cmbList.ListViewSelectionChanged += On_ListView_CurrentObjectChanged;
            this._cmbList.DropDownOpened += On_ComboList_DropDownOpened;

            this.AddReadOnlyComponent(this._cmbList, ComboBox.IsEnabledProperty, false);

            this.ResetBinding(this._cmbList);

            this.SetAutomationElement(this._cmbList);

            return this._cmbList;
        }

        protected override DependencyProperty BindingProperty()
        {
            return ComboBox.TextProperty;
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
            using (this._process.TryEnterProcess(LLPEProcess.ListViewCurrentObjectChanged))
            {
                if (this._process.Success) { this.SyncSelectionToValue(); }
            }
        }

        private void On_ComboList_DropDownOpened(object sender, EventArgs e)
        {
            var isReadOnly = this.IsReadOnly;
            if (isReadOnly == ReadOnlyStatus.ReadOnly)
            {
                this._cmbList.IsDropDownOpen = false;
                return;
            }

            if (this._cmbList.CurrentProgress != CLCProgress.LazyFiltering)
            {
                //控件被下拉时，自动加载数据，并定位到当前对象。
                using (this._process.TryEnterProcess(LLPEProcess.DropdownOpend))
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
            using (this._process.TryEnterProcess(LLPEProcess.PropertyValueChanged))
            {
                base.OnPropertyValueChanged();

                //如果当前流程是由 CurrentObject 改变引起的，所以不需要再对它进行定位了。
                if (this._process.Success) { this.SyncValueToSelection(); }
            }
        }

        #endregion

        #region 同步 Value 和 Selection

        /// <summary>
        /// 根据当前的值（PropertyValue），找到并定位到当前对象
        /// </summary>
        private void SyncValueToSelection()
        {
            try
            {
                this.SelectedByUser = false;

                this.SyncValueToSelection(this._listView);
            }
            finally
            {
                this.SelectedByUser = true;
            }
        }

        /// <summary>
        /// 根据选中的对象设置CurrentObject相关的值。
        /// </summary>
        private void SyncSelectionToValue()
        {
            this.SyncSelectionToValue(this._listView.SelectedEntities);
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
            var rvm = this.Meta.SelectionViewMeta;

            //以一种不会出错的方式来尝试获取数据源属性。
            object rawValues = null;
            try
            {
                if (rvm.DataSourceProvider != null)
                {
                    rawValues = rvm.DataSourceProvider(_context.CurrentObject);
                }
                else
                {
                    rawValues = this.Context.CurrentObject.GetProperty(rvm.DataSourceProperty);
                }
            }
            catch { }

            if (rawValues != null)
            {
                var values = rawValues as EntityList;
                if (values == null) throw new InvalidProgramException(string.Format(
                    "属性 {0} 的数据源必须是一个 EntityList 类型。", this.Meta));

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
                            using (this._process.TryEnterProcess(LLPEProcess.AsyncDataLoaded))
                            {
                                this.SyncValueToSelection();
                            }
                        });
                    }
                    else
                    {
                        this._listView.Data = RF.Find(this._listView.EntityType).CacheAll();
                    }
                }
            }
        }

        private bool IsUseLocalData()
        {
            var rvm = this.Meta.SelectionViewMeta;
            return rvm.DataSourceProvider != null || rvm.DataSourceProperty != null;
        }

        #endregion

        #region 当前流程鉴别器

        /// <summary>
        /// 当前流程的“源头”
        /// 
        /// 由于一个外部事件，可能引发此类中的多个方法。
        /// 设计了这个属性，是为了在方法体内区别：该方法是直接被外部调用而触发，还是被其它方法间接触发。
        /// </summary>
        private ProcessSourceIndicator<LLPEProcess> _process = new ProcessSourceIndicator<LLPEProcess>();

        private enum LLPEProcess
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