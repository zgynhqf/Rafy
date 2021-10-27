/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171104 11:05
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
using Rafy.ManagedProperty;

namespace Rafy.SystemSettings
{
    public abstract class SystemSettingsEntity : LongEntity
    {
    }

    public abstract class SystemSettingsEntityList : EntityList { }

    public abstract class SystemSettingsEntityRepository : EntityRepository
    {
        protected SystemSettingsEntityRepository() { }
    }

    [DataProviderFor(typeof(SystemSettingsEntityRepository))]
    public class SystemSettingsEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return SystemSettingsPlugin.DbSettingName; }
        }
    }

    public abstract class SystemSettingsEntityConfig<TEntity> : EntityConfig<TEntity> { }
}