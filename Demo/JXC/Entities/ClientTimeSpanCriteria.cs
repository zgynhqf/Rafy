/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    [QueryEntity, Serializable]
    public class ClientTimeSpanCriteria : TimeSpanCriteria
    {
        #region 构造函数

        public ClientTimeSpanCriteria() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ClientTimeSpanCriteria(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty ClientInfoIdProperty =
            P<ClientTimeSpanCriteria>.RegisterRefId(e => e.ClientInfoId, ReferenceType.Normal);
        public int? ClientInfoId
        {
            get { return (int?)this.GetRefNullableId(ClientInfoIdProperty); }
            set { this.SetRefNullableId(ClientInfoIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientInfo> ClientInfoProperty =
            P<ClientTimeSpanCriteria>.RegisterRef(e => e.ClientInfo, ClientInfoIdProperty);
        public ClientInfo ClientInfo
        {
            get { return this.GetRefEntity(ClientInfoProperty); }
            set { this.SetRefEntity(ClientInfoProperty, value); }
        }
    }
}