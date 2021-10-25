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

namespace JXC
{
    /// <summary>
    /// 产品类别
    /// </summary>
    [RootEntity, Serializable]
    public partial class ProductCategory : JXCEntity
    {
        public static readonly Property<string> NameProperty = P<ProductCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #region 性能测试代码

        //static ProductCategory()
        //{
        //    for (int i = 0; i < 100; i++)
        //    {
        //        P<ProductCategory>.RegisterExtension("Name" + i, typeof(ProductCategory), "默认数据");
        //    }
        //}

        //        for (int i = 0; i < 100; i++)
        //        {
        //            View.Property("Name" + i).ShowIn(ShowInWhere.List);
        //        }

        #endregion
    }

    [Serializable]
    public partial class ProductCategoryList : JXCEntityList { }

    public partial class ProductCategoryRepository : JXCEntityRepository
    {
        protected ProductCategoryRepository() { }
    }

    internal class ProductCategoryConfig : EntityConfig<ProductCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().MapProperties(
                ProductCategory.NameProperty
                );

            Meta.EnableClientCache();
        }
    }
}