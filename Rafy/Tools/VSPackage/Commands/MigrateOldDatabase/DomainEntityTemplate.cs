using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace $domainNamespace$
{
    /// <summary>
    /// 
    /// </summary>
    [RootEntity, Serializable]
    public class $domainEntityName$ : $domainBaseEntityName$
    {
        #region 构造函数

        public $domainEntityName$() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected $domainEntityName$(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性
        $refProperties$
        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性
        $normalProperties$
        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public class $domainEntityName$List : $domainBaseEntityName$List { }

    public class $domainEntityName$Repository : $domainBaseEntityName$Repository
    {
        protected $domainEntityName$Repository() { }
    }

    internal class $domainEntityName$Config : $domainBaseEntityName$Config<$domainEntityName$>
    {
        protected override void ConfigMeta()
        {
            $tableConfig$$columnConfig$
        }
    }
}