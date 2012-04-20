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
using System.Text;
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using System.ComponentModel;

namespace JXC
{
    [Serializable]
    public abstract class ProductRefItem : JXCEntity
    {
        public static readonly RefProperty<Product> ProductRefProperty =
            P<ProductRefItem>.RegisterRef(e => e.Product, ReferenceType.Normal);
        public int ProductId
        {
            get { return this.GetRefId(ProductRefProperty); }
            set { this.SetRefId(ProductRefProperty, value); }
        }
        public Product Product
        {
            get { return this.GetRefEntity(ProductRefProperty); }
            set { this.SetRefEntity(ProductRefProperty, value); }
        }

        public static readonly Property<int> AmountProperty = P<ProductRefItem>.Register(e => e.Amount, new PropertyMetadata<int>
        {
            PropertyChangedCallBack = (o, e) => (o as ProductRefItem).OnAmountChanged(e)
        });
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }
        protected virtual void OnAmountChanged(ManagedPropertyChangedEventArgs<int> e) { }

        #region 视图属性

        public static readonly Property<string> View_ProductNameProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_ProductName, e => (e as ProductRefItem).GetView_ProductName(), null);
        public string View_ProductName
        {
            get { return this.GetProperty(View_ProductNameProperty); }
        }
        private string GetView_ProductName()
        {
            return this.Product.MingCheng;
        }

        public static readonly Property<string> View_ProductCategoryNameProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_ProductCategoryName, e => (e as ProductRefItem).GetView_ProductCategoryName(), null);
        public string View_ProductCategoryName
        {
            get { return this.GetProperty(View_ProductCategoryNameProperty); }
        }
        private string GetView_ProductCategoryName()
        {
            return this.Product.ProductCategory.Name;
        }

        public static readonly Property<string> View_SpecificationProperty = P<ProductRefItem>.RegisterReadOnly(e => e.View_Specification, e => (e as ProductRefItem).GetView_Specification(), null);
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
        protected override void OnListChanged(ListChangedEventArgs e)
        {
            base.OnListChanged(e);

            this.RaiseRoutedEvent(ListChangedEvent, e);
        }

        public static readonly EntityRoutedEvent ListChangedEvent = EntityRoutedEvent.Register(EntityRoutedEventType.BubbleToParent);
    }
}