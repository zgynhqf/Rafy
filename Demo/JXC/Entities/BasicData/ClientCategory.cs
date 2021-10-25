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
using Rafy.Domain.ORM;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    /// <summary>
    /// 客户类别
    /// </summary>
    [RootEntity, Serializable]
    public partial class ClientCategory : JXCEntity
    {
        public static readonly string SupplierName = "供应商";

        public static readonly string CustomerName = "客户";

        public static readonly Property<string> NameProperty = P<ClientCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public partial class ClientCategoryList : JXCEntityList { }

    public partial class ClientCategoryRepository : JXCEntityRepository
    {
        protected ClientCategoryRepository() { }
    }

    [DataProviderFor(typeof(ClientCategoryRepository))]
    public partial class ClientCategoryDataProvider : JXCEntityDataProvider
    {
        protected override void Submit(SubmitArgs e)
        {
            if (e.Action == SubmitAction.Delete)
            {
                var entity = e.Entity as ClientCategory;
                if (entity.Name == ClientCategory.SupplierName || entity.Name == ClientCategory.CustomerName)
                {
                    throw new InvalidOperationException("不能删除系统内置的客户类型：" + entity.Name);
                }
            }

            base.Submit(e);
        }
    }

    internal class ClientCategoryConfig : EntityConfig<ClientCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache();
        }
    }
}