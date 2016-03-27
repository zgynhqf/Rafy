/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110414
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110414
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM;
using Rafy;
using Rafy.Web;
using Rafy.Domain;
using System.Security.Permissions;

namespace Rafy.RBAC.Old
{
    /// <summary>
    /// 部门岗位用户
    /// </summary>
    [ChildEntity, Serializable]
    public partial class OrgPositionUser : IntEntity
    {
        #region 构造函数

        public OrgPositionUser() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected OrgPositionUser(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly IRefIdProperty OrgPositionIdProperty =
            P<OrgPositionUser>.RegisterRefId(e => e.OrgPositionId, ReferenceType.Parent);
        public int OrgPositionId
        {
            get { return (int)this.GetRefId(OrgPositionIdProperty); }
            set { this.SetRefId(OrgPositionIdProperty, value); }
        }
        public static readonly RefEntityProperty<OrgPosition> OrgPositionProperty =
            P<OrgPositionUser>.RegisterRef(e => e.OrgPosition, OrgPositionIdProperty);
        public OrgPosition OrgPosition
        {
            get { return this.GetRefEntity(OrgPositionProperty); }
            set { this.SetRefEntity(OrgPositionProperty, value); }
        }

        public static readonly IRefIdProperty UserIdProperty =
            P<OrgPositionUser>.RegisterRefId(e => e.UserId, ReferenceType.Normal);
        public int UserId
        {
            get { return (int)this.GetRefId(UserIdProperty); }
            set { this.SetRefId(UserIdProperty, value); }
        }
        public static readonly RefEntityProperty<User> UserProperty =
            P<OrgPositionUser>.RegisterRef(e => e.User, UserIdProperty);
        public User User
        {
            get { return this.GetRefEntity(UserProperty); }
            set { this.SetRefEntity(UserProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> View_CodeProperty = P<OrgPositionUser>.RegisterReadOnly(e => e.View_Code, e => (e as OrgPositionUser).GetView_Code());
        public string View_Code
        {
            get { return this.GetProperty(View_CodeProperty); }
        }
        private string GetView_Code()
        {
            return User.Code;
        }

        public static readonly Property<string> View_NameProperty = P<OrgPositionUser>.RegisterReadOnly(e => e.View_Name, e => (e as OrgPositionUser).GetView_Name());
        public string View_Name
        {
            get { return this.GetProperty(View_NameProperty); }
        }
        private string GetView_Name()
        {
            return User.Name;
        }

        #endregion
    }

    [Serializable]
    public partial class OrgPositionUserList : EntityList { }

    public partial class OrgPositionUserRepository : EntityRepository
    {
        protected OrgPositionUserRepository() { }
    }

    internal class OrgPositionUserConfig : EntityConfig<OrgPositionUser>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            this.Meta.MapTable().MapProperties(
                OrgPositionUser.OrgPositionIdProperty,
                OrgPositionUser.UserIdProperty
                );
        }
    }
}