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

namespace Rafy.RBAC.UserRoleManagement
{
    [Serializable]
    public abstract class UserRoleManagementEntity : LongEntity
    {
        #region 构造函数

        protected UserRoleManagementEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected UserRoleManagementEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class UserRoleManagementEntityList : EntityList { }

    public abstract class UserRoleManagementEntityRepository : EntityRepository
    {
        protected UserRoleManagementEntityRepository() { }
    }

    [DataProviderFor(typeof(UserRoleManagementEntityRepository))]
    public class UserRoleManagementEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return UserRoleManagementPlugin.DbSettingName; }
        }
    }

    public abstract class UserRoleManagementEntityConfig<TEntity> : EntityConfig<TEntity> { }
}