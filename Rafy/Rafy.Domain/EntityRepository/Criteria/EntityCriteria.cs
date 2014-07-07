/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100920
 * 说明：一些实体类通用的条件类。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100920
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain
{
    [Serializable]
    class GetByIdCriteria : Criteria
    {
        #region 构造函数

        public GetByIdCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetByIdCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public object IdValue { get; set; }
    }

    /// <summary>
    /// 本类中的 Id 即是 ParentId 的值。
    /// </summary>
    [Serializable]
    class GetByParentIdCriteria : Criteria
    {
        #region 构造函数

        public GetByParentIdCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetByParentIdCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public object ParentId { get; set; }
    }

    [Serializable]
    class GetByParentIdListCriteria : Criteria
    {
        #region 构造函数

        public GetByParentIdListCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetByParentIdListCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public object[] ParentIdList { get; set; }
    }

    [Serializable]
    class GetAllCriteria : Criteria
    {
        #region 构造函数

        public GetAllCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetAllCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    class CountAllCriteria : Criteria
    {
        #region 构造函数

        public CountAllCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected CountAllCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    class GetByTreeParentIndexCriteria : Criteria
    {
        #region 构造函数

        public GetByTreeParentIndexCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetByTreeParentIndexCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion
    }

    [Serializable]
    class GetByIdListCriteria : Criteria
    {
        #region 构造函数

        public GetByIdListCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected GetByIdListCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public object[] IdList { get; set; }
    }

    [Serializable]
    class GetEntityValueCriteria
    {
        public object EntityId { get; set; }
        public string Property { get; set; }
    }
}