using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;



namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 列表编辑器
    /// </summary>
    public abstract class ListEditor
    {
        private IListEditorContext _context;

        private FrameworkElement _control;

        private bool? _isReadOnly;

        /// <summary>
        /// 构造露营
        /// 
        /// 子类在构造时，需要设置 Control 属性的值。
        /// </summary>
        /// <param name="context">视图对象</param>
        /// <param name="boType">显示的业务模型类型</param>
        protected ListEditor(IListEditorContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// 列表编辑控件
        /// </summary>
        public FrameworkElement Control
        {
            get { return this._control; }
        }

        internal virtual void SetControl(FrameworkElement value)
        {
            this._control = value;
        }

        /// <summary>
        /// 编辑器是否只读的
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                if (this._isReadOnly.HasValue) return this._isReadOnly.Value;

                //模型标记不可编辑
                if (this._context.Meta.NotAllowEdit) return true;

                //数据不可编辑
                var list = this._context.Data as IBindingList;
                if (list != null && !list.AllowEdit) return true;

                return false;
            }
            set
            {
                this._isReadOnly = value;
                this.OnIsReadOnlyChanged();
            }
        }

        protected virtual void OnIsReadOnlyChanged() { }

        /// <summary>
        /// 视图对象
        /// </summary>
        protected IListEditorContext Context
        {
            get { return this._context; }
        }

        /// <summary>
        /// 当前选中的数据模型
        /// </summary>
        public abstract object CurrentObject { get; set; }

        /// <summary>
        /// 当前选中的数据模型集合
        /// </summary>
        public abstract IList SelectedObjects { get; }

        /// <summary>
        /// 选择所有的数据
        /// </summary>
        public abstract void SelectAll();

        /// <summary>
        /// 通知列表编辑器，View.Data已经被改变了。
        /// </summary>
        internal void NotifyContextDataChanged()
        {
            this.OnContextDataChanged();
        }

        /// <summary>
        /// 通知列表编辑器，View.Data已经被改变了。
        /// </summary>
        protected abstract void OnContextDataChanged();

        /// <summary>
        /// 根据当前的数据源，强制刷新界面中的控件。
        /// </summary>
        public abstract void RefreshControl();

        /// <summary>
        /// 根据rootPid绑定数据
        /// </summary>
        /// <param name="rootPid">
        /// 如果这个值不是null，则这个值表示绑定的所有根节点的父id。
        /// </param>
        public abstract void BindData(int? rootPid);

        /// <summary>
        /// 控件的 “Check选择” 模式
        /// </summary>
        public abstract CheckingMode CheckingMode { get; set; }

        /// <summary>
        /// 在 CheckingMode 值为 CheckingRow 时，此属性有效。
        /// 它表示选中某行时，树型节点的级联选择行为。
        /// </summary>
        public abstract CheckingRowCascade CheckingRowCascade { get; set; }

        /// <summary>
        /// 过滤器
        /// </summary>
        public abstract Predicate<Entity> Filter { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public abstract IEnumerable<SortDescription> SortDescriptions { get; set; }

        /// <summary>
        /// 根集合用于分组的属性列表
        /// </summary>
        public abstract IEnumerable<string> RootGroupDescriptions { get; set; }
    }
}
