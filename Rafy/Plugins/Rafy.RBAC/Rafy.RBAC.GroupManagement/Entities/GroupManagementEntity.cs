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

namespace Rafy.RBAC.GroupManagement
{
    [Serializable]
    public abstract class GroupManagementEntity : LongEntity
    {
        #region 构造函数

        protected GroupManagementEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GroupManagementEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class GroupManagementEntityList : EntityList { }

    public abstract class GroupManagementEntityRepository : EntityRepository
    {
        protected GroupManagementEntityRepository() { }
    }

    [DataProviderFor(typeof(GroupManagementEntityRepository))]
    public class GroupManagementEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return GroupManagementPlugin.DbSettingName; }
        }
    }

    public abstract class GroupManagementEntityConfig<TEntity> : EntityConfig<TEntity> { }
}