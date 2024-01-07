using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

namespace Rafy.RBAC.RoleManagement
{
    public abstract class RoleManagementEntity : LongEntity
    {
    }

    public abstract class RoleManagementEntityList : InheritableEntityList
    {
    }

    public abstract class RoleManagementEntityRepository : EntityRepository
    {
    }

    [DataProviderFor(typeof (RoleManagementEntityRepository))]
    public class RoleManagementEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return RoleManagementPlugin.DbSettingName; }
        }
    }

    public abstract class RoleManagementEntityConfig<TEntity> : EntityConfig<TEntity>
    {
    }
}