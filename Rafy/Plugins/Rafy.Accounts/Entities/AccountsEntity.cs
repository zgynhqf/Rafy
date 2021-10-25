/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151209
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151209 15:13
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

namespace Rafy.Accounts
{
    public abstract class AccountsEntity : LongEntity
    {
        //#region 动态提供 AccountsEntity 的主键的类型

        //static AccountsEntity()
        //{
        //    IdProperty.OverrideMeta(typeof(AccountsEntity), new PropertyMetadata<object>(), m =>
        //    {
        //        m.DefaultValue = AccountsPlugin.KeyProvider.DefaultValue;
        //    });
        //}

        //public override IKeyProvider KeyProvider
        //{
        //    get { return AccountsPlugin.KeyProvider; }
        //}

        //#endregion
    }

    public abstract class AccountsEntityList : EntityList { }

    public abstract class AccountsEntityRepository : EntityRepository
    {
        protected AccountsEntityRepository() { }
    }

    [DataProviderFor(typeof(AccountsEntityRepository))]
    public class AccountsEntityRepositoryDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return AccountsPlugin.DbSettingName; }
        }
    }

    public abstract class AccountsEntityConfig<TEntity> : EntityConfig<TEntity> { }
}