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
using Rafy.Domain.ORM.Query;

namespace UT
{
    public abstract class UnitTestEntity : IntEntity { }

    public abstract class UnitTestEntityList : InheritableEntityList { }

    public abstract class UnitTestEntityRepository : EntityRepository//PropertyQueryRepository
    {
        protected UnitTestEntityRepository() { }

        protected QueryFactory f
        {
            get { return QueryFactory.Instance; }
        }

        //internal IEntityList QueryList(Action<IPropertyQuery> queryBuider, Predicate<Entity> filter = null, IEntityList entityList = null)
        //{
        //    var query = this.CreatePropertyQuery();
        //    if (queryBuider != null) queryBuider(query);
        //    return this.QueryList(new PropertyQueryArgs
        //    {
        //        PropertyQuery = query,
        //        Filter = filter,
        //        EntityList = entityList
        //    });
        //}
    }

    [DataProviderFor(typeof(UnitTestEntityRepository))]
    public class UnitTestEntityRepositoryDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "Test_RafyUnitTest";
        public static string DbSettingName_Duplicate = "Test_RafyUnitTest_Duplicate";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class UnitTestEntityConfig<TEntity> : EntityConfig<TEntity> { }
}