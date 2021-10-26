/*******************************************************
 * 
 * 作者：周金根
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security.Principal;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM;
using Rafy.Serialization.Mobile;
using Rafy.Utils;

namespace Rafy.RBAC.Old.Security
{
    /// <summary>
    /// 注意：防止重名，User增加Code区分唯一性，查询时通过Code查询，同时返回Code和Name
    /// </summary>
    [RootEntity]
    public partial class RafyIdentity : IntEntity, IRafyIdentity
    {
        public User User { get; internal set; }

        private OrgPositionList _roles;
        public OrgPositionList Roles
        {
            get { return this._roles; }
        }

        public static readonly Property<string> AuthenticationTypeProperty = P<RafyIdentity>.Register(e => e.AuthenticationType);
        public string AuthenticationType
        {
            get { return this.GetProperty(AuthenticationTypeProperty); }
            set { this.SetProperty(AuthenticationTypeProperty, value); }
        }

        public static readonly Property<bool> IsAuthenticatedProperty = P<RafyIdentity>.Register(e => e.IsAuthenticated);
        public bool IsAuthenticated
        {
            get { return this.GetProperty(IsAuthenticatedProperty); }
            set { this.SetProperty(IsAuthenticatedProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<RafyIdentity>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<RafyIdentity>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        internal void LoadChildrenProperties()
        {
            if (this.User != null)
            {
                this.Id = this.User.Id;
                this.Code = this.User.Code;
                this.Name = this.User.Name;
                this.IsAuthenticated = true;
                this._roles = (RF.Find<OrgPosition>() as OrgPositionRepository).GetList(User.Id); // list of roles from security store
            }
            else
            {
                this.Id = 0;
                this.IsAuthenticated = false;
                this._roles = null;
            }
        }
    }

    public partial class RafyIdentityList : EntityList { }

    public partial class RafyIdentityRepository : EntityRepository
    {
        protected RafyIdentityRepository() { }

        internal RafyIdentity GetBy(string username, string password)
        {
            var res = new RafyIdentity
            {
                User = RF.ResolveInstance<UserRepository>().GetBy(username, password)
            };

            res.LoadChildrenProperties();

            this.SetRepo(res);

            return res;
        }

        protected override Entity DoGetById(object id, LoadOptions loadOptions)
        {
            var res = new RafyIdentity
            {
                User = RF.ResolveInstance<UserRepository>().GetById(id) as User
            };

            res.LoadChildrenProperties();

            this.SetRepo(res);

            return res;
        }
    }
}