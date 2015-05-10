/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF.Controls;
using Rafy.WPF.Editors;

namespace Rafy.WPF
{
    /// <summary>
    /// 某实体的详细显示视图。
    /// </summary>
    public class DetailLogicalView : LogicalView
    {
        internal DetailLogicalView(WPFEntityViewMeta entityViewInfo) : base(entityViewInfo) { }

        public new Form Control
        {
            get { return base.Control as Form; }
        }

        /// <summary>
        /// 默认当前生成的动态控件应该是几列的？
        /// </summary>
        /// <param name="properties">如果只显示这些属性，计算需要的列。</param>
        /// <returns></returns>
        internal int CalculateColumnsCount(IEnumerable<WPFEntityPropertyViewMeta> properties = null)
        {
            var colCount = this.Meta.DetailColumnsCount;
            if (colCount != 0) return colCount;

            //查询面板动态的列永远只有一列。
            if (this is QueryLogicalView) return 1;

            if (properties == null) properties = this.Meta.EntityProperties.Cast<WPFEntityPropertyViewMeta>().Where(e => e.CanShowIn(ShowInWhere.Detail));

            var detailPropertiesCount = properties.Count();
            if (detailPropertiesCount <= 8) { return 1; }
            if (detailPropertiesCount <= 16) { return 2; }
            return 3;
        }

        internal override void LoadDataFromParent()
        {
            if (this.CouldLoadDataFromParent())
            {
                //var entityPropertyView = this.ChildBlock as WPFEntityPropertyViewMeta;
                //var refEntityProperty = entityPropertyView.ReferenceViewInfo.ReferenceInfo.RefEntityProperty;
                //var data = this.Parent.CurrentObject.GetPropertyValue(refEntityProperty);

                var data = this.Parent.Current
                    .GetRefEntity((this.ChildBlock.ChildrenProperty as IRefProperty).RefEntityProperty);

                this.Data = data;
            }
        }

        #region 适配 CurrentObject，Data 两个属性

        /// <summary>
        /// 详细面板的当前实体就是全部数据。
        /// </summary>
        public override Entity Current
        {
            get { return this.Data as Entity; }
            set { this.Data = value; }
        }

        /// <summary>
        /// 详细面板的全部数据只能是一个实体。
        /// </summary>
        public new Entity Data
        {
            get { return base.Data as Entity; }
            set { base.Data = value; }
        }

        protected override void RefreshCurrentEntityCore()
        {
            this.BindCurrentObject(this.Current);

            //this.OnCurrentChanged();
            //this.Control.DataContext = this.CurrentObject;//赋值后TextBox控件的IsReadOnly会重置，不知原因？？
        }

        protected override void OnDataChanging(IDomainComponent oldValue, IDomainComponent newValue)
        {
            var oldEntity = oldValue as Entity;
            if (oldEntity != null)
            {
                oldEntity.PropertyChanged -= Current_PropertyChanged;
            }

            if (newValue is EntityList)
            {
                throw new InvalidOperationException("详细面板的全部数据只支持设置为单一实体。");
            }

            base.OnDataChanging(oldValue, newValue);
        }

        protected override void OnDataChanged()
        {
            var current = this.Data;

            this.BindCurrentObject(current);

            this.OnCurrentChanged();

            //绑定属性变更事件。
            if (current != null)
            {
                current.PropertyChanged += Current_PropertyChanged;
            }

            base.OnDataChanged();
        }

        #endregion

        #region CurrentEntityPropertyChanged

        private void BindCurrentObject(Entity current)
        {
            this.Control.DataContext = current;
        }

        private void Current_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnCurrentEntityPropertyChanged(e);
        }

        /// <summary>
        /// 声明一个向父类路由的选择实体改变事件。
        /// </summary>
        internal static readonly RoutedViewEvent CurrentEntityPropertyChangedEvent =
            RoutedViewEvent.Register(typeof(DetailLogicalView), RoutedEventType.ToParent);

        /// <summary>
        /// 子类重写此方法实现当前对象属性变更处理逻辑。
        /// 
        /// 默认逻辑：聚合子属性更改时，更新聚合子视图的数据。
        /// </summary>
        /// <param name="e"></param>
        internal virtual void OnCurrentEntityPropertyChanged(PropertyChangedEventArgs e)
        {
            //当前属性是聚合子属性变更时，则需要自动刷新聚合子视图数据。
            foreach (var view in this.ChildrenViews)
            {
                var block = view.ChildBlock;
                if (block != null && block.ChildrenPropertyName == e.PropertyName)
                {
                    //先清空属性，
                    view.Data = null;
                    view.LoadDataFromParent();
                    break;
                }
            }

            this.RaiseRoutedEvent(CurrentEntityPropertyChangedEvent, e);
        }

        #endregion

        #region PropertyEditors

        private List<IPropertyEditor> _editors = new List<IPropertyEditor>();

        /// <summary>
        /// 这个View使用的所有的属性Editor
        /// Key：BOType的属性
        /// Value：这个属性使用的编辑器
        /// </summary>
        public ReadOnlyCollection<IPropertyEditor> PropertyEditors
        {
            get { return this._editors.AsReadOnly(); }
        }

        /// <summary>
        /// 焦点定位到指定的属性编辑器
        /// </summary>
        /// <param name="property">指定的托管属性。</param>
        public bool FocusToEditor(IManagedProperty property)
        {
            var editor = this.FindPropertyEditor(property);
            if (editor == null) return false;

            return this.FocusToEditor(editor);
        }

        /// <summary>
        /// 焦点定位到指定的属性编辑器
        /// </summary>
        /// <param name="editor"></param>
        public bool FocusToEditor(IPropertyEditor editor)
        {
            /*********************** 代码块解释 *********************************
             * 由于表单中可以使用了页签控件来作为许多属性的划分容器，
             * 所以此时需要找到指定属性对应的 TabItem，先激活该页签，
             * 然后才能对其中的属性编辑器进行设置焦点的操作。
            **********************************************************************/

            var editorControl = editor.Control;
            var form = this.Control;

            //查找 Form 中的 TabControl 控件。
            //如果还没有生成完毕，则先更新布局，再尝试查找一次。
            var tabControl = form.GetVisualChild<TabControl>();
            if (tabControl == null)
            {
                form.UpdateLayout();
                tabControl = form.GetVisualChild<TabControl>();
            }

            if (tabControl != null)
            {
                //找到编辑器对应的 TabItem，设置为选中状态，然后焦点选中。
                //方法：编辑器的可视根元素应该与 TabItem 的 Content 属性一致。
                foreach (var item in tabControl.Items)
                {
                    var tabItem = tabControl.ItemContainerGenerator.ContainerFromItem(item) as TabItem;
                    if (tabItem != null)
                    {
                        //如果 TabItem 包含了这个编辑器。
                        var content = tabItem.Content as Visual;
                        if (content != null && content.IsAncestorOf(editorControl))
                        {
                            tabItem.IsSelected = true;

                            //如果 TabItem 已经生成了可视树，则可以直接设置焦点成功。
                            //否则，则应该先更新布局，再设置编辑器的焦点。
                            var res = editorControl.Focus();
                            if (!res)
                            {
                                tabItem.UpdateLayout();
                                res = editorControl.Focus();
                            }

                            //简单处理：异步设置焦点的情况下，直接视为成功。
                            return res;
                        }
                    }
                }
            }

            return editorControl.Focus();
        }

        /// <summary>
        /// 在View中寻找指定属性的Editor
        /// </summary>
        /// <param name="conView"></param>
        /// <param name="property">找这个属性对应的Editor</param>
        /// <returns></returns>
        public IPropertyEditor FindPropertyEditor(IManagedProperty property)
        {
            return this._editors.FirstOrDefault(e => e.Meta.PropertyMeta.ManagedProperty == property);
        }

        /// <summary>
        /// 在视图中寻找指定属性的编辑器。
        /// </summary>
        /// <param name="conView"></param>
        /// <param name="propertyName">找这个属性对应的Editor</param>
        /// <returns></returns>
        public IPropertyEditor FindPropertyEditor(string propertyName)
        {
            return this._editors.FirstOrDefault(e => e.Meta.Name == propertyName);
        }

        /// <summary>
        /// 添加某个编辑器。
        /// </summary>
        /// <param name="editor"></param>
        /// <returns></returns>
        internal void AddPropertyEditor(IPropertyEditor editor)
        {
            this._editors.Add(editor);
        }

        #endregion

        protected override void RefreshControlCore()
        {
            //详细面板（表单）暂时不支持刷新控件。
        }

        protected override void SetControlReadOnly(ReadOnlyStatus value)
        {
            this.Control.IsReadOnly = value;
        }

        ///// <summary>
        ///// 可能会有一个列表对应这个详细视图
        ///// </summary>
        //public ListLogicalView ListView
        //{
        //    get
        //    {
        //        return this.TryFindRelation(DefaultSurrounderTypes.ListDetail.List) as ListLogicalView;
        //    }
        //}
    }
}