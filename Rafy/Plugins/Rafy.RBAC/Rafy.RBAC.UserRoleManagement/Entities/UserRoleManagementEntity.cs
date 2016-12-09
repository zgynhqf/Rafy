/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161201
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161201 10:20
 * 创建文件 宋军瑞 20161209 09:50 格式化代码
 * 
*******************************************************/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

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