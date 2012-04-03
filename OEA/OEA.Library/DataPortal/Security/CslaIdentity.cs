using System;
using System.Linq.Expressions;
using System.Security.Principal;

using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Reflection;
using OEA.Core;
using OEA.Reflection;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.Security
{
    /// <summary>
    /// Provides a base class to simplify creation of
    /// a .NET identity object for use with BusinessPrincipalBase.
    /// </summary>
    [Serializable()]
    public class CslaIdentity : ManagedPropertyObject, IIdentity
    {
        private static readonly ManagedProperty<string> AuthenticationTypeProperty = RegisterProperty<CslaIdentity, string>(e => e.AuthenticationType, "OEA");
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
    }
}