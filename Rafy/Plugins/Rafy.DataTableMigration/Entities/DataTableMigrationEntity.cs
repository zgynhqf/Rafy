/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:48
 * 
*******************************************************/

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.MetaModel;

namespace Rafy.DataTableMigration
{
    [Serializable]
    public abstract class DataTableMigrationEntity : LongEntity
    {
        #region 构造函数

        protected DataTableMigrationEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataTableMigrationEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class DataTableMigrationEntityList : EntityList { }

    public abstract class DataTableMigrationEntityRepository : EntityRepository
    {
        protected DataTableMigrationEntityRepository() { }
    }

    [DataProviderFor(typeof(DataTableMigrationEntityRepository))]
    public class DataTableMigrationEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return DataTableMigrationPlugin.DbSettingName; }
        }
    }

    public abstract class DataTableMigrationEntityConfig<TEntity> : EntityConfig<TEntity> { }
}