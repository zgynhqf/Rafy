/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 01:48
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

namespace Rafy.FileStorage
{
    public abstract class FileStorageEntity : LongEntity
    {
    }

    public abstract class FileStorageEntityList : EntityList { }

    public abstract class FileStorageEntityRepository : EntityRepository
    {
        protected FileStorageEntityRepository() { }
    }

    [DataProviderFor(typeof(FileStorageEntityRepository))]
    public class FileStorageEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return FileStoragePlugin.DbSettingName; }
        }
    }

    public abstract class FileStorageEntityConfig<TEntity> : EntityConfig<TEntity> { }
}