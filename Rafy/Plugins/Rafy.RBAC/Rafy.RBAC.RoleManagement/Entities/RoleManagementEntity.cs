using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

namespace Rafy.RBAC.RoleManagement
{
    [Serializable]
    public abstract class RoleManagementEntity : LongEntity
    {
        #region 构造函数

        protected RoleManagementEntity()
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected RoleManagementEntity(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        #endregion
    }

    [Serializable]
    public abstract class RoleManagementEntityList : EntityList
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