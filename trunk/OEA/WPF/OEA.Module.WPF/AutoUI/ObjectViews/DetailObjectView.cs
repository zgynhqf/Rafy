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
        /// 当前生成的控件是几列的？
        /// </summary>
        public int ColumnsCount
        {
            get
            {
                var colCount = this.Meta.ColumnsCountShowInDetail;
                if (colCount != 0) return colCount;

                var detailPropertiesCount = this.Meta.EntityProperties.Where(e => e.CanShowIn(ShowInWhere.Detail)).Count();
                return detailPropertiesCount > 6 ? 2 : 1;
            }
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
        /// 属性更改时，更新子视图的值
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCurrentObjectPropertyChanged(PropertyChangedEventArgs e)
        {
            //当前属性是明细时,如果更改了属性值,则需要自动刷新细表
            foreach (var view in this.ChildrenViews)
            {
                if (view.PropertyName == e.PropertyName)
                {
                    view.LoadDataFromParent();
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
    }
}