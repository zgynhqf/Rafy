using System;
using System.Security.Principal;
using SimpleCsla.Serialization;
using System.Collections.Generic;
using SimpleCsla.Core.FieldManager;
using System.Runtime.Serialization;
using SimpleCsla.Core;

namespace SimpleCsla.Security
{
    /// <summary>
    /// Implementation of a .NET identity object representing
    /// an unauthenticated user. Used by the
    /// UnauthenticatedPrincipal class.
    /// </summary>
    [Serializable()]
    public sealed class UnauthenticatedIdentity : CslaIdentity
    {
        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public UnauthenticatedIdentity()
        {
            IsAuthenticated = false;
            Name = string.Empty;
            AuthenticationType = string.Empty;
            Roles = new List<string>();
        }
    }
}
