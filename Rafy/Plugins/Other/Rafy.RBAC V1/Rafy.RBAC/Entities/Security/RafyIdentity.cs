using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using Rafy.Library;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

using Rafy.ORM;
using Rafy.Serialization.Mobile;
using Rafy.Utils;
using SimpleCsla;
using SimpleCsla.Data;
using SimpleCsla.Security;

namespace Rafy.RBAC.Security
{
    /// <summary>
    /// 注意：防止重名，User增加Code区分唯一性，查询时通过Code查询，同时返回Code和Name
    /// </summary>
    [Serializable]
    public class RafyIdentity : CslaIdentity, IUser
    {
        public User User { get; set; }

        private OrgPositions _roles;
        public new OrgPositions Roles
        {
            get { return this._roles; }
        }

        #region  Factory Methods

        internal static RafyIdentity GetIdentity(string username, string password)
        {
            return DataPortal.Fetch<RafyIdentity>(new UsernameCriteria(username, password));
        }

        internal static RafyIdentity GetIdentity(int id)
        {
            return DataPortal.Fetch<RafyIdentity>(id);
        }

        private RafyIdentity() { }

        #endregion

        #region  Data Access

        protected void DataPortal_Fetch(int id)
        {
            this.User = RF.Concreate<UserRepository>().GetById(id) as User;

            this.LoadChildrenProperties();
        }

        protected void DataPortal_Fetch(UsernameCriteria criteria)
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

        public static RafyIdentity Current
        {
            get { return ApplicationContext.User.Identity as RafyIdentity; }
        }
    }
}
