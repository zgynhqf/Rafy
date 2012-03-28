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
    /// Provides a base class to simplify creation of
    /// a .NET identity object for use with BusinessPrincipalBase.
    /// </summary>
    public abstract partial class CslaIdentity
    {
        #region Constructor

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        protected CslaIdentity() { }

        #endregion

        /// <summary>
        /// Invokes the data portal to get an instance of
        /// the identity object.
        /// </summary>
        /// <typeparam name="T">
        /// Type of the CslaIdentity subclass to retrieve.
        /// </typeparam>
        /// <param name="criteria">
        /// Object containing the user's credentials.
        /// </param>
        /// <returns></returns>
        public static T GetCslaIdentity<T>(object criteria) where T : CslaIdentity
        {
            return DataPortal.Fetch<T>(criteria);
        }
    }
}
