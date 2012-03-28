using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla.Security;
using System.Security.Principal;
using SimpleCsla;

namespace OEA.RBAC.Security
{
    [Serializable]
    public class OEAPrincipal : BusinessPrincipalBase
    {
        /// <summary>
        /// 由于DataportalContext使用了Principel，所以每次都传输OEAIdentity对象，
        /// 内部的数据要尽量简单，不可以序列化此字段。
        /// </summary>
        [NonSerialized]
        private OEAIdentity _realIdentity;

        private OEAPrincipal(IIdentity identity, OEAIdentity realIdentity)
            : base(identity)
        {
            _realIdentity = realIdentity;
        }

        public override IIdentity Identity
        {
            get
            {
                if (this._realIdentity == null)
                {
                    var id = (base.Identity as IdentityId).UserId;
                    this._realIdentity = OEAIdentity.GetIdentity(id);
                }

                return this._realIdentity;
            }
        }

        public static bool Login(string username, string password)
        {
            var identity = OEAIdentity.GetIdentity(username, password);
            if (identity.IsAuthenticated)
            {
                ApplicationContext.User = new OEAPrincipal(new IdentityId()
                {
                    UserId = identity.User.Id
                }, identity);
                return true;
            }
            else
            {
                ApplicationContext.User = new UnauthenticatedPrincipal();
                return false;
            }
        }

        public static void Logout()
        {
            ApplicationContext.User = new UnauthenticatedPrincipal();
        }

        [Serializable]
        private class IdentityId : IIdentity
        {
            public int UserId;

            string IIdentity.AuthenticationType
            {
                get { throw new NotImplementedException(); }
            }
            bool IIdentity.IsAuthenticated
            {
                get { throw new NotImplementedException(); }
            }
            string IIdentity.Name
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
