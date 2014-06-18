/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110509
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110509
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rafy.WPF.Command;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using System.Collections.Specialized;

namespace Rafy.WPF
{
    /// <summary>
    /// 用于最终绑定界面的模块视图
    /// </summary>
    public class ModuleViewModel : ViewModel
    {
        #region 字段

        private string _label, _translatedLabel;

        private bool _isSelected;

        /// <summary>
        /// 是否在被选择时，自动触发选中行为
        /// </summary>
        internal bool _autoDoActionAsSelected = true;

        private WPFModuleMeta _moduleInfo;

        private Action<CustomActionArgs> _action;

        internal ModuleViewModelCollection _ownerCollection;

        private ObservableCollection<ModuleViewModel> _subModules = new ObservableCollection<ModuleViewModel>();

        #endregion

        /// <summary>
        /// 注意：此为唯一标识符。
        /// 
        /// 暂不支持客户化。
        /// </summary>
        public string Label
        {
            get { return this._label; }
            set
            {
                if (this._label != value)
                {
                    this._label = value;

                    this.TranslatedLabel = value.Translate();

                    this.NotifyPropertyChanged("Label");
                }
            }
        }

        /// <summary>
        /// 翻译后的显示文字。
        /// </summary>
        public string TranslatedLabel
        {
            get { return this._translatedLabel; }
            set
            {
                if (this._translatedLabel != value)
                {
                    this._translatedLabel = value;

                    this.NotifyPropertyChanged("TranslatedLabel");

                    this.OnTranslatedLabelChanged();
                }
            }
        }

        /// <summary>
        /// 类型
        /// </summary>
        public ModuleViewModelType Type { get; private set; }

        /// <summary>
        /// 是否被选中（模块栏中只有一个模块会被选中）
        /// </summary>
        public bool IsSelected
        {
            get { return this._isSelected; }
            set
            {
                //变更为 false、或者可以被变更为 true 时。
                if (this._isSelected != value && (!value || this.SelectionEnabled))
                {
                    //如果本节点设置为真，则执行选中动作，并更新集合中的选中项。
                    if (value && this._ownerCollection != null)
                    {
                        if (_autoDoActionAsSelected)
                        {
                            var needSelected = this.TryDoAction();
                            if (!needSelected) return;
                        }

                        var oldItem = this._ownerCollection.SelectedItem;
                        if (oldItem != null) oldItem.IsSelected = false;
                        this._ownerCollection.SelectedItem = this;
                    }

                    //值变更。
                    this._isSelected = value;
                    this.NotifyPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// 是否可以进行选择
        /// 
        /// 只有可以选择的结点才能执行相应的行为。
        /// </summary>
        public bool SelectionEnabled
        {
            get
            {
                //自定义模块可以被选择，选择后直接执行自定义行为
                //元数据定义的界面模块也可以被选择，选择后显示相应的界面。
                return this.Type != ModuleViewModelType.DisplayOnly &&
                    (this._moduleInfo == null || this._moduleInfo.HasUI);
            }
        }

        /// <summary>
        /// 子模块
        /// </summary>
        public ObservableCollection<ModuleViewModel> SubModules
        {
            get { return this._subModules; }
        }

        /// <summary>
        /// TranslatedLabel 变化时发生此事件。
        /// </summary>
        protected virtual void OnTranslatedLabelChanged() { }

        #region 两种不同类型的模块

        /// <summary>
        /// 对应的模块元数据。
        /// </summary>
        public WPFModuleMeta ModuleMeta
        {
            get { return this._moduleInfo; }
            set
            {
                this._moduleInfo = value;

                if (value != null) { this.Type = ModuleViewModelType.EntityModule; }
            }
        }

        /// <summary>
        /// 自定义模块执行函数
        /// 
        /// 返回值：是否需要进入选中状态：处于选中状态的模块是不能再次点击的。
        /// </summary>
        public Action<CustomActionArgs> CustomAction
        {
            get { return this._action; }
            set
            {
                this._action = value;

                if (value != null) { this.Type = ModuleViewModelType.CustomAction; }
            }
        }

        private bool _isRunning = false;

        /// <summary>
        /// 执行某个模块行为，返回是否需要进入选中状态。
        /// </summary>
        /// <returns>返回是否需要进入选中状态</returns>
        private bool TryDoAction()
        {
            if (this._isRunning) return false;

            var selected = false;

            try
            {
                this._isRunning = true;

                if (this.Type == ModuleViewModelType.EntityModule)
                {
                    var win = App.Current.OpenModuleOrAlert(this._moduleInfo);
                    selected = win != null;
                }
                else
                {
                    var args = new CustomActionArgs { Module = this._moduleInfo };
                    this._action(args);
                    selected = args.Selected;
                }
            }
            finally
            {
                this._isRunning = false;
            }

            return selected;
        }

        #endregion

        /// <summary>
        /// CustomAction Args
        /// </summary>
        public class CustomActionArgs
        {
            /// <summary>
            /// 对象的模板元数据
            /// </summary>
            public ModuleMeta Module { get; internal set; }

            /// <summary>
            /// 设置是否让本节点进入选中状态。
            /// </summary>
            public bool Selected { get; set; }
        }
    }

    /// <summary>
    /// 界面中绑定的 ModuleViewModel 集合
    /// </summary>
    public class ModuleViewModelCollection : ObservableCollection<ModuleViewModel>
    {
        /// <summary>
        /// 当前选中项。
        /// </summary>
        internal ModuleViewModel SelectedItem;

        /// <summary>
        /// 在整个模块树中找指定名称的模块
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public ModuleViewModel FindByLabel(string label)
        {
            return this.TravalTree(vm => vm.Label == label);
        }

        /// <summary>
        /// 遍历树中的所有结点，直到 finder 返回 true。
        /// </summary>
        /// <param name="finder"></param>
        /// <returns></returns>
        public ModuleViewModel TravalTree(Func<ModuleViewModel, bool> finder)
        {
            return TravalTree(this, finder);
        }

        #region 数据装载

        internal void ReadEntityModules(IList<ModuleMeta> roots)
        {
            foreach (WPFModuleMeta rootModule in roots)
            {
                var root = this.CreateViewModel(rootModule);
                this.Add(root);

                this.ReadChildrenRecur(root);
            }
        }

        private void ReadChildrenRecur(ModuleViewModel module)
        {
            var meta = module.ModuleMeta;
            foreach (WPFModuleMeta child in meta.GetChildrenWithPermission())
            {
                var childModule = this.CreateViewModel(child);
                module.SubModules.Add(childModule);

                this.ReadChildrenRecur(childModule);
            }
        }

        /// <summary>
        /// 使用元数据中定义的模块来构造一个新的模块视图模型。
        /// 
        /// 子类可重写此方法来生成新的视图对象。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private ModuleViewModel CreateViewModel(WPFModuleMeta meta)
        {
            var vm = this.CreateViewModelCore(meta);
            vm._ownerCollection = this;

            vm.Label = meta.Label;
            vm.ModuleMeta = meta;

            return vm;
        }

        /// <summary>
        /// 使用元数据中定义的模块来构造一个新的模块视图模型。
        /// 
        /// 子类可重写此方法来生成新的视图对象。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        protected virtual ModuleViewModel CreateViewModelCore(WPFModuleMeta meta)
        {
            return new ModuleViewModel();
        }

        #endregion

        #region 树中添加任何对象时，都设置好它的 _ownerCollection 字段

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            this.OnAnyCollectionChanged(e);
        }

        private void OnAnyCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            foreach (ModuleViewModel item in e.NewItems)
            {
                this.PrepareItem(item);
            }
        }

        private void PrepareItem(ModuleViewModel item)
        {
            item._ownerCollection = this;

            item.SubModules.CollectionChanged += (o, ee) => this.OnAnyCollectionChanged(ee);

            TravalTree(item.SubModules, vm =>
            {
                this.PrepareItem(vm);
                return false;
            });
        }

        #endregion

        private static ModuleViewModel TravalTree(IList<ModuleViewModel> list, Func<ModuleViewModel, bool> finder)
        {
            foreach (var vm in list)
            {
                if (finder(vm)) return vm;

                var item = TravalTree(vm.SubModules, finder);
                if (item != null) return item;
            }

            return null;
        }
    }

    /// <summary>
    /// ModuleViewModel 的几种分类
    /// </summary>
    public enum ModuleViewModelType
    {
        /// <summary>
        /// 只用于显示。
        /// </summary>
        DisplayOnly,

        /// <summary>
        /// 实体模块
        /// </summary>
        EntityModule,

        /// <summary>
        /// 用户自定义行为
        /// </summary>
        CustomAction
    }
}
