/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161212 17:04
 * 
*******************************************************/

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
    public abstract class GroupManagementEntity : LongEntity
    {
    }

    public abstract class GroupManagementEntityList : InheritableEntityList { }

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