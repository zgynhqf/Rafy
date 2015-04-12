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
    [Serializable]
    public abstract class DemoEntity : IntEntity
    {
        #region 构造函数

        protected DemoEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DemoEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class DemoEntityList : EntityList { }

    public abstract class DemoEntityRepository : PropertyQueryRepository
    {
        public static string DbSettingName = "Demo";

        protected DemoEntityRepository() { }

        internal EntityList QueryList(Action<IPropertyQuery> queryBuider, Predicate<Entity> filter = null, EntityList entityList = null)
        {
            var query = this.CreatePropertyQuery();
            if (queryBuider != null) queryBuider(query);
            return this.QueryList(new PropertyQueryArgs
            {
                PropertyQuery = query,
                Filter = filter,
                EntityList = entityList
            });
        }
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