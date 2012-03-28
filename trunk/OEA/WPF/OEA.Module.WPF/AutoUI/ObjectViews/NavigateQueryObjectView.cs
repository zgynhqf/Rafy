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
 * 支持导航面板对象中孩子对象属性改变时，发生导航 胡庆访 20100328
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Diagnostics;
using System.ComponentModel;
using OEA.Library;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 导航查询面板视图控制器
    /// 
    /// 收集整个导航聚合对象中的所有导航属性（包括孩子对象的导航属性），
    /// 当某一个导航属性发生改变时，自动触发导航查询。
    /// </summary>
    public class NavigateQueryObjectView : QueryObjectView
    {
        #region 字段

        /// <summary>
        /// 导航对象中本身的导航属性
        /// </summary>
        private IList<NavigateProperty> _navigations = new List<NavigateProperty>();

        /// <summary>
        /// 导航对象中子实体的导航属性
        /// </summary>
        private IList<NavigateProperty> _childrenNavigations = new List<NavigateProperty>();

        #endregion

        #region 构造函数 与 初始化

        internal NavigateQueryObjectView(EntityViewMeta evm)
            : base(evm)
        {
            this.FindNavigationsRecur(this.Meta, this._navigations);
        }

        /// <summary>
        /// 找到所有的导航属性（包括子实体中已经标记的。）。
        /// </summary>
        /// <param name="entityView"></param>
        /// <param name="container"></param>
        private void FindNavigationsRecur(EntityViewMeta entityView, IList<NavigateProperty> container)
        {
            ////列表导航属性
            ////如果使用一个孩子集合属性直接作为导航项时，则同时必须附带一个可设置的 id 属性。
            ////这样，当用户选择某个孩子后，NavigateObjectView 会把选择好的孩子的 id 设置给此指定的属性。
            ////否则，导航不会起任何作用。
            //foreach (var propertyInfo in entityView.ChildrenProperties)
            //{
            //    if (propertyInfo.IsNavigateQueryItem())
            //    {
            //        var naviProperty = NavigateProperty.Convert(propertyInfo);
            //        container.Add(naviProperty);
            //    }
            //    else
            //    {
            //        this.FindNavigationsRecur(propertyInfo.ChildType, this._childrenNavigations);
            //    }
            //}

            //属性导航
            foreach (var propertyInfo in entityView.EntityProperties)
            {
                if (propertyInfo.NavigationMeta != null)
                {
                    var naviProperty = NavigateProperty.Convert(propertyInfo);
                    container.Add(naviProperty);
                }
                else
                {
                    //1:1 的子实体属性
                    var refInfo = propertyInfo.ReferenceViewInfo;
                    if (refInfo != null && refInfo.ReferenceInfo.Type == ReferenceType.Child)
                    {
                        this.FindNavigationsRecur(refInfo.RefTypeDefaultView, this._childrenNavigations);
                    }
                }
            }
        }

        #endregion

        #region 公有接口

        /// <summary>
        /// 这个导航面板使用的所有的导航属性。
        /// </summary>
        public IList<NavigateProperty> NavigateProperties
        {
            get
            {
                return _navigations;
            }
        }

        /// <summary>
        /// 使用这个查询面板中的查询对象数据，
        /// 给 newEntity 的外键设置值。
        /// </summary>
        /// <param name="newEntity"></param>
        public void SetReferenceEntity(object newEntity)
        {
            if (newEntity == null) throw new ArgumentNullException("referenceEntity");

            var currentObject = this.Current;

            //对每一个导航的实体引用属性，都给referenceEntity赋相应的值
            foreach (var naviProperty in this._navigations)
            {
                if (naviProperty.IsReferenceId)
                {
                    var idProperty = naviProperty.PropertyName;
                    var entityProperty = naviProperty.RefEntityProperty;

                    //读值
                    var idValue = currentObject.GetPropertyValue(idProperty);
                    var entityValue = currentObject.GetPropertyValue(entityProperty);

                    //尝试写值到新对象中
                    TrySetPropertyValue(newEntity, idProperty, idValue);
                    TrySetPropertyValue(newEntity, entityProperty, entityValue);
                }
            }
        }

        #endregion

        #region 导航主逻辑

        protected override void OnCurrentObjectPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnCurrentObjectPropertyChanged(e);

            this.RaiseQueryIfNavigationChanged(e.PropertyName);
        }

        /// <summary>
        /// 在这个事件中处理：子对象的属性改变/子对象集合的选择事件。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnRoutedEvent(object sender, RoutedViewEventArgs e)
        {
            base.OnRoutedEvent(sender, e);

            //目前只支持一级的子实体集合属性进行导航。
            //if (e.SourceView.Parent != this) return;

            //如果是子实体集合被选择，且它是导航项，则给相应的选择后属按约定赋值
            if (e.Event == ListObjectView.SelectedItemChangedEvent)
            {
                var args = e.Args as SelectedEntityChangedEventArgs;
                var selectedItem = args.NewItem;
                if (selectedItem != null)
                {
                    this.RaiseQueryIfChildrenSelected(args.View, selectedItem);
                }
            }
            //如果是 1:1 的子实体属性改变，且它是导航项，则需要导航。
            else if (e.Event == DetailObjectView.CurrentObjectPropertyChangedEvent)
            {
                var args = e.Args as PropertyChangedEventArgs;
                this.RaiseQueryIfNavigationChanged(args.PropertyName, e.SourceView.Meta);
            }
        }

        /// <summary>
        /// 如果某个导航项改变时，需要发生导航查询。
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="childEntityView">
        /// 如果为空，表示导航项定义在导航类本身当中。
        /// 否则，表示导航项定义在它的孩子类中。
        /// </param>
        private void RaiseQueryIfNavigationChanged(string propertyName, EntityViewMeta childEntityView = null)
        {
            //如果任何一个导航属性发生改变，则执行查询。
            if (childEntityView == null)
            {
                if (this._navigations.Any(p => p.PropertyName == propertyName))
                {
                    this.TryExecuteQuery();
                }
            }
            else
            {
                if (this._childrenNavigations.Any(p => p.PropertyName == propertyName && p.OwnerMeta == childEntityView))
                {
                    this.TryExecuteQuery();
                }
            }
        }

        /// <summary>
        /// 是否关闭自动查询的功能
        /// </summary>
        public bool SuppressAutoQuery { get; set; }

        private void TryExecuteQuery()
        {
            if (!this.SuppressAutoQuery) this.ExecuteQuery();
        }

        /// <summary>
        /// 通过外键引用实体，把CurrentObject相应的属性设置好。
        /// </summary>
        /// <param name="childrenProperty">可供选择的孩子属性，且它必须是一个导航项</param>
        /// <param name="selectedEntity">被引用的实体对象</param>
        private void RaiseQueryIfChildrenSelected(ObjectView childView, Entity selectedEntity)
        {
            if (childView == null) throw new ArgumentNullException("childView");
            if (selectedEntity == null) throw new ArgumentNullException("selectedEntity");

            var childProperty = childView.ChildBlock.ChildrenPropertyMeta;

            //根据propertyName在_navigateProperties列表中，查找已经得到的导航属性。
            NavigateProperty naviProperty = null;
            if (childProperty.Owner == this.Meta.EntityMeta)
            {
                string childrenProperty = childProperty.Name;
                naviProperty = this._navigations.FirstOrDefault(n => n.PropertyMeta.Name == childrenProperty);
            }
            else
            {
                naviProperty = this._childrenNavigations.FirstOrDefault(n => n.PropertyMeta.PropertyMeta == childProperty);
            }

            if (naviProperty != null)
            {
                if (!naviProperty.IsReferenceId) throw new InvalidOperationException("这个导航属性必须是外键引用实体属性。");

                var currentObject = childView.Parent.Current;
                if (currentObject != null)
                {
                    //设置实体引用Id，由于这两个属性很可能在导航项中，所以可能发生导航查询。
                    TrySetPropertyValue(currentObject, naviProperty.RefEntityProperty, selectedEntity);
                    TrySetPropertyValue(currentObject, naviProperty.PropertyName, selectedEntity.Id);
                }
            }
        }

        #endregion

        #region 帮助方法

        /// <summary>
        /// 尝试设置属性。
        /// 
        /// 这里需要尝试的原因是，有时实体不一定定义了所有的属性。
        /// 例如，实体定义了选择后设置的id属性，但是却没有定义对应的引用实体属性。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        private static void TrySetPropertyValue(object obj, string propertyName, object value)
        {
            PropertyInfo p = obj.GetType().GetProperty(propertyName);
            if (p != null && p.CanWrite)
            {
                p.SetValue(obj, value, null);
            }
        }

        #endregion

        #region public class NavigateProperty

        /// <summary>
        /// 导航属性
        /// 
        /// 可以是如下两种形式：
        /// Property（一般的属性）
        /// IdProperty + EntityProperty（实体引用属性）
        /// </summary>
        public class NavigateProperty
        {
            #region 字段

            private string _property;

            private string _refEntityProperty;

            #endregion

            #region 工厂

            private NavigateProperty() { }

            /// <summary>
            /// 把一个一般的属性转换为导航属性
            /// </summary>
            /// <param name="property"></param>
            /// <returns></returns>
            internal static NavigateProperty Convert(EntityPropertyViewMeta propertyInfo)
            {
                var optional = propertyInfo.PropertyMeta.Runtime.PropertyType.GUID == typeof(Nullable<>).GUID;

                var result = new NavigateProperty();
                result._property = propertyInfo.Name;
                result.Optional = optional;
                result.PropertyMeta = propertyInfo;
                return result;
            }

            ///// <summary>
            ///// 把一个关联的孩子属性转换为导航属性
            ///// </summary>
            ///// <param name="property"></param>
            ///// <returns></returns>
            //internal static NavigateProperty Convert(ChildBlock childBlock)
            //{
            //    //实体属性名就是属性的类型名
            //    var entityProperty = PropertyConvention.GetRefEntityPropertyName(childBlock.EntityType);
            //    var idProperty = childBlock.ChildrePropertyMeta.NavigationMeta.IdPropertyAfterSelection;
            //    if (string.IsNullOrEmpty(idProperty))
            //    {
            //        //对象名+Id
            //        idProperty = entityProperty + DBConvention.FieldName_Id;
            //    }

            //    return new NavigateProperty
            //    {
            //        _property = idProperty,
            //        _refEntityProperty = entityProperty,
            //        Optional = true,
            //        PropertyMeta = childBlock,
            //    };
            //}

            #endregion

            /// <summary>
            /// 属性名
            /// </summary>
            public string PropertyName { get { return this._property; } }

            /// <summary>
            /// 如果这个值为真，表示这是一个实体引用的id属性。
            /// 此时，EntityProperty表示实体引用属性名。
            /// </summary>
            public bool IsReferenceId { get { return !string.IsNullOrEmpty(this._refEntityProperty); } }

            public EntityPropertyViewMeta PropertyMeta { get; private set; }

            public EntityViewMeta OwnerMeta { get { return this.PropertyMeta.Owner; } }

            /// <summary>
            /// 如果IsEntityId为true，此属性表示表示实体引用属性名。
            /// </summary>
            public string RefEntityProperty { get { return this._refEntityProperty; } }

            /// <summary>
            /// true 表明此属性并不是必填的。
            /// </summary>
            public bool Optional { get; private set; }
        }

        #endregion
    }
}