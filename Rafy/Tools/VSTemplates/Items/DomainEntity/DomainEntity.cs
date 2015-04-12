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
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace $domainNamespace$
{
    /// <summary>
    /// $domainEntityLabel$
    /// </summary>
    $entityAttributes$
    public partial class $domainEntityName$ : $domainBaseEntityName$
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

    /// <summary>
    /// $domainEntityLabel$ 列表类。
    /// </summary>
    [Serializable]
    public partial class $domainEntityName$List : $domainBaseEntityName$List { }$repositoryCode$

    /// <summary>
    /// $domainEntityLabel$ 配置类。
    /// 负责 $domainEntityLabel$ 类的实体元数据的配置。
    /// </summary>
    internal class $domainEntityName$Config : $domainBaseEntityName$Config<$domainEntityName$>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            $tableConfig$$columnConfig$
        }$viewConfiguration$
    }
}