/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120419
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120419
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using System.ComponentModel;
using System.Collections.Specialized;

namespace JXC
{
    [Serializable]
    public abstract class ProductRefItem : JXCEntity
    {
        public static readonly IRefIdProperty ProductIdProperty =
            P<ProductRefItem>.RegisterRefId(e => e.ProductId, ReferenceType.Normal);
        public int ProductId
        {
            get { return (int)this.GetRefId(ProductIdProperty); }
            set { this.SetRefId(ProductIdProperty, value); }
        }
        public static readonly RefEntityProperty<Product> ProductProperty =
            P<ProductRefItem>.RegisterRef(e => e.Product, ProductIdProperty);
        public Product Product
        {
            get { return this.GetRefEntity(ProductProperty); }
            set { this.SetRefEntity(ProductProperty, value); }
        }

        public static readonly Property<int> AmountProperty = P<ProductRefItem>.Register(e => e.Amount, new PropertyMetadata<int>
        {
            PropertyChangingCallBack = (o, e) => (o as ProductRefItem).OnAmountChanging(e),
            PropertyChangedCallBack = (o, e) => (o as ProductRefItem).OnAmountChanged(e)
        });
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }
        protected virtual void OnAmountChanging(ManagedPropertyChangingEventArgs<int> e) { }
        protected virtual void OnAmountChanged(ManagedPropertyChangedEventArgs e) { }

        #region 视图属性

        public static readonly Property<string> View_ProductNameProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as ProductRefItem).GetView_ProductName());
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }

        public static readonly Property<string> View_ProductCategoryNameProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_ProductCategoryName, e => (e as ProductRefItem).GetView_ProductCategoryName());
        public string View_ProductCategoryName
        {
            get { return this.GetProperty(View_ProductCategoryNameProperty); }
        }
        private string GetView_ProductCategoryName()
        {
            return this.Product.ProductCategory.Name;
        }

        public static readonly Property<string> View_SpecificationProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_Specification, e => (e as ProductRefItem).GetView_Specification());
        public string View_Specification
        {
            get { return this.GetProperty(View_SpecificationProperty); }
        }
        private string GetView_Specification()
        {
            return this.Product.GuiGe;
        }

        #endregion
    }

    [Serializable]
    public class ProductRefItemList : JXCEntityList
    {
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            this.RaiseRoutedEvent(ListChangedEvent, e);
        }

        public static readonly EntityRoutedEvent ListChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);
    }
}