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
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public class ClientCategory : JXCEntity
    {
        public static readonly Property<string> NameProperty = P<ClientCategory>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }
    }

    [Serializable]
    public class ClientCategoryList : JXCEntityList { }

    public class ClientCategoryRepository : EntityRepository
    {
        protected ClientCategoryRepository() { }
    }

    internal class ClientCategoryConfig : EntityConfig<ClientCategory>
    {
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            Meta.MapTable().HasColumns(
                ClientCategory.NameProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.DomainName("客户类别").HasDelegate(ClientCategory.NameProperty);

            View.Property(ClientCategory.TreeCodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All)
                .HasOrderNo(-1).Readonly();
            View.Property(ClientCategory.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}