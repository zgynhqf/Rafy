/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120413
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace JXC
{
    /// <summary>
    /// 仓库
    /// </summary>
    [RootEntity, Serializable]
    public partial class Storage : JXCEntity
    {
        public static readonly ListProperty<StorageProductList> StorageProductListProperty = P<Storage>.RegisterList(e => e.StorageProductList);
        public StorageProductList StorageProductList
        {
            get { return this.GetLazyList(StorageProductListProperty); }
        }

        public static readonly Property<string> CodeProperty = P<Storage>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<Storage>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> AddressProperty = P<Storage>.Register(e => e.Address);
        public string Address
        {
            get { return this.GetProperty(AddressProperty); }
            set { this.SetProperty(AddressProperty, value); }
        }

        public static readonly Property<string> ResponsiblePersonProperty = P<Storage>.Register(e => e.ResponsiblePerson);
        public string ResponsiblePerson
        {
            get { return this.GetProperty(ResponsiblePersonProperty); }
            set { this.SetProperty(ResponsiblePersonProperty, value); }
        }

        public static readonly Property<string> AreaProperty = P<Storage>.Register(e => e.Area);
        public string Area
        {
            get { return this.GetProperty(AreaProperty); }
            set { this.SetProperty(AreaProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<Storage>.Register(e => e.IsDefault, new PropertyMetadata<bool>
        {
            PropertyChangedCallBack = (o, e) => (o as Storage).OnIsDefaultChanged(e)
        });
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }
        protected virtual void OnIsDefaultChanged(ManagedPropertyChangedEventArgs e)
        {
            //整个列表中只有一个默认仓库。
            if ((bool)e.NewValue && RafyPropertyDescriptor.IsOperating)
            {
                var list = (this as IEntity).ParentList;
                if (list != null)
                {
                    foreach (Storage item in list)
                    {
                        if (item != this)
                        {
                            item.IsDefault = false;
                        }
                    }
                }
            }
        }

        public static readonly Property<int> TotalAmountProperty = P<Storage>.RegisterReadOnly(e => e.TotalAmount, e => (e as Storage).GetTotalAmount());
        public int TotalAmount
        {
            get { return this.GetProperty(TotalAmountProperty); }
        }
        private int GetTotalAmount()
        {
            return this.StorageProductList.Cast<StorageProduct>().Sum(sp => sp.Amount);
        }

        /// <summary>
        /// 找到某个商品在这个仓库中的库存项
        /// 
        /// 如果不存在，则创建一个对应项。
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public StorageProduct FindOrCreateItem(Product product)
        {
            var children = this.StorageProductList;
            var item = children.Cast<StorageProduct>().FirstOrDefault(e => e.ProductId == product.Id);
            if (item != null) { return item; }

            item = new StorageProduct
            {
                Product = product
            };

            children.Add(item);

            return item;
        }

        /// <summary>
        /// 找到某个商品在这个仓库中的库存项
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public StorageProduct FindItem(Product product)
        {
            var children = this.StorageProductList;
            var item = children.Cast<StorageProduct>().FirstOrDefault(e => e.ProductId == product.Id);
            return item;
        }
    }

    [Serializable]
    public partial class StorageList : JXCEntityList { }

    public partial class StorageRepository : JXCEntityRepository
    {
        protected StorageRepository() { }

        public Storage GetDefault()
        {
            //有缓存，直接调用全部的列表
            return this.CacheAll().Concrete().FirstOrDefault(s => s.IsDefault);
        }
    }

    internal class StorageConfig : EntityConfig<Storage>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache();
        }
    }
}