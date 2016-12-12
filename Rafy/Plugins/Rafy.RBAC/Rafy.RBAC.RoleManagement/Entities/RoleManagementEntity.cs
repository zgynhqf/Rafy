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
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace Rafy.RBAC.RoleManagement
{
    [Serializable]
    public abstract class RoleManagementEntity : LongEntity
    {
        #region 构造函数

        protected RoleManagementEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected RoleManagementEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class RoleManagementEntityList : EntityList { }

    public abstract class RoleManagementEntityRepository : EntityRepository
    {
        protected RoleManagementEntityRepository() { }
    }

    [DataProviderFor(typeof(RoleManagementEntityRepository))]
    public class RoleManagementEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return RoleManagementPlugin.DbSettingName; }
        }
    }

    public abstract class RoleManagementEntityConfig<TEntity> : EntityConfig<TEntity> { }
}