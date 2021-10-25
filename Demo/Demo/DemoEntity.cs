using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace Demo
{
    public abstract class DemoEntity : IntEntity
    {
    }

    public abstract class DemoEntityList : EntityList { }

    public abstract class DemoEntityRepository : EntityRepository
    {
        public static string DbSettingName = "Demo";

        protected DemoEntityRepository() { }
    }

    [DataProviderFor(typeof(DemoEntityRepository))]
    public class DemoEntityDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return DemoEntityRepository.DbSettingName; }
        }
    }

    public abstract class DemoEntityConfig<TEntity> : EntityConfig<TEntity> { }
    public abstract class DemoEntityWPFViewConfig<TEntity> : WPFViewConfig<TEntity> { }
    public abstract class DemoEntityWebViewConfig<TEntity> : WebViewConfig<TEntity> { }
}