using System;
using System.Security.Principal;


using OEA;
using OEA.ManagedProperty;
using System.Runtime.Serialization;

namespace SimpleCsla.Security
{
    /// <summary>
    /// Base class from which custom principal
    /// objects should inherit to operate
    /// properly with the data portal.
    /// </summary>
    [Serializable]
    public class CslaPrincipal : IPrincipal//GenericPrincipal
    {
        private IIdentity _identity;

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public CslaPrincipal() { _identity = new CslaIdentity(); }

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        /// <param name="identity">Identity object for the user.</param>
        public CslaPrincipal(IIdentity identity)
        {
            _identity = identity;
        }

        /// <summary>
        /// Returns the user's identity object.
        /// </summary>
        public virtual IIdentity Identity
        {
            get { return _identity; }
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
    }
}
