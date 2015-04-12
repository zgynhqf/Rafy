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

namespace $domainNamespace$
{
    [Serializable]
    public abstract class $domainName$Entity : IntEntity
    {
        #region 构造函数

        protected $domainName$Entity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected $domainName$Entity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    public abstract class $domainName$EntityList : EntityList { }

    public abstract class $domainName$EntityRepository : EntityRepository
    {
        protected $domainName$EntityRepository() { }
    }

    [DataProviderFor(typeof($domainName$EntityRepository))]
    public class $domainName$EntityRepositoryDataProvider : RdbDataProvider
    {
        public static readonly string DbSettingName = "$domainName$";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class $domainName$EntityConfig<TEntity> : EntityConfig<TEntity> { }
}