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
using SimpleCsla;
using SimpleCsla.Data;
using SimpleCsla.Security;

namespace OEA.RBAC.Security
{
    /// <summary>
    /// 注意：防止重名，User增加Code区分唯一性，查询时通过Code查询，同时返回Code和Name
    /// </summary>
    [Serializable]
    public class OEAIdentity : CslaIdentity, IUser
    {
        public User User { get; set; }

        private OrgPositionList _roles;
        public new OrgPositionList Roles
        {
            get { return this._roles; }
        }

        #region  Factory Methods

        internal static OEAIdentity GetIdentity(string username, string password)
        {
            return DataPortal.Fetch<OEAIdentity>(new UsernameCriteria(username, password));
        }

        internal static OEAIdentity GetIdentity(int id)
        {
            return DataPortal.Fetch<OEAIdentity>(id);
        }

        private OEAIdentity() { }

        #endregion

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
                base.Name = this.User.Name;
                base.IsAuthenticated = true;
                this._roles = (RF.Create<OrgPosition>() as OrgPositionRepository).GetList(User.Id); // list of roles from security store

                //var loadThis = this.OrgsHavePermission;
            }
            else
            {
                base.Name = string.Empty;
                base.IsAuthenticated = false;
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
