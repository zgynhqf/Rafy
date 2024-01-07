/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 09:24
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

namespace Rafy.SerialNumber
{
    public abstract class SerialNumberEntity : LongEntity
    {
        //#region 动态提供 SerialNumber 的主键的类型

        //static SerialNumberEntity()
        //{
        //    IdProperty.OverrideMeta(typeof(SerialNumberEntity), new PropertyMetadata<object>(), m =>
        //    {
        //        m.DefaultValue = SerialNumberPlugin.KeyProvider.DefaultValue;
        //    });
        //}

        //public override IKeyProvider KeyProvider
        //{
        //    get { return SerialNumberPlugin.KeyProvider; }
        //}

        //#endregion
    }

    public abstract class SerialNumberEntityList : InheritableEntityList { }

    public abstract class SerialNumberEntityRepository : EntityRepository
    {
        protected SerialNumberEntityRepository() { }
    }

    [DataProviderFor(typeof(SerialNumberEntityRepository))]
    public class SerialNumberEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return SerialNumberPlugin.DbSettingName; }
        }
    }

    public abstract class SerialNumberEntityConfig<TEntity> : EntityConfig<TEntity> { }
}