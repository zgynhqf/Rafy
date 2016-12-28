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

namespace Rafy.RBAC.DataPermissionManagement
{
    [Serializable]
    public abstract class DataPermissionManagementEntity : LongEntity
    {
        #region 构造函数

        protected DataPermissionManagementEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataPermissionManagementEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class DataPermissionManagementEntityList : EntityList { }

    public abstract class DataPermissionManagementEntityRepository : EntityRepository
    {
        protected DataPermissionManagementEntityRepository() { }
    }

    [DataProviderFor(typeof(DataPermissionManagementEntityRepository))]
    public class DataPermissionManagementEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return DataPermissionManagementPlugin.DbSettingName; }
        }
    }

    public abstract class DataPermissionManagementEntityConfig<TEntity> : EntityConfig<TEntity> { }
}