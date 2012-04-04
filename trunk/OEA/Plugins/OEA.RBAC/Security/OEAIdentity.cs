using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;

using OEA.ORM;
using OEA.Serialization.Mobile;
using OEA.Utils;
using OEA;

namespace OEA.RBAC.Security
{
    /// <summary>
    /// 注意：防止重名，User增加Code区分唯一性，查询时通过Code查询，同时返回Code和Name
    /// </summary>
    [Serializable]
    public class OEAIdentity : Entity, IUser
    {
        public User User { get; set; }

        private OrgPositionList _roles;
        public OrgPositionList Roles
        {
            get { return this._roles; }
        }

        public static readonly Property<string> AuthenticationTypeProperty = P<OEAIdentity>.Register(e => e.AuthenticationType);
        public string AuthenticationType
        {
            get { return this.GetProperty(AuthenticationTypeProperty); }
            set { this.SetProperty(AuthenticationTypeProperty, value); }
        }

        public static readonly Property<bool> IsAuthenticatedProperty = P<OEAIdentity>.Register(e => e.IsAuthenticated);
        public bool IsAuthenticated
        {
            get { return this.GetProperty(IsAuthenticatedProperty); }
            set { this.SetProperty(IsAuthenticatedProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<OEAIdentity>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        internal static OEAIdentity GetIdentity(string username, string password)
        {
            var c = new UsernameCriteria()
            {
                Username = username,
                Password = password
            };

            return DataPortal.Fetch(typeof(OEAIdentity), c) as OEAIdentity;
        }

        internal static OEAIdentity GetIdentity(int id)
        {
            return DataPortal.Fetch(typeof(OEAIdentity), id) as OEAIdentity;
        }

        #region  Data Access

        protected void QueryBy(int id)
        {
            this.User = RF.Concreate<UserRepository>().GetById(id) as User;

            this.LoadChildrenProperties();
        }

        protected void QueryBy(UsernameCriteria criteria)
        {
            this.User = RF.Concreate<UserRepository>().GetBy(criteria.Username, criteria.Password);

            this.LoadChildrenProperties();
        }

        private void LoadChildrenProperties()
        {
            if (this.User != null)
            {
                this.Name = this.User.Name;
                this.IsAuthenticated = true;
                this._roles = (RF.Create<OrgPosition>() as OrgPositionRepository).GetList(User.Id); // list of roles from security store
            }
            else
            {
                this.Name = string.Empty;
                this.IsAuthenticated = false;
                this._roles = null;
            }
        }

        #endregion

        public static OEAIdentity Current
        {
            get { return ApplicationContext.User.Identity as OEAIdentity; }
        }
    }
}
