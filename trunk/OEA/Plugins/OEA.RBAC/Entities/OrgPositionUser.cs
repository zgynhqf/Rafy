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
using OEA.ManagedProperty;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.MetaModel.Attributes;
using OEA.ORM;
using SimpleCsla;
using SimpleCsla.Core;
using SimpleCsla.Data;
using OEA.Web;
using OEA.Library;

namespace OEA.RBAC
{
    [ChildEntity, Serializable]
    public class OrgPositionUser : Entity
    {
        protected OrgPositionUser() { }

        public static readonly RefProperty<OrgPosition> OrgPositionRefProperty =
            P<OrgPositionUser>.RegisterRef(e => e.OrgPosition, ReferenceType.Parent);
        public int OrgPositionId
        {
            get { return this.GetRefId(OrgPositionRefProperty); }
            set { this.SetRefId(OrgPositionRefProperty, value); }
        }
        public OrgPosition OrgPosition
        {
            get { return this.GetRefEntity(OrgPositionRefProperty); }
            set { this.SetRefEntity(OrgPositionRefProperty, value); }
        }

        public static readonly RefProperty<User> UserRefProperty =
            P<OrgPositionUser>.RegisterRef(e => e.User, ReferenceType.Normal);
        public int UserId
        {
            get { return this.GetRefId(UserRefProperty); }
            set { this.SetRefId(UserRefProperty, value); }
        }
        public User User
        {
            get { return this.GetRefEntity(UserRefProperty); }
            set { this.SetRefEntity(UserRefProperty, value); }
        }

        #region 视图属性

        public static readonly Property<string> View_CodeProperty = P<OrgPositionUser>.RegisterReadOnly(e => e.View_Code, e => (e as OrgPositionUser).GetView_Code(), null);
        public string View_Code
        {
            get { return this.GetProperty(View_CodeProperty); }
        }
        private string GetView_Code()
        {
            return User.Code;
        }

        public static readonly Property<string> View_NameProperty = P<OrgPositionUser>.RegisterReadOnly(e => e.View_Name, e => (e as OrgPositionUser).GetView_Name(), null);
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

    public class OrgPositionUserRepository : EntityRepository
    {
        protected OrgPositionUserRepository() { }
    }

    internal class OrgPositionUserConfig : EntityConfig<OrgPositionUser>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            this.Meta.MapTable().HasColumns(
                OrgPositionUser.OrgPositionRefProperty,
                OrgPositionUser.UserRefProperty
                );
        }

        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasLabel("岗位用户").NotAllowEdit();

            if (OEAEnvironment.IsWeb)
            {
                View.UseWebCommand("LookupSelectAddOrgPositionUser")
                    .HasLabel("选择用户")
                    .SetCustomParams("targetClass", ClientEntities.GetClientName(typeof(User)));

                View.RemoveWebCommands(WebCommandNames.Add);
            }
            else
            {
                View.RemoveWPFCommands(WPFCommandNames.Add, WPFCommandNames.Edit)
                    .UseWPFCommands("RBAC.ChooseUserCommand");
            }

            View.Property(OrgPositionUser.View_CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.List);
            View.Property(OrgPositionUser.View_NameProperty).HasLabel("名称").ShowIn(ShowInWhere.List);
        }
    }
}