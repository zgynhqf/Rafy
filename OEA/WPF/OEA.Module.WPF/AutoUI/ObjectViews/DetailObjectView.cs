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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using OEA.Editors;
using System.Diagnostics;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Library;
using OEA.ManagedProperty;
using OEA.Library.Validation;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 某对象的详细显示视图
    /// </summary>
    public class DetailObjectView : WPFObjectView
    {
        internal protected DetailObjectView(EntityViewMeta entityViewInfo) : base(entityViewInfo) { }

        #region 属性

        ///// <summary>
        ///// 可能会有一个列表对应这个详细视图
        ///// </summary>
        //public ListObjectView ListView
        //{
        //    get
        //    {
        //        return this.TryFindRelation(DefaultSurrounderTypes.ListDetail.List) as ListObjectView;
        //    }
        //}

        /// <summary>
        /// 默认当前生成的动态控件应该是几列的？
        /// </summary>
        /// <param name="properties">如果只显示这些属性，计算需要的列。</param>
        /// <returns></returns>
        public int CalculateColumnsCount(IEnumerable<EntityPropertyViewMeta> properties = null)
        {
            var colCount = this.Meta.DetailColumnsCount;
            if (colCount != 0) return colCount;

            //查询面板动态的列永远只有一列。
            if (this is QueryObjectView) return 1;

            if (properties == null) properties = this.Meta.EntityProperties.Where(e => e.CanShowIn(ShowInWhere.Detail));

            var detailPropertiesCount = properties.Count();
            if (detailPropertiesCount <= 6) { return 1; }
            if (detailPropertiesCount <= 16) { return 2; }
            return 3;
        }

        #endregion

        public override void LoadDataFromParent()
        {
            if (this.CouldLoadDataFromParent())
            {
                //var entityPropertyView = this.ChildBlock as EntityPropertyViewMeta;
                //var refEntityProperty = entityPropertyView.ReferenceViewInfo.ReferenceInfo.RefEntityProperty;
                //var data = this.Parent.CurrentObject.GetPropertyValue(refEntityProperty);

                var data = this.Parent.Current
                    .GetLazyRef(this.ChildBlock.ChildrenProperty as IRefProperty).Entity;

                this.Data = data;
            }
        }

        #region 适配 CurrentObject，Data 两个属性，并监听 CurrentObject_PropertyChanged 事件。

        /// <summary>
        /// DetailObjectView 的 CurrentObject 就是 Data。
        /// </summary>
        public override Entity Current
        {
            get { return this.Data as Entity; }
            set { this.Data = value; }
        }

        public override void RefreshCurrentEntity()
        {
            this.SetCurrentObject(this.Current);
            //this.OnCurrentObjectChanged(EventArgs.Empty);
            //this.Control.DataContext = this.CurrentObject;//赋值后TextBox控件的IsReadOnly会重置，不知原因？？
        }

        protected override void OnDataChanging()
        {
            var currentModel = this.Data as INotifyPropertyChanged;
            if (currentModel != null)
            {
                currentModel.PropertyChanged -= CurrentObject_PropertyChanged;
            }

            base.OnDataChanging();
        }

        /// <summary>
        /// 设置时，刷新子视图
        /// </summary>
        protected override void OnDataChanged()
        {
            Entity currentObject = null;

            IList list = this.Data as IList;
            if (list == null)
            {
                currentObject = this.Data as Entity;
            }
            else if (list.Count > 0)
            {
                currentObject = list[0] as Entity;
            }

            if (currentObject != null)
            {
                this.SetCurrentObject(currentObject);

                var currentModel = currentObject as INotifyPropertyChanged;
                if (currentModel != null)
                {
                    currentModel.PropertyChanged -= CurrentObject_PropertyChanged;
                    currentModel.PropertyChanged += CurrentObject_PropertyChanged;
                }
            }

            base.OnDataChanged();
        }

        private void SetCurrentObject(object currentObject)
        {
            this.Control.DataContext = currentObject;

            this.OnCurrentObjectChanged();
        }

        private void CurrentObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.OnCurrentObjectPropertyChanged(e);
        }

        #endregion

        /// <summary>
        /// 声明一个向父类路由的选择实体改变事件。
        /// </summary>
        public static readonly RoutedViewEvent CurrentObjectPropertyChangedEvent =
            RoutedViewEvent.Register(typeof(DetailObjectView), RoutedEventType.ToParent);

        /// <summary>
        /// 子类重写此方法实现当前对象属性变更处理逻辑。
        /// 
        /// 默认逻辑：聚合子属性更改时，更新聚合子视图的数据。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCurrentObjectPropertyChanged(PropertyChangedEventArgs e)
        {
            //当前属性是聚合子属性变更时，则需要自动刷新聚合子视图数据。
            foreach (var view in this.ChildrenViews)
            {
                if (view.PropertyName == e.PropertyName)
                {
                    //先清空属性，
                    view.Data = null;
                    view.LoadDataFromParent();
                    break;
                }
            }

            this.RaiseRoutedEvent(CurrentObjectPropertyChangedEvent, e);
        }

        #region PropertyEditors

        private List<IPropertyEditor> _editors = new List<IPropertyEditor>();

        /// <summary>
        /// 这个View使用的所有的属性Editor
        /// Key：BOType的属性
        /// Value：这个属性使用的编辑器
        /// </summary>
        public IList<IPropertyEditor> PropertyEditors
        {
            get { return this._editors.AsReadOnly(); }
        }

        /// <summary>
        /// 焦点定位到指定的属性编辑器
        /// </summary>
        /// <param name="mp"></param>
        public void FocusToEditor(IManagedProperty property)
        {
            var editor = this.FindPropertyEditor(property);
            if (editor != null)
            {
                var editorControl = editor.Control as FrameworkElement;

                //如果有页签，则定位到某指定的页签后再定位焦点。
                var tabControl = this.Control.GetVisualChild<TabControl>();
                if (tabControl != null)
                {
                    //找到编辑器对应的 TabItem，设置为选中状态，然后焦点选中。
                    //方法：编辑器的可视根元素应该与 TabItem 的 Content 属性一致。
                    var editorRoot = editorControl.GetVisualRoot();
                    var tabItem = tabControl.Items.Cast<TabItem>().FirstOrDefault(i => i.Content == editorRoot);
                    if (tabItem != null)
                    {
                        tabItem.IsSelected = true;
                        tabItem.ListenLayoutUpdatedOnce((o, e) => { editorControl.Focus(); });
                    }

                    #region //代码暂留

                    //通过 DetailGroupName 与 TabItem.Header 对比来查找 TabItem
                    //var header = item.Header;
                    //while (header is ContentControl)
                    //{
                    //    header = (header as ContentControl).Content;
                    //}

                    //if (header is string)
                    //{
                    //    var strHeader = header as string;
                    //    if (property.DetailGroupName == strHeader)
                    //    {
                    //        item.IsSelected = true;
                    //        break;
                    //    }
                    //} 

                    #endregion
                }
                else
                {
                    editorControl.Focus();
                }
            }
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
        /// 在View中寻找指定属性的Editor
        /// </summary>
        /// <param name="conView"></param>
        /// <param name="propertyName">找这个属性对应的Editor</param>
        /// <returns></returns>
        public IPropertyEditor FindPropertyEditor(string propertyName)
        {
            return this._editors.FirstOrDefault(e => e.Meta.Name == propertyName);
        }

        /// <summary>
        /// 添加某个 Editor
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
    }
}