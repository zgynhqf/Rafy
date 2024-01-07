using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.ManagedProperty;

namespace $domainNamespace$
{
    public abstract class $domainName$Entity : LongEntity { }

    public abstract class $domainName$EntityList<TEntity> : EntityList<TEntity> where TEntity : Entity { }

    public abstract class $domainName$EntityRepository : EntityRepository
    {
        protected $domainName$EntityRepository() { }
    }

    [DataProviderFor(typeof($domainName$EntityRepository))]
    public class $domainName$EntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return $domainName$Plugin.DbSettingName; }
        }
    }

    public abstract class $domainName$EntityConfig<TEntity> : EntityConfig<TEntity> { }
}