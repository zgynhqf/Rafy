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

namespace UT
{
    public abstract class UnitTest2Entity : IntEntity { }

    public abstract class UnitTest2EntityList : EntityList { }

    public abstract class UnitTest2EntityRepository : EntityRepository
    {
        protected UnitTest2EntityRepository() { }
    }

    [DataProviderFor(typeof(UnitTest2EntityRepository))]
    public class UnitTest2EntityRepositoryDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "Test_RafyUnitTest2";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class UnitTest2EntityConfig<TEntity> : EntityConfig<TEntity> { }
}