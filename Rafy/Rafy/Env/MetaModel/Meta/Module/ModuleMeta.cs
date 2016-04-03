/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120311
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120311
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 模块的元数据。
    /// </summary>
    [DebuggerDisplay("Label:{Label}")]
    public abstract class ModuleMeta : MetaBase
    {
        public ModuleMeta()
        {
            this._Children = new ModuleChildren(this);
        }

        private string _KeyLabel;
        /// <summary>
        /// 这个属性表示这个模块的名称。
        /// 
        /// 注意，这个名称在整个应用程序中所有模块中应该是唯一的，这样，就可以用它来实现权限控制。
        /// </summary>
        public string KeyLabel
        {
            get
            {
                var value = this._KeyLabel;

                if (string.IsNullOrWhiteSpace(value))
                {
                    value = this.Label;
                }

                return value;
            }
            set { this.SetValue(ref this._KeyLabel, value); }
        }

        private string _Label;
        /// <summary>
        /// 友好显示标签
        /// </summary>
        public string Label
        {
            get { return this._Label; }
            set { this.SetValue(ref this._Label, value); }
        }

        /*********************** 代码块解释 *********************************
         * 当 HasUI 为 false 时，EntityType、AggtBlocksName、CustomUI 等都不起作用。
         * 
         * 当 CustomUI 不为空时，表示一个自定义界面的模块，此时 EntityType、AggtBlocksName 不起作用。
         * 
         * 当 CustomUI 为空时，表示这个模块的界面使用 AutoUI 自动生成，EntityType 表示用于生成界面的实体。
         * 此时，如果 AggtBlocksName 不为空，表示这个界面使用一个用户定义的聚合块，否则使用默认生成的聚合块。
        **********************************************************************/

        /// <summary>
        /// 如果此属性返回 false，表示当前模块只是一个文件夹模块，它不对应任何的模块界面。
        /// </summary>
        public bool HasUI
        {
            get
            {
                return this.IsCustomUI ||
                    this._EntityType != null ||
                    !string.IsNullOrEmpty(this._AggtBlocksName);
            }
        }

        /// <summary>
        /// 是否为自定义模块
        /// </summary>
        public abstract bool IsCustomUI { get; }

        private string _AggtBlocksName;
        /// <summary>
        /// 如果当前模块是一个主动定义的聚合块，则这个属性表示此聚合块的名称。
        /// </summary>
        public string AggtBlocksName
        {
            get { return this._AggtBlocksName; }
            set { this.SetValue(ref this._AggtBlocksName, value); }
        }

        private Type _EntityType;
        /// <summary>
        /// 这个模块使用 AutoUI 功能的话，这个属性表示其显示的实体类型，否则返回 null。
        /// </summary>
        public Type EntityType
        {
            get { return this._EntityType; }
            set { this.SetValue(ref this._EntityType, value); }
        }

        private Type _TemplateType;
        /// <summary>
        /// 本模块使用的界面块模板类型。
        /// 如果指定此属性，则指定的该类型必须继承自 BlocksTemplate 类。
        /// <remarks>
        /// （注意，如果是 WPF 应用程序，则这个属性指定的类型必须继承自 UITemplate 类。）
        /// </remarks>
        /// </summary>
        public Type BlocksTemplate
        {
            get { return this._TemplateType; }
            set { this.SetValue(ref this._TemplateType, value); }
        }

        private ModuleMeta _Parent;
        /// <summary>
        /// 对应的父模块
        /// </summary>
        public ModuleMeta Parent
        {
            get { return this._Parent; }
        }

        private ModuleChildren _Children;
        /// <summary>
        /// 模块中的子模块
        /// </summary>
        public IList<ModuleMeta> Children
        {
            get { return this._Children; }
        }

        /// <summary>
        /// 返回所有可显示的模块。
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ModuleMeta> GetChildrenWithPermission()
        {
            return this.Children.Where(PermissionMgr.CanShowModule);
        }

        private IList<ModuleOperation> _CustomOpertions = new List<ModuleOperation>(0);
        /// <summary>
        /// 开发人员可以在这个模块中添加许多自定义功能。
        /// 权限系统为读取这个属性用于用户配置。
        /// </summary>
        public IList<ModuleOperation> CustomOpertions
        {
            get { return this._CustomOpertions; }
        }

        protected override void OnFrozen()
        {
            base.OnFrozen();

            this._Children.IsFrozen = true;
            this._CustomOpertions = new ReadOnlyCollection<ModuleOperation>(this._CustomOpertions);
        }

        #region private class ModuleChildren

        private class ModuleChildren : Collection<ModuleMeta>
        {
            internal bool IsFrozen;

            private ModuleMeta _owner;

            public ModuleChildren(ModuleMeta owner)
            {
                this._owner = owner;
            }

            protected override void InsertItem(int index, ModuleMeta item)
            {
                if (this.IsFrozen) throw new InvalidOperationException();

                base.InsertItem(index, item);

                if (item != null) { item._Parent = this._owner; }
            }

            protected override void ClearItems()
            {
                if (this.IsFrozen) throw new InvalidOperationException();

                base.ClearItems();
            }

            protected override void RemoveItem(int index)
            {
                if (this.IsFrozen) throw new InvalidOperationException();

                base.RemoveItem(index);
            }

            protected override void SetItem(int index, ModuleMeta item)
            {
                if (this.IsFrozen) throw new InvalidOperationException();

                base.SetItem(index, item);

                if (item != null) { item._Parent = this._owner; }
            }
        }

        #endregion
    }

    /// <summary>
    /// Web 应用程序中的模块定义
    /// </summary>
    public class WebModuleMeta : ModuleMeta
    {
        /// <summary>
        /// 是否为自定义模块
        /// </summary>
        public override bool IsCustomUI
        {
            get { return !string.IsNullOrEmpty(this.Url); }
        }

        private string _Url;
        /// <summary>
        /// 如果该模块不是由某个类型自动生成的，则这个属性将不为空，并表示某个自定义的 UI 界面。
        /// 
        /// 则这个属性表示目标页面的地址。
        /// </summary>
        public string Url
        {
            get { return this._Url; }
            set { this.SetValue(ref this._Url, value); }
        }

        private string _ClientRuntime;
        /// <summary>
        /// Web 界面中本模块使用的界面模板类型。
        /// 如果指定此属性，则指定的该类型必须继承自 Rafy.UITemplate 类。
        /// </summary>
        public string ClientRuntime
        {
            get { return this._ClientRuntime; }
            set { this.SetValue(ref this._ClientRuntime, value); }
        }
    }

    /// <summary>
    /// WPF 应用程序中的模块定义
    /// </summary>
    public class WPFModuleMeta : ModuleMeta
    {
        /// <summary>
        /// 是否为自定义模块
        /// </summary>
        public override bool IsCustomUI
        {
            get { return _CustomUI != null; }
        }

        private Type _CustomUI;
        /// <summary>
        /// 如果该模块不是由某个类型自动生成的，则这个属性将不为空，并表示某个自定义的 UI 界面。
        /// 
        /// 如果是 WPF 程序，那么这个属性表示目标用户控件的全名称，
        /// 如果是 Web 程序，则这个属性表示目标页面的地址。
        /// </summary>
        public Type CustomUI
        {
            get { return this._CustomUI; }
            set { this.SetValue(ref this._CustomUI, value); }
        }

        private bool _TryAutoLoadData = true;
        /// <summary>
        /// 在没有条件、导航面板时，尝试加载所有数据。默认是 true 。
        /// </summary>
        public bool TryAutoLoadData
        {
            get { return this._TryAutoLoadData; }
            set { this.SetValue(ref this._TryAutoLoadData, value); }
        }
    }
}