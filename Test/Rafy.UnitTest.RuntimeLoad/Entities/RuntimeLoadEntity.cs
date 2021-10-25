using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Rafy.UnitTest.RuntimeLoad
{
    public abstract class RuntimeLoadEntity : LongEntity { }

    public abstract class RuntimeLoadEntityList : EntityList { }

    public abstract class RuntimeLoadEntityRepository : EntityRepository
    {
        protected RuntimeLoadEntityRepository() { }
    }

    [DataProviderFor(typeof(RuntimeLoadEntityRepository))]
    public class RuntimeLoadEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return RuntimeLoadPlugin.DbSettingName; }
        }
    }

    public abstract class RuntimeLoadEntityConfig<TEntity> : EntityConfig<TEntity> { }
}