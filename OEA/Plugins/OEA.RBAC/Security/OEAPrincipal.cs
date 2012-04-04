using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using OEA;

namespace OEA.RBAC.Security
{
    [Serializable]
    public class OEAPrincipal : IPrincipal
    {
        /// <summary>
        /// 由于 DataportalContext 使用了 Principel，所以每次都传输 OEAIdentity 对象，
        /// 内部的数据要尽量简单，不可以序列化此字段。
        /// </summary>
        [NonSerialized]
        private OEAIdentity _realIdentity;

        private IIdentity _identity;

        private OEAPrincipal(IIdentity identity, OEAIdentity realIdentity)
        {
            this._identity = identity;
            this._realIdentity = realIdentity;
        }

        public IIdentity Identity
        {
            get
            {
                if (this._realIdentity == null)
                {
                    var id = (this._identity as IdentityIdCache).UserId;
                    this._realIdentity = OEAIdentity.GetIdentity(id);
                }

                return this._realIdentity;
            }
        }

        /// <summary>
        /// Returns a value indicating whether the
        /// user is in a given role.
        /// </summary>
        /// <param name="role">Name of the role.</param>
        public virtual bool IsInRole(string role)
        {
            return false;
        }

        public static bool Login(string username, string password)
        {
            var identity = OEAIdentity.GetIdentity(username, password);
            if (identity.IsAuthenticated)
            {
                ApplicationContext.User = new OEAPrincipal(new IdentityIdCache()
                {
                    UserId = identity.User.Id
                }, identity);
                return true;
            }
            else
            {
                ApplicationContext.User = new GenericPrincipal(new AnonymousIdentity(), null);
                return false;
            }
        }

        public static void Logout()
        {
            ApplicationContext.User = new GenericPrincipal(new AnonymousIdentity(), null);
        }

        [Serializable]
        private class IdentityIdCache : IIdentity
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
