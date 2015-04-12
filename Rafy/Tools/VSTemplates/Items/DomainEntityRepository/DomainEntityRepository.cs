using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using $domainNamespace$;

namespace $rootnamespace$
{
    /// <summary>
    /// $domainEntityName$ 仓库类。
    /// 负责 $domainEntityName$ 类的查询、保存。
    /// </summary>
    [RepositoryFor(typeof($domainEntityName$))]
    public partial class $domainEntityName$Repository : $baseRepositoryName$
    {
        /// <summary>
        /// 单例模式，由框架中的工厂构建。外界不可以直接构造本对象。
        /// </summary>
        protected $domainEntityName$Repository() { }
    }
}