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
using OEA.WPF.Command;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 用于最终绑定界面的模块视图
    /// </summary>
    public class ModuleViewModel : ViewModel
    {
        #region 字段

        private bool _isSelected;

        private ModuleMeta _moduleInfo;

        private Func<bool> _action;

        private List<ModuleViewModel> _subModules = new List<ModuleViewModel>();

        #endregion

        public ModuleViewModel(ModuleMeta moduleInfo)
        {
            this.Label = moduleInfo.Label;
            this.ModuleInfo = moduleInfo;
        }

        /// <summary>
        /// 注意：此为唯一标识符。
        /// 
        /// 暂不支持客户化。
        /// </summary>
        public string Label { get; private set; }

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
            internal set
            {
                if (this._isSelected != value)
                {
                    this._isSelected = value;

                    this.NotifyPropertyChanged("IsSelected");
                }
            }
        }

        /// <summary>
        /// 是否可以进行选择
        /// </summary>
        public bool SelectionEnabled
        {
            get { return this._moduleInfo.HasUI; }
        }

        /// <summary>
        /// 子模块
        /// </summary>
        public IList<ModuleViewModel> SubModules
        {
            get { return this._subModules; }
        }

        #region 三种不同类型的模块

        public ModuleMeta ModuleInfo
        {
            get { return this._moduleInfo; }
            set
            {
                this._moduleInfo = value;
                if (value != null)
                {
                    this.Type = ModuleViewModelType.EntityModule;
                }
            }
        }

        /// <summary>
        /// 自定义模块执行函数
        /// 
        /// 返回值：是否需要进入选中状态：处于选中状态的模块是不能再次点击的。
        /// </summary>
        public Func<bool> CustomAction
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
        /// <returns></returns>
        internal bool TryDoAction()
        {
            if (this._isRunning) return false;

            try
            {
                this._isRunning = true;

                switch (this.Type)
                {
                    case ModuleViewModelType.EntityModule:
                        App.Current.OpenModuleOrAlert(this._moduleInfo);
                        return true;
                    case ModuleViewModelType.CustomAction:
                        return this._action();
                    default:
                        return true;
                }
            }
            finally
            {
                this._isRunning = false;
            }
        }

        #endregion
    }

    public class ModuleViewModelCollection : ObservableCollection<ModuleViewModel>
    {
        private IList<ModuleMeta> _roots;

        internal ModuleViewModelCollection(IList<ModuleMeta> roots)
        {
            this._roots = roots;

            this.ReadEntityModules();
        }

        /// <summary>
        /// 在整个模块树中找指定名称的模块
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public ModuleViewModel FindByLabel(string label)
        {
            return FindByLabel(this, label);
        }

        /// <summary>
        /// 选中唯一一个菜单。
        /// </summary>
        /// <param name="item"></param>
        public void SelectSingle(string label)
        {
            var vm = this.FindByLabel(label);

            if (vm != null) { this.SelectSingle(vm); }
        }

        /// <summary>
        /// 把某个菜单的选中状态清除
        /// </summary>
        /// <param name="label"></param>
        public void Deselect(string label)
        {
            var vm = this.FindByLabel(label);
            vm.IsSelected = false;
        }

        /// <summary>
        /// 选中唯一一个。
        /// </summary>
        /// <param name="item"></param>
        public void SelectSingle(ModuleViewModel item)
        {
            if (!item.IsSelected && item.SelectionEnabled)
            {
                var needSelected = item.TryDoAction();

                if (needSelected)
                {
                    TravalTree(this, vm => vm.IsSelected = false);

                    item.IsSelected = true;
                }
            }
        }

        private void ReadEntityModules()
        {
            foreach (var rootModule in this._roots)
            {
                var root = new ModuleViewModel(rootModule);
                this.Add(root);

                ReadChildrenRecur(root);
            }
        }

        private static void ReadChildrenRecur(ModuleViewModel module)
        {
            var meta = module.ModuleInfo;
            foreach (var child in meta.GetChildrenWithPermission())
            {
                var childModule = new ModuleViewModel(child);
                module.SubModules.Add(childModule);

                ReadChildrenRecur(childModule);
            }
        }

        private static void TravalTree(IList<ModuleViewModel> list, Action<ModuleViewModel> action)
        {
            foreach (var vm in list)
            {
                action(vm);
                TravalTree(vm.SubModules, action);
            }
        }

        private static ModuleViewModel FindByLabel(IList<ModuleViewModel> list, string label)
        {
            foreach (var vm in list)
            {
                if (vm.Label == label) { return vm; }

                var item = FindByLabel(vm.SubModules, label);
                if (item != null) return item;
            }

            return null;
        }
    }

    public enum ModuleViewModelType
    {
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
