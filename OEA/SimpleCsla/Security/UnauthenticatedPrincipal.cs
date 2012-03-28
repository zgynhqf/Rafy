using System;
using System.Security.Principal;
using SimpleCsla.Serialization;
using System.Runtime.Serialization;
namespace SimpleCsla.Security
{
    /// <summary>
    /// Implementation of a .NET principal object that represents
    /// an unauthenticated user. Contains an UnauthenticatedIdentity
    /// object.
    /// </summary>
    [Serializable]
    public sealed class UnauthenticatedPrincipal : BusinessPrincipalBase
    {
        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public UnauthenticatedPrincipal() : base(new UnauthenticatedIdentity()) { }

        /// <summary>
        /// Returns a value indicating whether the user is in the
        /// specified role.
        /// </summary>
        /// <param name="role">Role name.</param>
        /// <returns></returns>
        public override bool IsInRole(string role)
        {
            if (SimpleCsla.DataPortal.IsInDesignMode)
                return true;
            else
                return base.IsInRole(role);
        }
    }
}
