using System;
using System.Linq.Expressions;
using System.Security.Principal;
using SimpleCsla.Serialization;
using System.Collections.Generic;
using SimpleCsla.Core.FieldManager;
using System.Runtime.Serialization;
using System.Reflection;
using SimpleCsla.Core;
using SimpleCsla.Reflection;
using OEA.ManagedProperty;
using OEA.Library;

namespace SimpleCsla.Security
{
    /// <summary>
    /// Provides a base class to simplify creation of
    /// a .NET identity object for use with BusinessPrincipalBase.
    /// </summary>
    [Serializable()]
    public abstract partial class CslaIdentity : ManagedPropertyObject, IIdentity, ICheckRoles
    {
        #region UnauthenticatedIdentity

        /// <summary>
        /// Creates an instance of the class.
        /// </summary>
        /// <returns></returns>
        public static CslaIdentity UnauthenticatedIdentity()
        {
            return new SimpleCsla.Security.UnauthenticatedIdentity();
        }

        #endregion

        #region  IsInRole

        private static readonly ManagedProperty<List<string>> RolesProperty = RegisterProperty<CslaIdentity, List<string>>(e => e.Roles, null);
        /// <summary>
        /// Gets or sets the list of roles for this user.
        /// </summary>
        protected List<string> Roles
        {
            get { return GetProperty(RolesProperty); }
            set { LoadProperty(RolesProperty, value); }
        }

        bool ICheckRoles.IsInRole(string role)
        {
            var roles = GetProperty(RolesProperty);
            if (roles != null)
                return roles.Contains(role);
            else
                return false;
        }

        #endregion

        #region  IIdentity

        private static readonly ManagedProperty<string> AuthenticationTypeProperty = RegisterProperty<CslaIdentity, string>(e => e.AuthenticationType, "SimpleCsla");
        /// <summary>
        /// Gets the authentication type for this identity.
        /// </summary>
        public string AuthenticationType
        {
            get { return GetProperty<string>(AuthenticationTypeProperty); }
            protected set { LoadProperty<string>(AuthenticationTypeProperty, value); }
        }

        private static readonly ManagedProperty<bool> IsAuthenticatedProperty = RegisterProperty<CslaIdentity, bool>(e => e.IsAuthenticated, false);
        /// <summary>
        /// Gets a value indicating whether this identity represents
        /// an authenticated user.
        /// </summary>
        public bool IsAuthenticated
        {
            get { return GetProperty<bool>(IsAuthenticatedProperty); }
            protected set { LoadProperty<bool>(IsAuthenticatedProperty, value); }
        }

        private static readonly ManagedProperty<string> NameProperty = RegisterProperty<CslaIdentity, string>(e => e.Name, string.Empty);
        /// <summary>
        /// Gets the username value.
        /// </summary>
        public string Name
        {
            get { return GetProperty<string>(NameProperty); }
            protected set { LoadProperty<string>(NameProperty, value); }
        }

        #endregion
    }
}